using ColourClashNet.Color.Tile;
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

public partial class TileProcessing 
{
    static readonly string sC = nameof(TileProcessing);

    object locker = new object();

    //public bool IsValid
    //{
    //    get
    //    {
    //        if (!tileItem?.IsValid ?? true)
    //        {
    //            return false;
    //        }
    //        if(transformation == null)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }
    //} 

    protected TileItem? tileItem =  null;

    protected ColorTransformInterface? transformation = null;

    public int OriginRow => tileItem?.OriginY ?? 0; 
    public int OriginColoumn => tileItem?.OriginX ?? 0; 
    public int TileW => tileItem?.TileW ?? 0;       
    public int TileH => tileItem?.TileH ?? 0;
    public double NormalizationError { get; private set; } = 1.0;
    
    double transformationError = double.NaN;

    public double TransformationError => !double.IsNaN(transformationError) ? transformationError : transformation?.TransformationError ?? double.NaN;


    public ImageData? TileSourceImage => tileItem?.TileImage;
    public ImageData? TileProcessedImage => transformation?.ImageOutput;
    public ColorDistanceEvaluationMode ColorDistanceEvaluationMode => transformation?.ColorDistanceEvaluationMode ?? ColorDistanceEvaluationMode.RGB;

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

    public TileProcessing Create(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight, double normalizationError, ColorTransformType colorTransformType, Dictionary<ColorTransformProperties, object> colorTransformParams)
    {
        lock (locker)
        {
            Reset();
            NormalizationError = normalizationError;
            tileItem = new TileItem().Create(sourceImage, sourceX, sourceY, tileWidth, tileHeight);
            transformation = ColorTransformBase.CreateColorTransformInterface(colorTransformType, colorTransformParams );
            return this;
        }
    }

    public ColorTransformResult ProcessTile(CancellationToken token = default)
    {
        string sM = nameof(ProcessTile);
        lock (locker)
        {
            AssertValid(this);
            var result = transformation.Create(tileItem.TileImage).ProcessColors(token) ?? ColorTransformResult.CreateErrorResult("Invalid processing");
            if (!result.IsSuccess)
            {
                LogMan.Error(sC, sM, $"Tile transformation failed: {result.Message}");
            }
            return result;
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
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(mergeMatrix)} is null");
            ColorTransformBase.AssertValid(transformation);
            ImageData.AssertValid(transformation.ImageOutput);

            return MatrixTools.Blit(transformation.ImageOutput.matrix, mergeMatrix, 0, 0, tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
        }
    }

    public double RecalculateTransformationError(ImageData rferenceImage, ColorDistanceEvaluationMode colorDistanceEvaluationMode, CancellationToken token)
    {
        string sM = nameof(RecalculateTransformationError);
        lock (locker)
        {
            AssertValid(this);
            ColorTransformBase.AssertValid(transformation);
            ImageData.AssertValid(transformation.ImageOutput);
            var refTile = rferenceImage.Extract( tileItem.OriginX, tileItem.OriginY, tileItem.TileW, tileItem.TileH);
            transformationError = ColorIntExt.EvaluateError(refTile, TileProcessedImage, colorDistanceEvaluationMode, token);
            return transformationError/NormalizationError;
        }
    }

    public double RecalculateTransformationError(ImageData rferenceImage, CancellationToken token)
        => RecalculateTransformationError(rferenceImage, ColorDistanceEvaluationMode, token );

    public override string ToString()
    {
        return $"R={OriginRow}:C={OriginColoumn}:H={TileH}:W={TileW} : TE={TransformationError}";
    }
}

