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
    /// <summary>
    /// Raster (scanline) color reduction, emulating palette changes during the video beam (e.g. Amiga copper).
    /// <para/>
    /// The image is split into horizontal chunks of <see cref="ChunkHeight"/> lines, each with its own palette:
    /// <see cref="ScanlinePaletteMode.IndependentChunkPalette"/> reduces each chunk independently to <see cref="MaxColorsPerChunk"/> colors;
    /// <see cref="ScanlinePaletteMode.SharedChunksPalette"/> first reduces the whole image to <see cref="MaxColorsWanted"/> colors,
    /// then picks at most <see cref="MaxColorsPerChunk"/> of them for each chunk. Dithering is always bypassed.
    /// </summary>
    public class ColorTransformScanLine : ColorTransformBase
    {
        static string sC = nameof(ColorTransformScanLine);

        /// <summary>
        /// How the chunk palettes are computed.
        /// </summary>
        public enum ScanlinePaletteMode
        {
            /// <summary>
            /// Each chunk has an independent palette.
            /// </summary>
            IndependentChunkPalette,
            /// <summary>
            /// Chunk palettes are subsets of a global palette.
            /// </summary>
            SharedChunksPalette
        }

        /// <summary>
        /// Creates the scanline reduction.
        /// </summary>
        public ColorTransformScanLine()
        {
            Type = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }

        /// <summary>
        /// Tile manager of the last run: one tile per chunk.
        /// </summary>
        public TileManager? TileManager { get; private set; } = null;

        /// <summary>
        /// Number of lines per chunk.
        /// </summary>
        public int ChunkHeight
        {
            get => config.ScanlineChunkHeight;
            set => config.ScanlineChunkHeight = value;
        }
        /// <summary>
        /// Chunk palette mode.
        /// </summary>
        public ScanlinePaletteMode PaletteMode
        {
            get => config.ScanlinePaletteMode;
            set => config.ScanlinePaletteMode = value;
        }
        /// <summary>
        /// Size of the global palette in shared mode.
        /// </summary>
        public int MaxColorsWanted
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        /// <summary>
        /// Maximum number of colors per chunk.
        /// </summary>
        public int MaxColorsPerChunk
        {
            get => config.ScanlineMaxColorsPerChunk;
            set => config.ScanlineMaxColorsPerChunk = value;
        }
        /// <summary>
        /// Reduction used for the global palette in shared mode.
        /// </summary>
        public ColorTransformType InternalTransformationModel 
        { 
            get => config.ScanlineInternalTransformationModel;
            set => config.ScanlineInternalTransformationModel = value;
        }




        /// <summary>
        /// Palette of each row (reserved for export, currently not filled).
        /// </summary>
        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        /// <summary>
        /// Palette change mask of each row (reserved for export, currently not filled).
        /// </summary>
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();

        /// <summary>
        /// Sets the processing parameters (fluent).
        /// </summary>
        public ColorTransformScanLine WithProcessingParams(int chunkHeight, ScanlinePaletteMode scanlinePaletteMode, int maxColorsWanted, int maxColorsPerChunk, ColorTransformType internalTransformationModel )
        {
            ChunkHeight=chunkHeight;
            PaletteMode = scanlinePaletteMode;
            MaxColorsWanted = maxColorsWanted;
            MaxColorsPerChunk = maxColorsPerChunk;
            InternalTransformationModel = internalTransformationModel;
            return this;
        }


        /// <summary>
        /// Sets the processing parameters from a configuration (fluent).
        /// </summary>
        public ColorTransformScanLine WithProcessingParams(ColorTransformConfig cfg) 
            => WithProcessingParams(cfg.ScanlineChunkHeight, cfg.ScanlinePaletteMode, cfg.MaxColorsWanted, cfg.ScanlineMaxColorsPerChunk, cfg.ScanlineInternalTransformationModel);


        /// <summary>
        /// Dispatches to the independent or shared palette processing.
        /// </summary>
        /// <exception cref="NotImplementedException">Unknown <see cref="PaletteMode"/>.</exception>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            String sM = nameof(ExecuteTransform);
            BypassDithering = true;
            ColorListRow.Clear();

            switch (PaletteMode)
            {
                case ScanlinePaletteMode.IndependentChunkPalette:
                    return ExecuteTransformIndependentPalette(token);
                case ScanlinePaletteMode.SharedChunksPalette:
                    return ExecuteTransformSharedPalette(token);
                default:
                    throw new NotImplementedException($"{sC}.{sM} : ReductionModePalette {PaletteMode} not implemented");
            }
        }

        /// <summary>
        /// Reduces every chunk independently.
        /// </summary>
        ColorTransformResult ExecuteTransformIndependentPalette(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformIndependentPalette);
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk).WithReferencePalette(ReferencePalette);
            TileManager = new TileManager()
                .Create(ImageSource.Width, ChunkHeight, ImageSource, 1.0, cfg, token);
            var tileResult = TileManager.ProcessColors(token);
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

        /// <summary>
        /// Reduces the image to the global palette, then reduces every chunk within it.
        /// </summary>
        ColorTransformResult ExecuteTransformSharedPalette(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);
            var mainImage = ImageSource;
            var mainPalette = mainImage.ColorPalette;
            if (mainPalette.Count > MaxColorsWanted)
            {
                // Reduce the whole image first: its palette becomes the shared palette of all chunks
                var transform = ColorTransformInternal.Alloc(config,config.ScanlineInternalTransformationModel).Create(ImageSource);
                var result = transform.ProcessColors(token);
                mainImage = result.DataOut;
            }
            ImageData.AssertValid(mainImage);
            var cfg = config.Clone()
                .WithMaxColorWanted(MaxColorsPerChunk)
                .WithReferencePalette(new(), true);
            TileManager = new TileManager()
                .Create(mainImage.Width, ChunkHeight, mainImage, 1.0, cfg, token);
            var tileResult = TileManager.ProcessColors(token);
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
