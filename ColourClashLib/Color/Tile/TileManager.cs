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
        }
    }

    public TileManager Create(int tileWidth, int tileHeight, ImageData image, ColorTransformType processingType, Dictionary<ColorTransformProperties, object> processingParameters , CancellationToken oToken=default)
    {

        string sM = nameof(Create);
        lock (locker)
        {
            try
            {

                Reset();
                TileW = tileWidth;
                TileH = tileHeight;
                if (TileW <= 0 || TileH <= 0)
                {
                    LogMan.Error(sC, sM, "Tile dimensions or max colors wanted are invalid");
                    Reset();
                    return this;
                }
                ProcessingType = processingType;
            
                ImageSource = new ImageData().Create(image);
                if (!ImageSource.IsValid)
                {
                    LogMan.Error(sC, sM, "Source image data is invalid");
                    Reset();
                    return this;
                }
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
                            ProcessingType,
                            processingParameters);
                    }
                }//);
                return this;
            }
            catch (OperationCanceledException)
            {
                LogMan.Warning(sC, sM, "Operation cancelled");
                Reset();
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Exception creating TileManager", ex);
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
            {
                LogMan.Error(sC, sM, "TileManager is not valid");
                return false;
            }
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
        catch (OperationCanceledException)
        {
            LogMan.Warning(sC, sM, "Operation cancelled");
            return false;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, "Exception processing colors", ex);
            return false;
        }
    }

    public double RecalcGlobalTransformationError()
    {
        string sM = nameof(RecalcGlobalTransformationError);
        try
        {
            lock (locker)
            {
                GlobalTransformationError = 0;
                if (!IsValid)
                {
                    LogMan.Error(sC, sM, "invalid data");
                    GlobalTransformationError = double.NaN;
                    return GlobalTransformationError;
                }
                for (int r = 0; r < TileRows; r++)
                {
                    for (int c = 0; c < TileColumns; c++)
                    {
                        var err = tileProcessingMatrix[r, c].TransformationError;
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
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, "Exception recalculating global transformation error", ex);
            GlobalTransformationError = double.NaN;
        }
        return GlobalTransformationError;
    }

    public double RecalcGlobalTransformationError(ImageData referenceImage, CancellationToken token = default)
    {
        string sM = nameof(RecalcGlobalTransformationError);
        try
        {
            lock (locker)
            {
                GlobalTransformationError = 0;
                if (!IsValid)
                {
                    LogMan.Error(sC, sM, "invalid data");
                    GlobalTransformationError = double.NaN;
                    return GlobalTransformationError;
                }
                for (int r = 0; r < TileRows; r++)
                {
                    for (int c = 0; c < TileColumns; c++)
                    {
                        token.ThrowIfCancellationRequested();
                        var err = tileProcessingMatrix[r, c].RecalculateTransformationError(referenceImage);
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
            }
        }
        catch (OperationCanceledException)
        {
            LogMan.Warning(sC, sM, "Operation cancelled");
            GlobalTransformationError = double.NaN;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, "Exception recalculating global transformation error", ex);
            GlobalTransformationError = double.NaN;
        }
        return GlobalTransformationError;
    }
       

    public ImageData? CreateImageFromTiles()
    {
        string sM = nameof(CreateImageFromTiles);
        if (!IsValid)
        {
            LogMan.Error(sC, sM, "TileManager is not valid");
            return null;
        }
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
