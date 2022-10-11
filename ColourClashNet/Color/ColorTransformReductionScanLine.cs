using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformReductionScanLine: ColorTransformBase
    {

        public int MaxColors { get; set; } = -1;

        public bool Clustering { get; set; }

        protected override void BuildTrasformation()
        {
        }


        public new ColorItem[,] Transform(ColorItem[,] oSource)
        {
            if (oSource == null)
                return null;
            
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new ColorItem[R, C];
            var oCols = new ColorItem[1, C];
            //            var oTrasf= new ColorTransformReductionFast();
            ColorTransform oTrasf;
            if (Clustering)
            {
                var oTrasf2 = new ColorTransformReductionCluster();
                oTrasf2.MaxColors = MaxColors;
                oTrasf2.TrainingLoop = 30;
                oTrasf = oTrasf2;
            }
            else
            {
                var oTrasf2 = new ColorTransformReductionFast();
                oTrasf2.MaxColors = MaxColors;
                oTrasf = oTrasf2;
            }
            for (int r = 0; r < R; r++)
            {
                Array.Clear(oCols);
                for (int c = 0; c < C; c++)
                {
                    oCols[0, c] = oSource[r, c];
                }
                oTrasf.Create(oCols, BackColor);
                var oColsTrasf = oTrasf.Transform(oCols);
                if (oColsTrasf == null)
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = new ColorItem();
                    }
                }
                else
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = oColsTrasf[0, c];
                    }
                }
            }
            return oRet;
        }

    }
}