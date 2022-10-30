using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionScanLine: ColorTransformBase
    {

        public int MaxColors { get; set; } = -1;

        public bool Clustering { get; set; }
        public bool ClusteringUseMean { get; set; }

        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();

        protected override void CreateTrasformationMap()
        {
        }


        public override int[,]? Transform(int[,]? oSource, Dictionary<Parameters, object>? oParameters)
        {
            if (oSource == null)
                return null;

            ColorListRow.Clear();

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            var oCols = new int[1, C];
            //            var oTrasf= new ColorTransformReductionFast();
            ColorTransformInterface oTrasf;
            if (Clustering)
            {
                var oTrasf2 = new ColorTransformReductionCluster();
                oTrasf2.MaxColors = MaxColors;
                oTrasf2.UseClusterColorMean = ClusteringUseMean;    
                oTrasf2.TrainingLoop = 30;
                oTrasf = oTrasf2;
            }
            else
            {
                var oTrasf2 = new ColorTransformReductionFast();
                oTrasf2.MaxColors = MaxColors;
                oTrasf = oTrasf2;
            }

            HashSet<int> oRowColors = new HashSet<int>();

            for (int r = 0; r < R; r++)
            {
                Array.Clear(oCols);
                for (int c = 0; c < C; c++)
                {
                    oCols[0, c] = oSource[r, c];
                }
                oTrasf.Create(oCols);
                var oColsTrasf = oTrasf.Transform(oCols, null);
                if (oColsTrasf == null)
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = -1;
                    }
                }
                else
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = oColsTrasf[0, c];
                    }
                }

                oRowColors.Clear();
                foreach (var kvp in oTrasf.oColorTransformationMap)
                {
                    oRowColors.Add(kvp.Value);
                    hashColorsPalette.Add(kvp.Value);
                }
                ColorListRow.Add(oRowColors.ToList());
            }
            return oRet;
        }

    }
}