using ColourClashNet.Color;
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
    public class TileBase
    {
        static readonly string sC =nameof(TileBase);
        public enum EnumColorReductionMode
        {
            Fast,
            Detailed,
        }

        public enum EnumErrorSourceMode
        {
            TrasformationError,
            ExternalImageError,
        }

        /// <summary>
        /// Color distance model
        /// </summary>
        public ColorDistanceEvaluationMode ColorDistanceMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        /// <summary>
        /// Tile Width
        /// </summary>
        public int TileW { get; internal set; } = 8;

        /// <summary>
        /// Tile Height
        /// </summary>
        public int TileH { get; internal set; } = 8;

        /// <summary>
        /// Max color allowable in tiles
        /// </summary>
        public int TileMaxColors { get; internal set; } = 2;

        /// <summary>
        /// Tile datasource
        /// </summary>
        public int[,]? DataSource { get; private set; } = null;

        /// <summary>
        /// Original position in source
        /// </summary>
        public int DataSourceOriginC { get; private set; } = 0;

        /// <summary>
        /// Original position in source
        /// </summary>
        public int DataSourceOriginR { get; private set; } = 0;

        /// <summary>
        /// Tile data processed
        /// </summary>
        public int[,]? DataProcessed { get; private set; } = null;

        /// <summary>
        /// Color error evaluation between source and destination tile
        /// </summary>
        public double TrasformationError { get; private set; } = Double.MaxValue;

        /// <summary>
        /// Color error evaluation between source and destination tile
        /// </summary>
        public double ExternalImageError { get; private set; } = Double.MaxValue;

        /// <summary>
        /// Source colors
        /// </summary>
        public Palette ColorPalette { get; set; } = new Palette();

        /// <summary>
        /// Prioritary colors
        /// </summary>
        public Palette ForcedColorPalette { get; set; } = new Palette();

        public EnumColorReductionMode ColorReductionMode { get; internal set; } =  EnumColorReductionMode.Fast;


        int[,]? GetDataTile(int[,]? oDataSource, int iSourceR, int iSourceC, int iTileW, int iTileH )
        {
            if (oDataSource == null)
            {
                return null;
            }
            if (iTileW <= 0 || iTileH <= 0 )
            {
                return null;
            }

            var oDataTile = new int[iTileH, iTileW];
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            int CC = Math.Max(0, Math.Min(C, iSourceC + iTileW));
            int RR = Math.Max(0, Math.Min(R, iSourceR + iTileH));
            // Get tile data
            for (int sr = iSourceR, r = 0; sr < RR; sr++, r++)
            {
                for (int sc = iSourceC, c = 0; sc < CC; sc++, c++)
                {
                    oDataTile[r, c] = oDataSource[sr, sc];
                }
            }
            return oDataTile;
        }

        ColorTransformResults CreateColorTransformResultsRerror(string sMessage, Exception ex)
        { 
            return new ColorTransformResults()
            {
                 Message = sMessage,
                 Exception = ex
            };
        }

        public async Task<ColorTransformResults> CreateAsync(int[,]? oDataSource, int iSourceR, int iSourceC, CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                string sM = nameof(CreateAsync);
                try
                {
                    DataSourceOriginC = iSourceC;
                    DataSourceOriginR = iSourceR;
                    if (oDataSource == null)
                    {
                        LogMan.Error(sC, sM, "Datasource null");
                        return new ColorTransformResults();
                    }
                    if (TileW <= 0 || TileH <= 0 || TileMaxColors <= 0)
                    {
                        LogMan.Error(sC, sM, "Invalid Tile Data");
                        return new ColorTransformResults();
                    }
                    DataSource = GetDataTile(oDataSource, iSourceR, iSourceC, TileW, TileH);
                    return ColorTransformResults.CreateValidResult();
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sC, sM, ex);
                    return ColorTransformResults.CreateErrorResult(ex);
                }
            });
        }

        /// <summary>
        /// Create a int[TileW,TileH] Tile and process it 
        /// </summary>
        /// <param name="oDataSource">Input source data</param>
        /// <param name="iSourceR">Start row in input source data</param>
        /// <param name="iSourceC">Start column in input source data</param>
        /// <returns>int[,] processed data or null on error</returns>
        public async Task<ColorTransformResults> ProcessAsync(CancellationToken? oToken )
        {
            string sM = nameof(ProcessAsync);

            // Process colors
            ColorTransformInterface oColorReduction;

            switch (ColorReductionMode)
            {
                default:
                case EnumColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionFast();
                    }
                    break;
                case EnumColorReductionMode.Detailed:
                    {
                        oColorReduction = new ColorTransformReductionCluster();
                    }
                    break;
            }

            oColorReduction
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, ColorPalette)
                .SetProperty(ColorTransformProperties.MaxColorsWanted, TileMaxColors)
                .SetProperty(ColorTransformProperties.UseColorMean, false)
                .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 5);

            await oColorReduction.CreateAsync(DataSource, oToken);
            var res = await oColorReduction.ProcessColorsAsync( oToken);
            if (res.Valid)
            {
                DataProcessed = res.DataOut;
            }

            // Evaluate error
            res.DataError = await ColorIntExt.EvaluateErrorAsync(DataSource, DataProcessed, ColorDistanceMode,oToken);
            return res;
        }

        /// <summary>
        /// Copy tile processed data on destination data
        /// <para>No data will be copied on error</para>
        /// </summary>
        /// <param name="oDestinationData">Data to be overweritten</param>
        public async Task<bool> MergeDataAsync(int[,]? oDestinationData, CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                if (oDestinationData == null)
                {
                    return false;
                }
                if (DataProcessed == null)
                {
                    return false;
                }
                int R = oDestinationData.GetLength(0);
                int C = oDestinationData.GetLength(1);
                int RR = Math.Max(0, Math.Min(R, DataSourceOriginR + TileH));
                int CC = Math.Max(0, Math.Min(C, DataSourceOriginC + TileW));

                // Merge Data
                Parallel.For(DataSourceOriginR, RR, dr =>//  int dr = DataSourceOriginR, r = 0; dr < RR; dr++, r++)
                {
                    int r = dr - DataSourceOriginR;
                    for (int dc = DataSourceOriginC, c = 0; dc < CC; dc++, c++)
                    {
                        oDestinationData[dr, dc] = DataProcessed[r, c];
                    }
                });
                return true;
            });
        }

        /// <summary>
        /// Copy the best tile (lower color error) on destination data
        /// </summary>
        /// <param name="oDestinationData"></param>
        /// <param name="oTileA"></param>
        /// <param name="oTileB"></param>
        /// <returns>true if data</returns>
        public static async Task<bool> MergeDataAsync(int[,]? oDestinationData, TileBase oTileA, TileBase oTileB, EnumErrorSourceMode eErrorMode, CancellationToken? oToken )
        {
            if (oDestinationData == null)
            {
                return false;
            }
            if (oTileA == null && oTileB == null)
            {
                return false;
            }
            if (oTileA != null && oTileB == null)
            {
                return await oTileA.MergeDataAsync(oDestinationData,oToken);
            }
            if (oTileA == null && oTileB != null)
            {
                return await oTileB.MergeDataAsync(oDestinationData, oToken);
            }
            double dErrorA=0, dErrorB=0;
            switch (eErrorMode)
            {
                case EnumErrorSourceMode.TrasformationError:
                    {
                        dErrorA = oTileA.TrasformationError;
                        dErrorB = oTileB.TrasformationError;
                    }
                    break;
                case EnumErrorSourceMode.ExternalImageError:
                    {
                        dErrorA = oTileA.ExternalImageError;
                        dErrorB = oTileB.ExternalImageError;
                    }
                    break;
                default:
                    break;
            }
            if (dErrorA <= dErrorB)
            {
                //Trace.TraceInformation("A");
                return await oTileA.MergeDataAsync(oDestinationData, oToken);
            }
            else
            {
                //Trace.TraceInformation("B");
                return await oTileB.MergeDataAsync(oDestinationData, oToken);
            }
        }

        /// <summary>
        /// Copy the best tile (lower color error) on destination data
        /// </summary>
        /// <param name="oDestinationData"></param>
        /// <param name="lTiles">List to tiles to compare</param>
        /// <returns></returns>
        public static async Task<bool> MergeDataAsync(int[,]? oDestinationData, List<TileBase> lTiles, EnumErrorSourceMode eErrorMode, CancellationToken? oToken)
        {
            if (oDestinationData == null)
            {
                return false;
            }
            if (lTiles == null || lTiles.Count == 0)
            {
                return false;
            }
            switch (eErrorMode)
            {
                case EnumErrorSourceMode.TrasformationError:
                    {
                        var oTile = lTiles.Where(X => X.TrasformationError == lTiles.Min(Y => Y.TrasformationError)).FirstOrDefault();
                        if (oTile == null)
                        {
                            return false;
                        }
                        return await oTile.MergeDataAsync(oDestinationData, oToken);
                    }
                    break;
                case EnumErrorSourceMode.ExternalImageError:
                    {
                        var oTile = lTiles.Where(X => X.ExternalImageError == lTiles.Min(Y => Y.ExternalImageError)).FirstOrDefault();
                        if (oTile == null)
                        {
                            return false;
                        }
                        return await oTile.MergeDataAsync(oDestinationData, oToken);
                    }
                    break;
                default:
                    {
                        return false;
                    }
            }
        }

        public async Task<bool> CalcExternalImageErrorAsync(int[,]? oDestinationData, CancellationToken? oToken)
        {
            var oDataTile = GetDataTile(oDestinationData, DataSourceOriginR, DataSourceOriginC, TileW, TileH);
            if (oDataTile == null)
            {
                ExternalImageError = double.MaxValue;
            }
            ExternalImageError = await ColorIntExt.EvaluateErrorAsync(oDataTile, DataProcessed, ColorDistanceMode, oToken  );
            return true;
        }

        public override string ToString()
        {
            return $"R={DataSourceOriginR}:C={DataSourceOriginC}:H={TileH}:W={TileW} : TE={TrasformationError} : IE={ExternalImageError}";
        }
    }
}
