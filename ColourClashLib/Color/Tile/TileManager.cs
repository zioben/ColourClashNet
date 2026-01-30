using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileManager
{
    static string sC = nameof(TileManager);

    object locker = new object();

    TileProcessing[,]? tileProcessingMatrix;
    public int TileW { get; private set; } = 8;
    public int TileH { get; private set; } = 8;

    public int TileRows => tileProcessingMatrix?.GetLength(0) ?? 0;
    public int TileColumns => tileProcessingMatrix?.GetLength(1) ?? 0;
    public bool TileBorderShow { get; set; } = false;
    public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

    public ImageData ImageSource { get; private set; } = new ImageData();

    public ColorTransformType ProcessingType { get; private set; } = ColorTransformType.ColorReductionClustering;
    public Dictionary<ColorTransformProperties, object> ProcessingParameters { get; private set; } = new Dictionary<ColorTransformProperties, object>();
    public double GlobalTransformationError { get; set; } = double.NaN;

    public double NormalizationError { get; private set; } = 1.0;
    public bool IsValid
    {
        get
        {
            lock (locker)
            {
                return tileProcessingMatrix != null &&
                    ImageSource.IsValid &&
                    TileW > 0 &&
                    TileH > 0 &&
                    TileRows > 0 &&
                    TileColumns > 0;
            }
        }
    }

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

    public void Reset()
    {
        lock (locker)
        {
            tileProcessingMatrix = null;
            ImageSource = new ImageData();
            GlobalTransformationError = double.NaN;
            ProcessingType = ColorTransformType.ColorReductionClustering;
            ProcessingParameters = new Dictionary<ColorTransformProperties, object>();
            NormalizationError = 1.0;
        }
    }

    public TileManager Create(int tileWidth, int tileHeight, ImageData image, double normalizationError, ColorTransformType processingType, Dictionary<ColorTransformProperties, object> processingParameters , CancellationToken oToken=default)
    {

        string sM = nameof(Create);
        if( image == null )
            throw new ArgumentNullException(nameof(image));
        if( !image.IsValid)
            throw new ArgumentException(nameof(image)); 
        if (tileWidth <= 0 || tileHeight <= 0)
            throw new ArgumentOutOfRangeException($"Tile {TileW}x{TileH}");
        lock (locker)
        {
            try
            { 
                Reset();
                TileW = tileWidth;
                TileH = tileHeight;
                ProcessingType = processingType;
                NormalizationError = normalizationError;

                ImageSource = new ImageData().Create(image);
                tileProcessingMatrix = new TileProcessing[(ImageSource.Height) / TileH, (ImageSource.Width) / TileW];

                //Parallel.For(0, TileRows, r =>
                for (int r = 0; r < TileRows; r++)
                {
                    for (int c = 0; c < TileColumns; c++)
                    {
                        oToken.ThrowIfCancellationRequested();
                        int ys = r * TileH;
                        int xs = c * TileW;
                        tileProcessingMatrix[r, c] = new TileProcessing().Create(
                            ImageSource,
                            xs,
                            ys,
                            TileW,
                            TileH,
                            NormalizationError,
                            ProcessingType,
                            processingParameters);
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



    public bool ProcessColors(CancellationToken token = default)
    {
        string sM = nameof(ProcessColors);  
        GlobalTransformationError = double.NaN;
        try
        {
            if (!IsValid)
                throw new InvalidDataException(nameof(ProcessColors));
            //Parallel.For(0, TileRows, r =>
            for (int r = 0; r < TileRows; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < TileColumns; c++)
                {
                    int rr = r;
                    int cc = c;
                    token.ThrowIfCancellationRequested();
                    tileProcessingMatrix[rr, cc].ProcessTile(token);
                };
            }//);
            var dError = RecalcGlobalTransformationError();
            return true;
        }
        catch (OperationCanceledException ex)
        {
            LogMan.Exception(sC, sM, "Operation cancelled",ex);
            return false;
        }
    }


    public double RecalcGlobalTransformationError(ImageData referenceImage,  CancellationToken token=default)
    {
        string sM = nameof(RecalcGlobalTransformationError);
        try
        {
            lock (locker)
            {
                if (!IsValid)
                {
                    LogMan.Error(sC, sM, "invalid data");
                    GlobalTransformationError = double.NaN;
                    return GlobalTransformationError;
                }
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

    public double RecalcGlobalTransformationError(CancellationToken token = default) => RecalcGlobalTransformationError(ImageSource, token);

    public ImageData CreateImageFromTiles()
    {
        string sM = nameof(CreateImageFromTiles);
        if (!IsValid)
            throw new InvalidDataException(nameof(IsValid));

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
