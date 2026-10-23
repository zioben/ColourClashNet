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
    public class ColorTransformScanLine : ColorTransformBase
    {
        static string sC = nameof(ColorTransformScanLine);

        public enum ScanlinePaletteMode
        {
            IndependentChunkPalette,
            SharedChunksPalette
        }

        public ColorTransformScanLine()
        {
            Type = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------
        // Rationale:
        // Input image is divided in horizontal chunks of N lines (default 1 line)
        // -Without shared palette:
        //      Each chunk is processed to reduce the number of colors to a maximum of indipendent MaxColorsPerChunk colors (default 8)
        // -With shared palette:
        //      Image is color reduced to MaxColorsWanted colors (default 16) that contains the shared palette for all chunks 
        //      Every chunk is then processed to reduce the number of colors to a maximum MaxColorsPerChunk (default 8) with the shared palette.
        // ---------------------------------------------------------------------------------------------------------------------------------------------

        public TileManager? TileManager { get; private set; } = null;

        public int ChunkHeight
        {
            get => config.ScanlineChunkHeight;
            set => config.ScanlineChunkHeight = value;
        }
        public ScanlinePaletteMode PaletteMode
        {
            get => config.ScanlinePaletteMode;
            set => config.ScanlinePaletteMode = value;
        }
        public int MaxColorsWanted
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        public int MaxColorsPerChunk
        {
            get => config.ScanlineMaxColorsPerChunk;
            set => config.ScanlineMaxColorsPerChunk = value;
        }
        public ColorTransformType InternalTransformationModel 
        { 
            get => config.ScanlineInternalTransformationModel;
            set => config.ScanlineInternalTransformationModel = value;
        }




        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();

        public ColorTransformScanLine WithProcessingParams(int chunkHeight, ScanlinePaletteMode scanlinePaletteMode, int maxColorsWanted, int maxColorsPerChunk, ColorTransformType internalTransformationModel )
        {
            ChunkHeight=chunkHeight;
            PaletteMode = scanlinePaletteMode;
            MaxColorsWanted = maxColorsWanted;
            MaxColorsPerChunk = maxColorsPerChunk;
            InternalTransformationModel = internalTransformationModel;
            return this;
        }


        public ColorTransformScanLine WithProcessingParams(ColorTransformConfig cfg) 
            => WithProcessingParams(cfg.ScanlineChunkHeight, cfg.ScanlinePaletteMode, cfg.MaxColorsWanted, cfg.ScanlineMaxColorsPerChunk, cfg.ScanlineInternalTransformationModel);


        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken = default)
        {
            String sM = nameof(ExecuteTransform);
            BypassDithering = true;
            ColorListRow.Clear();

            var oRet = new int[ImageSource.Rows, ImageSource.Columns];
            var oSourceNew = new Imaging.ImageData().Create(ImageSource);

            var oLineFixedPalette = ReferencePalette;
            switch (PaletteMode)
            {
                case ScanlinePaletteMode.IndependentChunkPalette:
                    return ExecuteTransformIndipendentPalette(oToken);
                case ScanlinePaletteMode.SharedChunksPalette:
                    return ExecuteTransformSharedPalette(oToken);
                default:
                    throw new NotImplementedException($"{sC}.{sM} : ReductionModePalette {PaletteMode} not implemented");
            }
        }

        ColorTransformResult ExecuteTransformIndipendentPalette(CancellationToken oToken = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk).WithReferencePalette(ReferencePalette);
            TileManager = new TileManager()
                .Create(ImageSource.Width, ChunkHeight, ImageSource, 1.0, cfg, oToken);
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
                var transf = ColorTransformInternal.Alloc(config,config.ScanlineInternalTransformationModel).Create(ImageSource);
                var result = transf.ProcessColors(oToken);
                oMainImage = result.DataOut;
            }
            ImageData.AssertValid(oMainImage);
            var cfg = config.Clone()
                .WithMaxColorWanted(MaxColorsPerChunk)
                .WithReferencePalette(new(), true);
            TileManager = new TileManager()
                .Create(oMainImage.Width, ChunkHeight, oMainImage, 1.0, cfg, oToken);
            var tileResult = TileManager.ProcessColors(oToken);
            if (tileResult.IsSuccess)
            { 
                ImageOutput = TileManager.MergeToImage(new List<TileManager?>() { TileManager });
                return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult(ImageSource, null, $"{sC}.{sM} : Tile processing error : {tileResult.Message}");
            }
        }
    }
}
