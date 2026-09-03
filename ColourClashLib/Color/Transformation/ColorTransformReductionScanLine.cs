using ColourClashNet.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color.Dithering;
using System.Globalization;
using ColourClashNet.Color.Tile;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionScanLine : ColorTransformBase
    {
        static string sC = nameof(ColorTransformReductionScanLine);

        public enum ScanlineReductionMode
        {
            IndependentPalettePerLine,
            SharedPaletteForAllLines
        }

        public ColorTransformReductionScanLine()
        {
            Type = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }

        // -------------------------------------------------------------
        // Ideas:
        // Input image is divided in horizontal chunks of N lines (default 1 line)
        // Without shared palette:
        // Each chunk is processed to reduce the number of colors to a maximum of indipendent MaxColorsPerChunk colors (default 8)
        // With shared palette
        // Image is color reduced to MaxColorsWanted colors (default 16) with a single palette for all chunks 
        // Every chunk is processed to reduce the number of colors to a maximum MaxColorsPerChunk (default 8) with palette selected from the shared palette

        public TileManager? TileManager { get; private set; } = null;

        public int ChunkHeight
        {
            get => config.ChunkHeight;
            set => config.ChunkHeight = value;
        }
        public ScanlineReductionMode ReductionModePalette
        {
            get => config.ScanlineReductionMode;
            set => config.ScanlineReductionMode = value;
        }
        public int MaxColorsWanted
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        public int MaxColorsPerChunk
        {
            get => config.MaxColorsPerChunk;
            set => config.MaxColorsPerChunk = value;
        }
        public ColorTransformType InternalTransformationModel 
        { 
            get => config.InternalTransformationModel;
            set => config.InternalTransformationModel = value;
        }

        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();

        public ColorTransformReductionScanLine WithProcessingParams(int chunkHeight, ScanlineReductionMode scanlineReductionMode, int maxColorsWanted, int maxColorsPerChunk, ColorTransformType internalTransformationModel)
        {
            ChunkHeight=chunkHeight;
            ReductionModePalette = scanlineReductionMode;
            MaxColorsWanted = maxColorsWanted;
            MaxColorsPerChunk = maxColorsPerChunk;
            InternalTransformationModel = internalTransformationModel;
            return this;
        }


        public ColorTransformReductionScanLine WithReductionScanLine(ColorTransformConfig cfg) 
            => WithProcessingParams(cfg.ChunkHeight, cfg.ScanlineReductionMode, cfg.MaxColorsWanted, cfg.MaxColorsPerChunk, cfg.InternalTransformationModel);


        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken = default)
        {
            String sM = nameof(ExecuteTransform);
            BypassDithering = true;
            ColorListRow.Clear();

            var oRet = new int[ImageSource.Rows, ImageSource.Columns];
            //var oCols = new int[1, C];
            var oSourceNew = new Imaging.ImageData().Create(ImageSource);

            var oLineFixedPalette = ReferencePalette;
            // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
            // MainPaletteUsed = false;
            switch (ReductionModePalette)
            {
                case ScanlineReductionMode.IndependentPalettePerLine:
                    return ExecuteTransformIndipendentPalette(oToken);
                case ScanlineReductionMode.SharedPaletteForAllLines:
                    return ExecuteTransformSharedPalette(oToken);
                default:
                    throw new NotImplementedException($"{sC}.{sM} : ReductionModePalette {ReductionModePalette} not implemented");
            }
        }

        ColorTransformResult ExecuteTransformIndipendentPalette(CancellationToken oToken = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);
            TileManager = new TileManager();
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk).WithReferencePalette(new Palette());
            TileManager.Create(ImageSource.Width, ChunkHeight, ImageSource, 1.0, cfg, oToken);
            var tileResult = TileManager.ProcessColors(oToken);
            if (tileResult.IsSuccess)
            {
                ImageOutput = TileManager.MergeToImage(new List<TileManager?>() { TileManager });
                return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult(ImageSource, ImageSource, $"{sC}.{sM} : Tile processing error : {tileResult.Message}");
            }
        }

        ColorTransformResult ExecuteTransformSharedPalette(CancellationToken oToken = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);  
            var oMainImage = ImageSource;
            var oMainPalette = oMainImage.ColorPalette;
            if (oMainPalette.Count > MaxColorsWanted)
            {
                var transf = ColorTransformInternal.Alloc(config).Create(ImageSource);
                var result = transf.ProcessColors(oToken);
                oMainImage = result.DataOut;
            }
            ImageData.AssertValid(oMainImage);
            WithReferencePalette(oMainImage.ColorPalette);
            TileManager = new TileManager();
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk);
            TileManager.Create(oMainImage.Width, ChunkHeight, oMainImage, 1.0, cfg, oToken);
            var tileResult = TileManager.ProcessColors(oToken);
            if (tileResult.IsSuccess)
            { 
                ImageOutput = TileManager.MergeToImage(new List<TileManager?>() { TileManager });
                return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult(ImageSource, ImageSource, $"{sC}.{sM} : Tile processing error : {tileResult.Message}");
            }
        }
    }
}
