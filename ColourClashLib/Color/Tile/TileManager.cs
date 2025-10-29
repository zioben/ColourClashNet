using ColourClashNet.Color;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile
{
    public class TileManager
    {
        static string sClass = nameof(TileManager); 

        TileBase[,] TileData { get; set; }
        public int TileW { get; private set; } = 8;
        public int TileH { get; private set; } = 8;
        public int TileR { get; private set; } = 0;
        public int TileC { get; private set; } = 0;

        public Palette ColorPalette { get; set; } = new Palette();
        public Palette ForcedColorPalette { get; set; } = new Palette();

        int[,] SourceData;

        ColorDistanceEvaluationMode ColorDistanceMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        TileBase.EnumColorReductionMode ColorReductionMode { get; set; } = TileBase.EnumColorReductionMode.Detailed;
        public int MaxColors { get; set; } = 2;

        public TileBase? GetTileData(int r, int c)
        {
            if( r >= 0 && r<TileR && c >= 0 && c < TileC )
            {
                return TileData[r, c];
            }
            return null;
        }

        void Destroy()
        {
            TileData = null;
            SourceData = null;
            TileW = 0;
            TileH = 0;
            TileR = 0;
            TileC = 0;  
            MaxColors = 0;
        }


        public async Task<bool> CreateAsync(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, Palette oColorPalette, Palette oForceColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode, CancellationToken? oToken)
        {
            Destroy();
            if (oDataSource == null)
            {
                return false;
            }
            if (iTileW <= 0 || iTileH <= 0 || iMaxTileColors <= 0)
            {
                return false;
            }
            SourceData = oDataSource.Clone() as int[,];
            TileW = iTileW;
            TileH = iTileH;
            ColorDistanceMode = eColorDistanceMode;
            ColorReductionMode = eColorReductionMode;
            ColorPalette = oColorPalette ?? new Palette();
            ForcedColorPalette = oForceColorPalette ?? new Palette();   
            MaxColors = iMaxTileColors;
            TileR = (SourceData.GetLength(0) + TileH - 1) / TileH;
            TileC = (SourceData.GetLength(1) + TileW - 1) / TileW;
            TileData = new TileBase[TileR, TileC];

            var tasks = new List<Task>();

            for (int r = 0; r < TileR; r++)
            {
                for (int c = 0; c < TileC; c++)
                {
                    int rr = r;
                    int cc = c;
                    tasks.Add(Task.Run(async () =>
                    {
                        TileData[rr, cc] = new TileBase()
                        {
                            TileW = TileW,
                            TileH = TileH,
                            TileMaxColors = MaxColors,
                            ColorDistanceMode = eColorDistanceMode,
                            ColorReductionMode = ColorReductionMode,
                            ColorPalette = ColorPalette,
                            ForcedColorPalette = ForcedColorPalette,
                        };
                        await TileData[rr, cc].CreateAsync(oDataSource, rr * TileH, cc * TileW, oToken);
                    }));
                }
            }

            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> ProcessAsync(CancellationToken? oToken)
        {
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

            var tasks = new List<Task>();

            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    int rr = r;
                    int cc = c;

                    tasks.Add(Task.Run(() =>
                        TileData[rr, cc].ProcessAsync(oToken)));
                }
            }
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<int[,]> CreateImageFromTilesAsync( CancellationToken? oToken)
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

            var tasks = new List<Task>();

            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    int rr = r;
                    int cc = c;
                    tasks.Add(Task.Run(() => TileData[rr, cc].MergeDataAsync(oRet, oToken)));
                }
            }
            await Task.WhenAll(tasks);

            return oRet;
        }

        //public async Task <int[,]?> TransformAndDitherAsync( CancellationToken? oToken)
        //{ 
        //    if( await CreateTilesAsync(oDataSource,oToken) ) 
        //    {
        //        return await CreateImageFromTilesAsync(oDataSource,oToken);
        //    }
        //    return null;
        //}

        public async Task<double> CalcExternalImageErrorAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            if (oDataSource == null)
            {
                return double.NaN;
            }
            if (TileW == 0 || TileH == 0)
            {
                return double.NaN;
            }
            if (TileData == null)
            {
                return double.NaN;
            }
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            var tasks = new List<Task>();

            for( int r =0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    int rr = r;
                    int cc = c;
                    tasks.Add(Task.Run(()=>TileData[rr, cc].CalcExternalImageErrorAsync(oDataSource,oToken)));
                }
            }
            await Task.WhenAll(tasks);
            double dError = 0;
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    dError += TileData[r, c].ExternalImageError;
                }
            }
            return dError;
        }

        public async Task<double> CalcImageErrorAsync( CancellationToken? oToken) => await CalcExternalImageErrorAsync(SourceData,oToken);

        //public static async Task<TileManager?> CreateTileManagerAsync(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, Palette oFixedColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode, CancellationToken? oToken)
        //{
        //    string sMethod = nameof(CreateTileManagerAsync); 
        //    var oRet = new TileManager();
        //    var res = await oRet.InitAsync(oDataSource,iTileW, iTileH, iMaxTileColors, oFixedColorPalette, eColorDistanceMode, eColorReductionMode, oToken);
        //    if (res)
        //    {
        //        return oRet;
        //    }
        //    else
        //    { 
        //        LogMan.Error(sClass, sMethod, "Cannot init TileManager");
        //        return null;
        //    }
        //}

        public static async Task<int[,]?> MergeDataAsync(int[,]? oDataSource, List<TileManager> lTileMan, TileBase.EnumErrorSourceMode eErrorMode, CancellationToken? oToken)
        {
            if (oDataSource == null)
                return null;
            if (lTileMan == null || lTileMan.Count == 0)
                return null;
            var lTileManF = lTileMan.Where(X=>X != null).ToList();
            if (lTileManF == null || lTileManF.Count == 0)
                return null;

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            var oRet = new int[R, C];

            var RT = lTileManF.Min(X => X.TileData.GetLength(0));
            var CT = lTileManF.Min(X => X.TileData.GetLength(1));
            var lTiles = new List<TileBase>();
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    int rr = r;
                    int cc = c;
                    lTiles.Clear();
                    lTileManF.ForEach(X=>lTiles.Add(X.GetTileData(rr, cc)));   
                    await TileBase.MergeDataAsync(oRet, lTiles, eErrorMode, oToken);
                }
            }

            return oRet;
        }


        public static async Task<int[,]?> MergeDataAsync(int[,]? oDataSource, TileManager oTileA, TileManager oTileB, TileBase.EnumErrorSourceMode eErrorMode, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;
            if (oTileA == null && oTileB == null)
                return null;

            return await MergeDataAsync(oDataSource, new List<TileManager> { oTileA, oTileB }, eErrorMode, oToken );
        }
    }
}
