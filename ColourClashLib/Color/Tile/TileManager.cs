using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

/// <summary>
/// Splits an image into a grid of tiles and color-reduces each tile independently.
/// <para/>
/// Used to emulate attribute based video modes (ZX Spectrum, C64, CPC ...) where every
/// character cell can only use a limited set of colors.
/// </summary>
public partial class TileManager
{
    static string sC = nameof(TileManager);

    object locker = new object();

    TileProcessing[,]? tileProcessingMatrix;

    /// <summary>
    /// Raised with the progress of the tile processing: one event per processed tile row,
    /// plus the events forwarded from the tile transformations (throttled, see <see cref="TileEventInterval"/>).
    /// </summary>
    public event EventHandler<ColorProcessingEventArgs>? ProcessPartial;

    /// <summary>
    /// Minimum interval between two forwarded tile transformation events.
    /// <para/>
    /// Every tile runs its own transformation (e.g. 1000 tiles x 10 clustering loops for a C64 image):
    /// forwarding all of them would flood the UI, so they are throttled. Tile row progress events are never throttled.
    /// </summary>
    public TimeSpan TileEventInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    long lastTileEventTicks = 0;

    /// <summary>
    /// Forwards a tile transformation event, unless another one was forwarded less than <see cref="TileEventInterval"/> ago.
    /// </summary>
    void ForwardTileEvent(object? sender, ColorProcessingEventArgs args)
    {
        if (ProcessPartial == null)
            return;
        long now = Environment.TickCount64;
        long last = Interlocked.Read(ref lastTileEventTicks);
        if (now - last < (long)TileEventInterval.TotalMilliseconds)
            return;
        // Only one of concurrent callers wins the slot
        if (Interlocked.CompareExchange(ref lastTileEventTicks, now, last) != last)
            return;
        RaiseProcessPartialEvent(sender, args);
    }

    /// <summary>
    /// Raises <see cref="ProcessPartial"/> with a copy of <paramref name="args"/>.
    /// </summary>
    void RaiseProcessPartialEvent(object? sender, ColorProcessingEventArgs args)
    {
        if (ProcessPartial == null || args == null)
            return;
        ProcessPartial.Invoke(this, new ColorProcessingEventArgs()
        {
            ColorTransformInterface = args.ColorTransformInterface,
            ProcessingResults = args.ProcessingResults,
            CancellationTokenSource = args.CancellationTokenSource,
            TransformationMap = args.TransformationMap,
            CompletedPercent = args.CompletedPercent,
        });
    }

    /// <summary>Tile width in pixels.</summary>
    public int TileW { get; private set; } = 8;

    /// <summary>Tile height in pixels.</summary>
    public int TileH { get; private set; } = 8;

    /// <summary>Number of tile rows.</summary>
    public int TileRows => tileProcessingMatrix?.GetLength(0) ?? 0;

    /// <summary>Number of tile columns.</summary>
    public int TileColumns => tileProcessingMatrix?.GetLength(1) ?? 0;

    /// <summary>When true, <see cref="CreateImageFromTiles"/> marks the top-left pixel of each tile.</summary>
    public bool TileBorderShow { get; set; } = false;

    /// <summary>Color used to mark tile borders.</summary>
    public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

    /// <summary>Copy of the image being tiled.</summary>
    public ImageData ImageSource { get; private set; } = new ImageData();

    /// <summary>Transformation settings applied to each tile.</summary>
    public TransformOptions TileOptions { get; private set; } = new ClusteringOptions();

    /// <summary>Sum of the transformation errors of all tiles; NaN when not computed.</summary>
    public double GlobalTransformationError { get; set; } = double.NaN;

    /// <summary>Divider applied to each tile error to make different transformations comparable.</summary>
    public double NormalizationError { get; private set; } = 1.0;

    /// <summary>
    /// Returns the tile at grid position (<paramref name="r"/>, <paramref name="c"/>), or null if out of range.
    /// </summary>
    public TileProcessing? GetTileProcessing(int r, int c)
    {
        lock (locker)
        {
            if (r >= 0 && r < TileRows && c >= 0 && c < TileColumns)
            {
                return tileProcessingMatrix?[r, c] ?? null;
            }
            return null;
        }
    }

    /// <summary>
    /// Clears tiles, source image and errors.
    /// </summary>
    public void Reset()
    {
        lock (locker)
        {
            tileProcessingMatrix = null;
            ImageSource = new ImageData();
            GlobalTransformationError = double.NaN;
            TileOptions = new ClusteringOptions();
            NormalizationError = 1.0;
        }
    }


    /// <summary>
    /// Splits <paramref name="image"/> into tiles of <paramref name="tileWidth"/> x <paramref name="tileHeight"/> pixels.
    /// Edge tiles may be smaller.
    /// </summary>
    /// <param name="tileWidth">Tile width in pixels</param>
    /// <param name="tileHeight">Tile height in pixels</param>
    /// <param name="image">Source image</param>
    /// <param name="normalizationError">Divider applied to each tile error</param>
    /// <param name="tileOptions">Options of the transformation applied to every tile (usually <see cref="ClusteringOptions"/>)</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>This instance; reset when cancelled.</returns>
    /// <exception cref="InvalidConstraintException">Invalid tile size.</exception>
    public TileManager Create(int tileWidth, int tileHeight, ImageData image, double normalizationError, TransformOptions tileOptions, CancellationToken token=default)
    {

        string sM = nameof(Create);

        ImageData.AssertValid(image);

        if (tileWidth <= 0 || tileHeight <= 0)
            throw new InvalidConstraintException($"{sC}.{sM} : tile size {tileWidth}x{tileHeight} invalid");

        lock (locker)
        {
            try
            { 
                Reset();
                TileW = tileWidth;
                TileH = tileHeight;
                NormalizationError = normalizationError;

                TileOptions = tileOptions ?? throw new ArgumentNullException(nameof(tileOptions));
                ImageSource = new ImageData().Create(image);
                // Round up: partial tiles on the right/bottom edges must be processed too
                tileProcessingMatrix = new TileProcessing[(ImageSource.Height + TileH - 1) / TileH, (ImageSource.Width + TileW - 1) / TileW];

                //Parallel.For(0, TileRows, r =>
                for (int r = 0; r < TileRows; r++)
                {
                    for (int c = 0; c < TileColumns; c++)
                    {
                        token.ThrowIfCancellationRequested();
                        int ys = r * TileH;
                        int xs = c * TileW;
                        tileProcessingMatrix[r, c] = new TileProcessing().Create(
                            ImageSource,
                            xs,
                            ys,
                            Math.Min(TileW, ImageSource.Width - xs),
                            Math.Min(TileH, ImageSource.Height - ys),
                            NormalizationError,
                            tileOptions);
                        tileProcessingMatrix[r, c].ProcessPartial += ForwardTileEvent;
                    }
                }//);
                return this;
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, "Operation cancelled", ex);
                Reset();
                return this;
            }
        }
    }

    /// <summary>
    /// Configures the tile border marking (fluent).
    /// </summary>
    public TileManager WithTileBorder( bool tileBorderShow, int tileBorderColor)
    {
        TileBorderShow = tileBorderShow;
        TileBorderColor = tileBorderColor;
        return this;
    }

    /// <summary>
    /// Color-reduces every tile, then recalculates <see cref="GlobalTransformationError"/>.
    /// </summary>
    /// <returns>A valid result, or an error result for the first failing tile or on cancellation.</returns>
    public ColorTransformResult ProcessColors(CancellationToken token = default)
    {
        string sM = nameof(ProcessColors);  
        GlobalTransformationError = double.NaN;
        try
        {
            TileManager.AssertValid(this);
            //Parallel.For(0, TileRows, r =>
            for (int r = 0; r < TileRows; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < TileColumns; c++)
                {
                    int rr = r;
                    int cc = c;
                    token.ThrowIfCancellationRequested();
                    var tileResult = tileProcessingMatrix[rr, cc].ProcessTile(token);
                    if (tileResult == null || !tileResult.IsSuccess)
                    {
                        LogMan.Error(sC, sM, $"Tile ({rr},{cc}) processing failed : {tileResult?.Message}");
                        return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"tile ({rr},{cc}) processing failed", tileResult);
                    }
                }
                // Tile row completed: report the overall progress
                RaiseProcessPartialEvent(this, new ColorProcessingEventArgs()
                {
                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, null, $"Tile row {r + 1}/{TileRows} processed"),
                    CompletedPercent = 100.0 * (r + 1) / TileRows,
                });
            }//);
            var error = RecalcGlobalTransformationError();
            return ColorTransformResult.CreateValidResult();
        }
        catch (OperationCanceledException ex)
        {
            LogMan.Exception(sC, sM, "Operation cancelled",ex);
            return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, "operation cancelled", ex);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, "Tile processing failed", ex);
            return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, "tile processing failed", ex);
        }
    }


    /// <summary>
    /// Recalculates the sum of the tile errors against <paramref name="referenceImage"/>.
    /// </summary>
    /// <returns>The global error, or NaN on error or cancellation.</returns>
    public double RecalcGlobalTransformationError(ImageData referenceImage,  CancellationToken token=default)
    {
        string sM = nameof(RecalcGlobalTransformationError);
        try
        {
            lock (locker)
            {
                AssertValid(this);
                GlobalTransformationError = 0;
                for (int r = 0; r < TileRows; r++)
                {
                    for (int c = 0; c < TileColumns; c++)
                    {
                        token.ThrowIfCancellationRequested();
                        var err = tileProcessingMatrix[r, c].RecalculateTransformationError(referenceImage,token);
                        if (double.IsNaN(err))
                        {
                            LogMan.Error(sC, sM, $"invalid transformation error in tile ({r},{c})");
                            throw new Exception("invalid transformation error");
                        }
                        else
                        {
                            GlobalTransformationError += err;
                        }
                    }
                }
                return GlobalTransformationError;
            }
        }
        catch (OperationCanceledException)
        {
            LogMan.Warning(sC, sM, "Operation cancelled");
            GlobalTransformationError = double.NaN;
            return GlobalTransformationError;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, "Exception recalculating global transformation error", ex);
            GlobalTransformationError = double.NaN;
            return GlobalTransformationError;
        }
    }

    /// <summary>
    /// Recalculates the sum of the tile errors against <see cref="ImageSource"/>.
    /// </summary>
    public double RecalcGlobalTransformationError(CancellationToken token = default) => RecalcGlobalTransformationError(ImageSource, token);

    /// <summary>
    /// Reassembles the processed tiles into a full image, optionally marking tile corners.
    /// </summary>
    public ImageData CreateImageFromTiles()
    {
        string sM = nameof(CreateImageFromTiles);

        AssertValid(this);

        var matrix = new int[ImageSource.Rows, ImageSource.Columns];

        //Parallel.For(0, TileRows, r =>
        for (int r = 0; r < TileRows; r++)
        {
            for (int c = 0; c < TileColumns; c++)
            {
                int rr = r;
                int cc = c;
                tileProcessingMatrix[rr, cc].MergeData(matrix);
            }
        }//);

        if (TileBorderShow && TileBorderColor >= 0)
        {
            for (int r = 0; r < TileRows; r++)
            {
                int y = r * TileH;
                for (int c = 0; c < TileColumns; c++)
                {
                    int x = c * TileW;
                    matrix[y, x] = TileBorderColor;
                }
            }
        }
        return new ImageData().Create(matrix);
    }



}
