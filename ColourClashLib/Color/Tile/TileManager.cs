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

    TileItem[,]? TileData { get; set; }
    public int TileW { get; private set; } = 8;
    public int TileH { get; private set; } = 8;
    public int TileRR { get; private set; } = 0;
    public int TileCC { get; private set; } = 0;

    public Palette ColorPalette { get; set; } = new Palette();
    public Palette ForcedColorPalette { get; set; } = new Palette();

    public bool TileBorderShow { get; set; } = true;
    public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

    public ImageData SourceData { get; private set; }
    ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;

    public int MaxColorsWanted { get; private set; } = 2;

    public ColorDithering DitheringType { get; set; }
    public double DitheringStrenght { get; set; } = 1.0;

    public double TransformationError { get; set; } = double.NaN;

    public TileItem? GetTileData(int r, int c)
    {
        if (r >= 0 && r < TileRR && c >= 0 && c < TileCC)
        {
            return TileData?[r, c];
        }
        return null;
    }


    public TileManager SetProperty(ColorTransformProperties eProperty, object oValue)
    {
        switch (eProperty)
        {
            case ColorTransformProperties.ColorDistanceEvaluationMode:
                if (Enum.TryParse<ColorDistanceEvaluationMode>(oValue?.ToString(), out var eMode))
                {
                    ColorDistanceEvaluationMode = eMode;
                }
                break;
            case ColorTransformProperties.Fixed_Palette:
                {
                    if (oValue is IEnumerable<int> oPalette)
                    {
                        ColorPalette = Palette.CreatePalette(oPalette);
                    }
                    else if (oValue is Palette oPal)
                    {
                        ColorPalette = Palette.CreatePalette(oPal);
                    }
                    else
                    {
                        ColorPalette = new Palette();
                    }
                }
                break;
            case ColorTransformProperties.Forced_Palette:
                {
                    if (oValue is IEnumerable<int> oPalette)
                    {
                        ForcedColorPalette = Palette.CreatePalette(oPalette);
                    }
                    else if (oValue is Palette oPal)
                    {
                        ForcedColorPalette = oPal;
                    }
                    else
                    {
                        ForcedColorPalette = new Palette();
                    }
                }
                break;
            case ColorTransformProperties.Dithering_Type:
                {
                    DitheringType = ColorDithering.None;
                    if (Enum.TryParse<ColorDithering>(oValue?.ToString(), true, out var eRes))
                    {
                        DitheringType = eRes;
                    }
                }
                break;
            case ColorTransformProperties.Dithering_Strength:
                {
                    DitheringStrenght = 1.0;
                    if (double.TryParse(oValue?.ToString(), out var dStrenght))
                    {
                        DitheringStrenght = dStrenght;
                    }
                }
                break;
            case ColorTransformProperties.MaxColorsWanted:
                {
                    MaxColorsWanted = 0;
                    if (int.TryParse(oValue.ToString(), out var iColors))
                    {
                        MaxColorsWanted = iColors;
                    }
                }
                break;
            default:
                break;
        }
        return this;
    }




    public async Task<TileManager> CreateAsync(ImageData oDataSource, CancellationToken oToken)
    {
        return await Task.Run(() =>
        {
            string sM = nameof(CreateAsync);
            if (oDataSource == null)
            {
                LogMan.Error(sC, sM, "Source data is null");
                return this;
            }
            if (TileW <= 0 || TileH <= 0 || MaxColorsWanted <= 0)
            {
                LogMan.Error(sC, sM, "Tile dimensions or max colors wanted are invalid");
                return this;
            }
            SourceData = new ImageData().Create(oDataSource);
            if (SourceData == null)
            {
                LogMan.Error(sC, sM, "Failed to copy source data");
                return this;
            }
            if (ColorPalette == null)
                ColorPalette = new();
            if (ForcedColorPalette == null)
                ForcedColorPalette = new();

            TileRR = (SourceData.Rows + TileH - 1) / TileH;
            TileCC = (SourceData.Columns + TileW - 1) / TileW;
            TileData = new TileItem[TileRR, TileCC];

            Parallel.For(0, TileRR, r =>
            {
                for (int c = 0; c < TileCC; c++)
                {
                    oToken.ThrowIfCancellationRequested();
                    int rr = r;
                    int cc = c;
                    TileData[rr, cc] = new TileItem()
                    {
                        TileW = TileW,
                        TileH = TileH
                    };
                    TileData[rr, cc]
                     .Create(oDataSource, rr * TileH, cc * TileW)
                     .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                     .SetProperty(ColorTransformProperties.Fixed_Palette, ForcedColorPalette)
                     .SetProperty(ColorTransformProperties.MaxColorsWanted, MaxColorsWanted)
                     .SetProperty(ColorTransformProperties.UseColorMean, false)
                     .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10)
                     .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
                     .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);
                }
            });
            return this;
        });
    }



    public async Task<bool> ProcessColorsAsync(CancellationToken oToken)
    {
        TransformationError = double.NaN;

        if (SourceData == null)
        {
            return false;
        }
        if (TileW == 0 || TileH == 0)
        {
            return false;
        }
        if (TileData == null)
        {
            return false;
        }

        int RT = TileData.GetLength(0);
        int CT = TileData.GetLength(1);

        for (int r = 0; r < RT; r++)
        {
            oToken.ThrowIfCancellationRequested();
            await Parallel.ForAsync(0, CT, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async (c, oToken) =>
            {
                int rr = r;
                int cc = c;
                await TileData[rr, cc].ProcessColorsAsync(oToken);
            });
        }

        RecalcTransformationError();
        return true;
    }

    public double RecalcTransformationError()
    {
        if (TileData == null)
        {
            TransformationError = double.NaN;
        }
        else
        {
            TransformationError = 0;
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    TransformationError += TileData[r, c].TransformationError;
                }
            }
        }
        return TransformationError;
    }

    public async Task<double> EvaluateImageErrorAsync(ImageData oDataReference, CancellationToken? oToken)
    {
        TransformationError = double.NaN;
        if (TileData == null || oDataReference == null)
        {
            return TransformationError;
        }
        int RT = TileData.GetLength(0);
        int CT = TileData.GetLength(1);
        // Merge Data
        var tasks = new List<Task<double>>();
        await Parallel.ForAsync(0, RT, async (r, oToken) =>
        {
            int row = r * TileH;
            for (int c = 0; c < CT; c++)
            {
                int col = c * TileW;
                var oTileRefData = TileItem.CreateTileData(oDataReference, row, col, TileW, TileH);
                await TileData[r, c].RecalcTransformationErrorAsync(oTileRefData, oToken);
            }
        });
        return RecalcTransformationError();
    }

    public ImageData? CreateImageFromTiles()
    {
        string sM = nameof(CreateImageFromTiles);
        if (!SourceData?.Valid ?? true)
        {
            LogMan.Error(sC, sM, "invalid source data");
            return null;
        }
        if (TileW == 0 || TileH == 0 || TileData == null)
        {
            LogMan.Error(sC, sM, "invalid tile data");
            return null;
        }
        int RT = TileData.GetLength(0);
        int CT = TileData.GetLength(1);
        var oRet = new int[SourceData.Rows, SourceData.Columns];

        for (int r = 0; r < RT; r++)
        {
            Parallel.For(0, CT, c =>
            {
                int rr = r;
                int cc = c;
                TileData[rr, cc].MergeData(oRet);
            });
        }

        if (TileBorderShow && TileBorderColor >= 0)
        {
            for (int r = 0; r < RT; r++)
            {
                int y = r * TileH;
                for (int c = 0; c < CT; c++)
                {
                    int x = c * TileW;
                    oRet[y, x] = TileBorderColor;
                }
            }
        }
        return new ImageData().Create(oRet);
    }



}
