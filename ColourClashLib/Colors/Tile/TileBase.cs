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

        static ColorDistanceEvaluationMode eColorMode = ColorDistanceEvaluationMode.RGB;

        public int TileW { get; set; } = 8;
        public int TileH { get; set; } = 8;
        public int MaxColors { get; set; } = 2;
        public int SourceC { get; private set; } = 0;
        public int SourceR { get; private set; } = 0;

        public double Error { get; private set; } = 0;

        int[,] DataSource { get; set; }
        int[,] DataProcessed { get; set; }

        public int[,]? ExecuteTrasform(int[,]? oDataSource, int iSourceR, int iSourceC )
        {
            DataSource = new int[TileW, TileH];
            DataProcessed  = new int[TileW, TileH];
            SourceC = iSourceC; 
            SourceR = iSourceR;
            if (oDataSource == null)
            {
                return null;                
            }
            if( TileW <= 0 || TileH <= 0 || MaxColors <= 0 )
            {
                return null;
            }

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            int CC = Math.Max(0, Math.Min(C, SourceC + TileW));
            int RR = Math.Max(0, Math.Min(R, SourceR + TileH));

            Trace.TraceInformation($"R={iSourceR}->{RR}, C={iSourceC}->{CC}");

            // Get tile data
            for (int sr = SourceR, r=0; sr<RR; sr++, r++ )
            {
                for (int sc = SourceC, c=0 ; sc <CC; sc++, c++)
                {
                    DataSource[r, c] = oDataSource[sr,sc];
                    DataProcessed[r, c] = oDataSource[sr,sc];
                }
            }

            var oColorReduction = new ColorTransformReductionCluster()
            {
                ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
                ColorsMax = 2,
                UseClusterColorMean = false,
                TrainingLoop = 3,
            };
            oColorReduction.Create(DataSource);
            DataProcessed = oColorReduction.TransformAndDither(DataSource);
            Error = ColorTransformBase.Error(DataSource, DataProcessed, eColorMode);
            return DataProcessed;
        }

        public void MergeData(int[,]? oDataMerge)
        {
            if (oDataMerge == null)
            {
                return;
            }
            if (DataProcessed == null)
            {
                return;
            }
            int R = oDataMerge.GetLength(0);
            int C = oDataMerge.GetLength(1);
            int RR = Math.Max(0, Math.Min(R, SourceR + TileH));
            int CC = Math.Max(0, Math.Min(C, SourceC + TileW));

            // Merge Data
            for (int dr = SourceR, r = 0; dr < RR; dr++, r++)
            {
                for (int dc = SourceC, c = 0; dc < CC; dc++, c++)
                {
                    oDataMerge[dr, dc] = DataProcessed[r, c];
                }
            }
        }

        public static bool MergeData(int[,]? oDataMerge, TileBase oTileA, TileBase oTileB)
        {
            if (oTileA == null && oTileB == null)
            {
                return false;
            }
            if (oTileA != null && oTileB == null)
            {
                oTileA.MergeData(oDataMerge);
                return true;
            }
            if (oTileA == null && oTileB != null)
            {
                oTileB.MergeData(oDataMerge);
                return true;
            }
            if (oTileA.TileW == oTileB.TileW && oTileA.TileH == oTileB.TileH && oTileA.SourceC == oTileB.SourceC && oTileA.SourceR == oTileB.SourceR)
            {
                if (oTileA.Error < oTileB.Error)
                {
                    oTileA.MergeData(oDataMerge);
                }
                else
                {
                    oTileB.MergeData(oDataMerge);
                }
                return true;    
            }
            return false;
        }
    }
}
