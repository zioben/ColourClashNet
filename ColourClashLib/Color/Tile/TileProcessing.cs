using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

/// <summary>
/// A single tile of a <see cref="TileManager"/> grid together with the color transformation that reduces it.
/// </summary>
public partial class TileProcessing 
{
    static readonly string sC = nameof(TileProcessing);

    object locker = new object();

    /// <summary>
    /// Raised with the progress of the tile transformation (forwarded from its
    /// <see cref="ColorTransformInterface.ProcessPartial"/> and <see cref="ColorTransformInterface.ProcessAdvance"/> events).
    /// </summary>
    public event EventHandler<ColorProcessingEventArgs>? ProcessPartial;

    /// <summary>
    /// Raises <see cref="ProcessPartial"/> with a copy of the event data of the tile transformation.
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

    /// <summary>Tile image and its position in the source image.</summary>
    protected TileItem? tileItem =  null;

    /// <summary>Color reduction applied to the tile (clustering by default).</summary>
    protected ColorTransformInterface? transformation = null;

    /// <summary>Row (Y) of the tile origin in the source image.</summary>
    public int OriginRow => tileItem?.OriginY ?? 0; 

    /// <summary>Column (X) of the tile origin in the source image.</summary>
    public int OriginColoumn => tileItem?.OriginX ?? 0; 

    /// <summary>Tile width in pixels.</summary>
    public int TileW => tileItem?.TileW ?? 0;       

    /// <summary>Tile height in pixels.</summary>
    public int TileH => tileItem?.TileH ?? 0;

    /// <summary>Divider applied to the tile error by <see cref="RecalculateTransformationError(ImageData, CancellationToken)"/>.</summary>
    public double NormalizationError { get; private set; } = 1.0;
    
    double transformationError = double.NaN;

    /// <summary>
    /// Last recalculated tile error; falls back to the transformation's own error when not recalculated yet.
    /// </summary>
    public double TransformationError => !double.IsNaN(transformationError) ? transformationError : transformation?.TransformationError ?? double.NaN;

    /// <summary>Original tile image.</summary>
    public ImageData? TileSourceImage => tileItem?.TileImage;

    /// <summary>Color-reduced tile image; null until processed.</summary>
    public ImageData? TileProcessedImage => transformation?.ImageOutput;

    /// <summary>Number of colors of the original tile.</summary>
    public int TileSourceColors => tileItem?.SourceColors ?? 0;

    /// <summary>Number of colors of the processed tile.</summary>
    public int TileProcessedColors => transformation?.ImageOutput?.Colors ?? 0;

    /// <summary>Distance metric used by the tile transformation.</summary>
    public ColorDistanceEvaluationMode ColorDistanceEvaluationMode => transformation?.ColorDistanceEvaluationMode ?? ColorDistanceEvaluationMode.RGB;

    /// <summary>
    /// Clears tile, transformation and errors.
    /// </summary>
    public void Reset()
    {
        lock (locker)
        {
            tileItem = null;
            transformation = null;
            transformationError = double.NaN;
            NormalizationError = 1.0;
        }
    }

    /// <summary>
    /// Extracts the tile from <paramref name="sourceImage"/> and allocates its clustering transformation.
    /// </summary>
    /// <param name="sourceImage">Full source image</param>
    /// <param name="sourceX">Tile left coordinate</param>
    /// <param name="sourceY">Tile top coordinate</param>
    /// <param name="tileWidth">Tile width</param>
    /// <param name="tileHeight">Tile height</param>
    /// <param name="normalizationError">Divider applied to the tile error</param>
    /// <param name="colorTransformConfig">Transformation settings</param>
    /// <returns>This instance.</returns>
    public TileProcessing Create(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight, double normalizationError, ColorTransformConfig colorTransformConfig)
    {
        lock (locker)
        {
            Reset();
            NormalizationError = normalizationError;
            tileItem = new TileItem().Create(sourceImage, sourceX, sourceY, tileWidth, tileHeight);
            transformation = ColorTransformInternal.Alloc(colorTransformConfig, ColorTransformType.ColorReductionClustering);
            // Preview images of a single tile are useless and costly: progress messages are still raised
            if (transformation is ColorTransformBase transformationBase)
                transformationBase.EnablePreviewEvents = false;
            transformation.ProcessPartial += RaiseProcessPartialEvent;
            transformation.ProcessAdvance += RaiseProcessPartialEvent;
            return this;
        }
    }

    /// <summary>
    /// Runs the color reduction on the tile.
    /// </summary>
    public ColorTransformResult ProcessTile(CancellationToken token = default)
    {
        string sM = nameof(ProcessTile);
        lock (locker)
        {
            AssertValid(this);
            var result = transformation.Create(tileItem.TileImage)?.ProcessColors(token);
            if (result == null || !result.IsSuccess)
            {
                LogMan.Error(sC, sM, $"Tile transformation failed: {result?.Message ?? "transformation not created"}");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, tileItem.TileImage, $"tile {this} transformation failed", result);
            }
            return result;
        }
    }

    /// <summary>
    /// Copies the processed tile into <paramref name="mergeMatrix"/> at the tile origin.
    /// <para/>
    /// Nothing is copied on error.
    /// </summary>
    /// <param name="mergeMatrix">Destination matrix, overwritten in the tile area</param>
    /// <returns>True if the tile was copied.</returns>
    internal bool MergeData(int[,] mergeMatrix)
    {
        string sM = nameof(MergeData);
        lock (locker)
        {
            if (mergeMatrix == null)
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(mergeMatrix)} is null");
            ColorTransformBase.AssertValid(transformation);
            ImageData.AssertValid(transformation.ImageOutput);

            return MatrixTools.Blit(transformation.ImageOutput.matrix, mergeMatrix, 0, 0, tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
        }
    }

    /// <summary>
    /// Recalculates the mean color error between the processed tile and the same area of <paramref name="referenceImage"/>.
    /// </summary>
    /// <param name="referenceImage">Full size reference image</param>
    /// <param name="colorDistanceEvaluationMode">Distance metric</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The error divided by <see cref="NormalizationError"/>.</returns>
    public double RecalculateTransformationError(ImageData referenceImage, ColorDistanceEvaluationMode colorDistanceEvaluationMode, CancellationToken token)
    {
        string sM = nameof(RecalculateTransformationError);
        lock (locker)
        {
            AssertValid(this);
            ColorTransformBase.AssertValid(transformation);
            ImageData.AssertValid(transformation.ImageOutput);
            var refTile = referenceImage.Extract( tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
            transformationError = ColorIntExt.EvaluateError(refTile, TileProcessedImage, colorDistanceEvaluationMode, token);
            return transformationError/NormalizationError;
        }
    }

    /// <summary>
    /// Recalculates the tile error using the transformation distance metric.
    /// </summary>
    public double RecalculateTransformationError(ImageData referenceImage, CancellationToken token)
        => RecalculateTransformationError(referenceImage, ColorDistanceEvaluationMode, token );

    public override string ToString()
    {
        return $"R={OriginRow}:C={OriginColoumn}:H={TileH}:W={TileW} : TE={TransformationError}";
    }
}

