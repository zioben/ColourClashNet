using ColourClashLib.Color;
using ColourClashNet.Colors;
using ColourClashNet.Colors.Transformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Colors.Tile
{
    public class TileBase
    {

        public enum EnumColorReductionMode
        {
            Fast,
            Detailed,
        }

        static ColorDistanceEvaluationMode eColorMode = ColorDistanceEvaluationMode.RGB;

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
        public int DataSourceC { get; private set; } = 0;

        /// <summary>
        /// Original position in source
        /// </summary>
        public int DataSourceR { get; private set; } = 0;

        /// <summary>
        /// Tile data processed
        /// </summary>
        public int[,]? DataProcessed { get; private set; } = null;

        /// <summary>
        /// Color error evaluation between source and destination tile
        /// </summary>
        public double Error { get; private set; } = Double.MaxValue;

        /// <summary>
        /// Prioritary colors
        /// </summary>
        public ColorPalette FixedPalette { get; set; } = new ColorPalette();

        public EnumColorReductionMode ColorReductionMode { get; internal set; } =  EnumColorReductionMode.Fast; 

        /// <summary>
        /// Create a int[TileW,TileH] Tile and process it 
        /// </summary>
        /// <param name="oDataSource">Input source data</param>
        /// <param name="iSourceR">Start row in input source data</param>
        /// <param name="iSourceC">Start column in input source data</param>
        /// <returns>int[,] processed data or null on error</returns>
        public int[,]? ExecuteTrasform(int[,]? oDataSource, int iSourceR, int iSourceC )
        {
            DataSource = new int[TileW, TileH];
            DataSourceC = iSourceC; 
            DataSourceR = iSourceR;
            if (oDataSource == null)
            {
                return null;                
            }
            if( TileW <= 0 || TileH <= 0 || TileMaxColors <= 0 )
            {
                return null;
            }

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            int CC = Math.Max(0, Math.Min(C, DataSourceC + TileW));
            int RR = Math.Max(0, Math.Min(R, DataSourceR + TileH));

            Trace.TraceInformation($"R={iSourceR}->{RR}, C={iSourceC}->{CC}");

            // Get tile data
            for (int sr = DataSourceR, r=0; sr<RR; sr++, r++ )
            {
                for (int sc = DataSourceC, c=0 ; sc <CC; sc++, c++)
                {
                    DataSource[r, c] = oDataSource[sr,sc];
                }
            }

            // Process colors
            ColorTransformInterface oColorReduction;

            switch (ColorReductionMode)
            {
                default:
                case EnumColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionFast()
                            {
                                ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
                                ColorsMax = TileMaxColors,
                            };
                    }
                    break;
                case EnumColorReductionMode.Detailed:
                    {
                        oColorReduction = new ColorTransformReductionCluster
                        {
                            TrainingLoop = 6,
                            UseClusterColorMean = false,
                            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
                            ColorsMax = TileMaxColors,
                        };
                    }
                    break;
            }
            oColorReduction.Create(DataSource,FixedPalette);
            DataProcessed = oColorReduction.TransformAndDither(DataSource);

            // Evaluate error
            Error = ColorTransformBase.Error(DataSource, DataProcessed, eColorMode);
            return DataProcessed;
        }

        /// <summary>
        /// Copy tile processed data on destination data
        /// <para>No data will be copied on error</para>
        /// </summary>
        /// <param name="oDestinationData">Data to be overweritten</param>
        public bool MergeData(int[,]? oDestinationData)
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
            int RR = Math.Max(0, Math.Min(R, DataSourceR + TileH));
            int CC = Math.Max(0, Math.Min(C, DataSourceC + TileW));

            // Merge Data
            for (int dr = DataSourceR, r = 0; dr < RR; dr++, r++)
            {
                for (int dc = DataSourceC, c = 0; dc < CC; dc++, c++)
                {
                    oDestinationData[dr, dc] = DataProcessed[r, c];
                }
            }
            return true;
        }

        /// <summary>
        /// Copy the best tile (lower color error) on destination data
        /// </summary>
        /// <param name="oDestinationData"></param>
        /// <param name="oTileA"></param>
        /// <param name="oTileB"></param>
        /// <returns>true if data</returns>
        public static bool MergeData(int[,]? oDestinationData, TileBase oTileA, TileBase oTileB)
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
                return oTileA.MergeData(oDestinationData);
            }
            if (oTileA == null && oTileB != null)
            {
                return oTileB.MergeData(oDestinationData);
            }
            if (oTileA.Error < oTileB.Error)
            {
                return oTileA.MergeData(oDestinationData);
            }
            else
            {
                return oTileB.MergeData(oDestinationData);
            }
        }

        /// <summary>
        /// Copy the best tile (lower color error) on destination data
        /// </summary>
        /// <param name="oDestinationData"></param>
        /// <param name="lTiles">List to tiles to compare</param>
        /// <returns></returns>
        public static bool MergeData(int[,]? oDestinationData, List<TileBase> lTiles)
        {
            if (oDestinationData == null)
            {
                return false;
            }
            if (lTiles == null || lTiles.Count == 0)
            {
                return false;
            }
            var oTile = lTiles.Where(X => X.Error == lTiles.Min(Y => Y.Error)).FirstOrDefault();
            if (oTile == null)
            {
                return false;
            }
            return oTile.MergeData(oDestinationData);
        }
    }
}
