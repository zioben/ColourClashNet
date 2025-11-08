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
    public class TileBase : ColorTransformReductionCluster
    {
        static readonly string sC =nameof(TileBase);
        public enum EnumColorReductionMode
        {
            Fast,
            Detailed,
        }

        //public enum EnumErrorSourceMode
        //{
        //    TrasformationError,
        //    ExternalImageError,
        //}

        /// <summary>
        /// Tile Width
        /// </summary>
        public int TileW { get; internal set; } = 8;

        /// <summary>
        /// Tile Height
        /// </summary>
        public int TileH { get; internal set; } = 8;

        /// <summary>
        /// Original position in source
        /// </summary>
        public int DataSourceOriginC { get; private set; } = 0;

        /// <summary>
        /// Original position in source
        /// </summary>
        public int DataSourceOriginR { get; private set; } = 0;


        ///// <summary>
        ///// Color error evaluation between source and destination tile
        ///// </summary>
        //public double ExternalImageError { get; private set; } = Double.MaxValue;

        /// <summary>
        /// Prioritary colors
        /// </summary>
        public Palette ForcedColorPalette { get; set; } = new Palette();

        int[,]? CreateTileData(int[,]? oDataSource, int iSourceR, int iSourceC, int iTileW, int iTileH )
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

        [Obsolete("Not supported in Tile class", true)]
        public new async Task<ColorTransformResults> CreateAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            throw new NotSupportedException("Not valid on Tile Class");
        }


        public async Task<ColorTransformResults> CreateAsync(int[,]? oDataSource, int iSourceR, int iSourceC, CancellationToken? oToken)
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
                if (TileW <= 0 || TileH <= 0 || ColorsMaxWanted <= 0)
                {
                    LogMan.Error(sC, sM, "Invalid Tile Data");
                    return new ColorTransformResults();
                }
                var oTileData = CreateTileData(oDataSource, iSourceR, iSourceC, TileW, TileH);
                await base.CreateAsync( oTileData, oToken);
                return ColorTransformResults.CreateValidResult();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return ColorTransformResults.CreateErrorResult(ex);
            }
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
                if (OutputData == null)
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
                        oDestinationData[dr, dc] = OutputData[r, c];
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
        public static async Task<bool> MergeDataAsync(int[,]? oDestinationData, TileBase oTileA, TileBase oTileB, CancellationToken? oToken )
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
            //double dErrorA=0, dErrorB=0;
            double dErrorA = oTileA.TransformationError;
            double dErrorB = oTileB.TransformationError;
            if (dErrorA <= dErrorB)
            {
                return await oTileA.MergeDataAsync(oDestinationData, oToken);
            }
            else
            { 
                return await oTileB.MergeDataAsync(oDestinationData, oToken);
            }
        }

        /// <summary>
        /// Copy the best tile (lower color error) on destination data
        /// </summary>
        /// <param name="oDestinationData"></param>
        /// <param name="lTiles">List to tiles to compare</param>
        /// <returns></returns>
        public static async Task<bool> MergeDataAsync(int[,]? oDestinationData, List<TileBase> lTiles, CancellationToken? oToken)
        {
            if (oDestinationData == null)
            {
                return false;
            }
            if (lTiles == null || lTiles.Count == 0)
            {
                return false;
            }
            var oTile = lTiles.Where(X => X.TransformationError == lTiles.Min(Y => Y.TransformationError)).FirstOrDefault();
            if (oTile == null)
            {
                return false;
            }
            return await oTile.MergeDataAsync(oDestinationData, oToken);
        }

        public override string ToString()
        {
            return $"R={DataSourceOriginR}:C={DataSourceOriginC}:H={TileH}:W={TileW} : TE={TransformationError}";
        }
    }
}
