using ColourClashNet.Color;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
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

namespace ColourClashNet.Color.Tile
{
    public class TileManager
    {
        static string sClass = nameof(TileManager);

        TileBase[,]? TileData { get; set; }
        public int TileW { get; private set; } = 8;
        public int TileH { get; private set; } = 8;
        public int TileR { get; private set; } = 0;
        public int TileC { get; private set; } = 0;

        public Palette ColorPalette { get; set; } = new Palette();
        public Palette ForcedColorPalette { get; set; } = new Palette();

        public bool TileBorderShow { get; set; } = true;
        public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

        public int[,]? SourceData { get; private set; }
        ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        public int MaxColorsWanted { get; private set; } = 2;

        public ColorDithering DitheringType { get; set; }
        public double DitheringStrenght { get; set; } = 1.0;

        public double TransformationError { get; set; } = double.NaN;

        public TileBase? GetTileData(int r, int c)
        {
            if (r >= 0 && r < TileR && c >= 0 && c < TileC)
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
                        if (oValue is List<int> oPalette)
                        {
                            ColorPalette = Palette.CreateColorPalette(oPalette);
                        }
                        else if (oValue is Palette oPal)
                        {
                            ColorPalette = oPal;
                        }
                        else
                        {
                            ColorPalette = new Palette();
                        }
                    }
                    break;
                case ColorTransformProperties.Forced_Palette:
                    {
                        if (oValue is List<int> oPalette)
                        {
                            ForcedColorPalette = Palette.CreateColorPalette(oPalette);
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




        public async Task<bool> CreateAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            if (oDataSource == null)
            {
                return false;
            }
            if (TileW <= 0 || TileH <= 0 || MaxColorsWanted <= 0)
            {
                return false;
            }
            SourceData = oDataSource.Clone() as int[,];
            if (SourceData == null)
            {
                return false;
            }
            if (ColorPalette == null)
                ColorPalette = new();
            if (ForcedColorPalette == null)
                ForcedColorPalette = new();

            TileR = (SourceData.GetLength(0) + TileH - 1) / TileH;
            TileC = (SourceData.GetLength(1) + TileW - 1) / TileW;
            TileData = new TileBase[TileR, TileC];

            for (int r = 0; r < TileR; r++)
            {
                oToken?.ThrowIfCancellationRequested();
                await Parallel.ForAsync(0, TileC, async (c, oToken) =>
                {
                    int rr = r;
                    int cc = c;
                    TileData[rr, cc] = new TileBase()
                    {
                        TileW = TileW,
                        TileH = TileH
                    };
                    TileData[rr, cc]
                     .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                     .SetProperty(ColorTransformProperties.Fixed_Palette, ForcedColorPalette)// ColorPalette)
                     //.SetProperty(ColorTransformProperties.Forced_Palette, ForcedColorPalette)// ColorPalette)
                     .SetProperty(ColorTransformProperties.MaxColorsWanted, MaxColorsWanted)
                     .SetProperty(ColorTransformProperties.UseColorMean, false)
                     .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10)
                     .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
                     .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);
                    await TileData[rr, cc].CreateAsync(oDataSource, rr * TileH, cc * TileW, oToken);
                });
            }

            return true;
        }

        

        public async Task<bool> ProcessColorsAsync(CancellationToken? oToken)
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
                oToken?.ThrowIfCancellationRequested();
                await Parallel.ForAsync(0, CT,new ParallelOptions() { MaxDegreeOfParallelism=1}, async (c, oToken) =>
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

        public async Task<double> EvaluateImageErrorAsync(int[,] oDataReference, CancellationToken? oToken)
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
                    var oTileRefData = TileBase.CreateTileData(oDataReference, row, col, TileW, TileH);
                    await TileData[r, c].RecalcTransformationErrorAsync(oTileRefData, oToken);
                }
            });
            return RecalcTransformationError();
        }

        public async Task<int[,]?> CreateImageFromTilesAsync(CancellationToken? oToken)
        {
            if (SourceData == null)
            {
                return null;
            }
            if (TileW == 0 || TileH == 0)
            {
                return null;
            }
            if (TileData == null)
            {
                return null;
            }
            var R = SourceData.GetLength(0);
            var C = SourceData.GetLength(1);
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            var oRet = new int[R, C];

            for (int r = 0; r < RT; r++)
            {
                oToken?.ThrowIfCancellationRequested();
                await Parallel.ForAsync(0, CT, async (c, oToken) =>
                {
                    int rr = r;
                    int cc = c;
                    await TileData[rr, cc].MergeDataAsync(oRet, oToken);
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
            return oRet;
        }



        public static async Task<int[,]?> MergeDataAsync(int R, int C, List<TileManager?> lTileMan, CancellationToken? oToken)
        {
            if (lTileMan == null || lTileMan.Count == 0)
                return null;
            var lTileManF = lTileMan.Where(X => X != null).ToList();
            if (lTileManF == null || lTileManF.Count == 0)
                return null;

            var oRet = new int[R, C];

            var RT = lTileManF.Min(X => X.TileData?.GetLength(0) ?? 0);
            var CT = lTileManF.Min(X => X.TileData?.GetLength(1) ?? 0);
            var lTiles = new List<TileBase>();
            List<Task> tasks = new List<Task>();
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    int rr = r;
                    int cc = c;
                    lTiles.Clear();
                    lTileManF.ForEach(X => lTiles.Add(X.GetTileData(rr, cc)));
                    tasks.Add(TileBase.MergeDataAsync(oRet, lTiles, oToken));
                }
            }
            await Task.WhenAll(tasks);
            return oRet;
        }
        public static async Task<int[,]?> MergeDataAsync(int[,] oDataRef, List<TileManager?> lTileMan, CancellationToken? oToken)
        { 
            if( oDataRef == null ) 
                return null;
            return await MergeDataAsync(oDataRef.GetLength(0), oDataRef.GetLength(1), lTileMan, oToken);
        }

        public static async Task<int[,]?> MergeDataAsync(int[,]? oDataRef, TileManager? oTileA, TileManager? oTileB, CancellationToken oToken)
        {
            if (oTileA == null && oTileB == null)
                return null;

            return await MergeDataAsync(oDataRef, new List<TileManager?> { oTileA, oTileB }, oToken );
        }

        public static async Task<int[,]?> MergeDataAsync(int R, int C, TileManager? oTileA, TileManager? oTileB, CancellationToken oToken)
        {
            if (oTileA == null && oTileB == null)
                return null;
            return await MergeDataAsync(R,C, new List<TileManager?> { oTileA, oTileB }, oToken);
        }

        public static TileManager Create(int iTileW, int iTileH, int iMaxColorWanted)
        {
            return new TileManager()
            {
                TileW = iTileW,
                TileH = iTileH,
                MaxColorsWanted = iMaxColorWanted
            };
        }
    }
}
