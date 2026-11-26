using ColourClashNet.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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
    /// The image is split into horizontal chunks of <see cref="ScanlineOptions.ChunkHeight"/> lines, each with its own palette:
    /// <see cref="ScanlinePaletteMode.IndependentChunkPalette"/> reduces each chunk independently with <see cref="ScanlineOptions.ChunkReduction"/>;
    /// <see cref="ScanlinePaletteMode.SharedChunksPalette"/> first reduces the whole image with <see cref="ScanlineOptions.SharedReduction"/>,
    /// then reduces every chunk within that palette. Dithering is always bypassed (each chunk reduction dithers itself).
    /// </summary>
    public class ColorTransformScanLine : ColorTransformBase<ScanlineOptions>
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
            Description = "Raster line color reduction";
        }

        /// <summary>
        /// Tile manager of the last run: one tile per chunk.
        /// </summary>
        public TileManager? TileManager { get; private set; } = null;





        /// <summary>
        /// Palette of each row (reserved for export, currently not filled).
        /// </summary>
        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        /// <summary>
        /// Palette change mask of each row (reserved for export, currently not filled).
        /// </summary>
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();





        /// <summary>
        /// Dispatches to the independent or shared palette processing.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            BypassDithering = true;
            ColorListRow.Clear();
            LogDebug(sC, sM, $"{Type} : {Options.PaletteMode}, chunks of {Options.ChunkHeight} lines, chunk reduction {Options.ChunkReduction.TransformType} to {Options.ChunkReduction.MaxColors} colors");
            try
            {
                switch (Options.PaletteMode)
                {
                    case ScanlinePaletteMode.IndependentChunkPalette:
                        return ExecuteTransformIndependentPalette(token);
                    case ScanlinePaletteMode.SharedChunksPalette:
                        return ExecuteTransformSharedPalette(token);
                    default:
                        LogMan.Error(sC, sM, $"{Type} : palette mode {Options.PaletteMode} not implemented");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : palette mode {Options.PaletteMode} not implemented");
                }
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"{Type} : operation cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : scanline processing failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : scanline processing failed", ex);
            }
        }

        /// <summary>
        /// Reduces every chunk independently.
        /// </summary>
        ColorTransformResult ExecuteTransformIndependentPalette(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformIndependentPalette);
            // Every chunk is reduced independently, keeping the user palette colors
            var cfg = Options.InheritCommon(Options.ChunkReduction);
            return ProcessChunks(sM, ImageSource, cfg, token);
        }

        /// <summary>
        /// Reduces the image to the global palette, then reduces every chunk within it.
        /// </summary>
        ColorTransformResult ExecuteTransformSharedPalette(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);
            var mainImage = ImageSource;
            var mainPalette = mainImage.ColorPalette;
            var sharedOptions = Options.InheritCommon(Options.SharedReduction);
            if (mainPalette.Count > sharedOptions.MaxColors)
            {
                // Reduce the whole image first: its palette becomes the shared palette of all chunks
                LogDebug(sC, sM, $"{Type} : shared palette reduction of {mainPalette.Count} colors to {sharedOptions.MaxColors} with {sharedOptions.TransformType}");
                var transform = ForwardEventsFrom(ColorTransformInternal.Alloc(sharedOptions));
                var result = transform.Process(ImageSource, null, token);
                if (result == null || !result.IsSuccess || result.DataOut == null)
                {
                    LogMan.Error(sC, sM, $"{Type} : shared palette reduction failed");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : shared palette reduction failed", result);
                }
                mainImage = result.DataOut;
            }
            // Chunks pick their colors among the shared palette: the user palette is already part of it
            var cfg = Options.InheritCommon(Options.ChunkReduction) with { UserPalette = Array.Empty<int>() };
            return ProcessChunks(sM, mainImage, cfg, token);
        }

        /// <summary>
        /// Splits <paramref name="image"/> into chunks of <see cref="ScanlineOptions.ChunkHeight"/> lines and reduces each one.
        /// </summary>
        /// <param name="sM">Calling method, used in error messages</param>
        /// <param name="image">Image to split</param>
        /// <param name="cfg">Options of each chunk reduction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A valid result with the merged chunks, or an error result.</returns>
        ColorTransformResult ProcessChunks(string sM, ImageData image, TransformOptions cfg, CancellationToken token)
        {
            TileManager = new TileManager();
            // Subscribe before Create: the chunks forward their transformation events through the manager
            ForwardEventsFrom(TileManager);
            TileManager.Create(image.Width, Options.ChunkHeight, image, 1.0, cfg, token);
            LogTrace(sC, sM, $"{Type} : processing {(image.Height + Options.ChunkHeight - 1) / Options.ChunkHeight} chunks");
            var tileResult = TileManager.ProcessColors(token);
            if (tileResult == null || !tileResult.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : chunk processing failed : {tileResult?.Message}");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, image, $"{Type} : chunk processing failed : {tileResult?.Message}", tileResult?.Exception);
            }
            var outputImage = TileManager.MergeToImage(new List<TileManager?>() { TileManager });
            if (outputImage == null)
            {
                LogMan.Error(sC, sM, $"{Type} : chunk merge failed");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, image, $"{Type} : chunk merge failed");
            }
            return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
        }
    }
}
