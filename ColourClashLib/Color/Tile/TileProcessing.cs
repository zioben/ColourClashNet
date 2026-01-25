using ColourClashNet.Color.Tile;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileProcessing 
{
    static readonly string sC = nameof(TileProcessing);

    object locker = new object();

    public bool IsValid
    {
        get
        {
            if (tileItem?.IsValid ?? false)
            {
                return false;
            }
            if(transformation == null)
            {
                return false;
            }
            return true;
        }
    } 

    protected TileItem? tileItem =  null;

    protected ColorTransformInterface? transformation = null;

    public int OriginRow => tileItem?.OriginY ?? 0; 
    public int OriginColoumn => tileItem?.OriginX ?? 0; 
    public int TileW => tileItem?.TileW ?? 0;       
    public int TileH => tileItem?.TileH ?? 0;   

    
    double transformationError = double.NaN;

    public double TransformationError => !double.IsNaN(transformationError) ? transformationError : transformation?.TransformationError ?? double.NaN;

    public ImageData? TileSourceImage => tileItem?.TileImage;
    public ImageData? TileProcessedImage => transformation?.OutputData;
    public ColorDistanceEvaluationMode ColorDistanceEvaluationMode => transformation?.ColorDistanceEvaluationMode ?? ColorDistanceEvaluationMode.RGB;

    public void Reset()
    {
        lock (locker)
        {
            tileItem = null;
            transformation = null;
            transformationError = double.NaN;
        }
    }

    public TileProcessing Create(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight, ColorTransformType colorTransformType, Dictionary<ColorTransformProperties, object> colorTransformParams)
    {
        lock (locker)
        {
            Reset();
            tileItem = TileItem.CreateTileItem(sourceImage, sourceX, sourceY, tileWidth, tileHeight);
            transformation = ColorTransformBase.CreateColorTransformInterface(colorTransformType, colorTransformParams );
            return this;
        }
    }

    public TileProcessing ProcessTile(CancellationToken token = default)
    {
        string sM = nameof(ProcessTile);
        lock (locker)
        {
            if (!IsValid)
            {
                LogMan.Error(sC, sM, "No valid data");
                return this;
            }
            transformation?.Create(tileItem.TileImage);
            var result = transformation?.ProcessColors(token) ?? ColorTransformResults.CreateErrorResult("Invalid processing");
            if (!result.IsSuccess)
            {
                LogMan.Error(sC, sM, $"Tile transformation failed: {result.Message}");
            }
            return this;
        }
    }

    /// <summary>
    /// Copy tile processed data on destination data
    /// <para>No data will be copied on error</para>
    /// </summary>
    /// <param name="oDestinationData">Data to be overweritten</param>
    internal bool MergeData(int[,] mergeMatrix)
    {
        string sM = nameof(MergeData);
        lock (locker)
        {
            if (mergeMatrix == null)
            {
                LogMan.Error(sC, sM, "Merge matrix null");
                return false;
            }
            if (!transformation?.OutputData?.IsValid ?? true)
            {
                LogMan.Error(sC, sM, "No tile processed image");
                return false;
            }
            return MatrixTools.Blit(transformation.OutputData.matrix, mergeMatrix, 0, 0, tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
        }
    }

    public double RecalculateTransformationError(ImageData rferenceImage, ColorDistanceEvaluationMode colorDistanceEvaluationMode)
    {
        string sM = nameof(RecalculateTransformationError);
        lock (locker)
        {
            if (!tileItem?.IsValid ?? true)
            {
                LogMan.Error(sC, sM, "No valid tile item");
                transformationError = double.NaN;
            }
            if (!transformation?.OutputData?.IsValid ?? true)
            {
                LogMan.Error(sC, sM, "No valid transformed image");
                transformationError = double.NaN;
            }
            var refTile = ImageData.CreateImageData(rferenceImage, tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
            transformationError = ColorIntExt.EvaluateError(refTile, TileProcessedImage, colorDistanceEvaluationMode);
            return transformationError;
        }
    }

    public double RecalculateTransformationError(ImageData rferenceImage)
        => RecalculateTransformationError(rferenceImage, ColorDistanceEvaluationMode);

    public override string ToString()
    {
        return $"R={OriginRow}:C={OriginColoumn}:H={TileH}:W={TileW} : TE={TransformationError}";
    }

}

