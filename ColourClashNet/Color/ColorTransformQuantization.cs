using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformQuantization: ColorTransformBase
    {

        static string sClass = nameof(ColorTransformQuantization);
        public ColorTransformQuantization()
        {
            Type = ColorTransform.ColorReductionQuantization;
            Description = "Reduces color bit spectrum";
        }

        public ColorQuantizationMode QuantizationMode { get; set; }

        public int QuantizeColor(int iRGB)
        {
            if (iRGB < 0)
                return iRGB;
            double dR = iRGB.ToR();
            double dG = iRGB.ToG();
            double dB = iRGB.ToB();
            switch (QuantizationMode)
            {
                case ColorQuantizationMode.RGB888:
                    return iRGB;
                case ColorQuantizationMode.RGB222:
                    dR = Math.Round(dR / 64) * 64; 
                    dG = Math.Round(dG / 64) * 64;
                    dB = Math.Round(dB / 64) * 64;
                    break;
                case ColorQuantizationMode.RGB333:
                    dR = Math.Round(dR / 32) * 32;
                    dG = Math.Round(dG / 32) * 32;
                    dB = Math.Round(dB / 32) * 32;
                    break;
                case ColorQuantizationMode.RGB444:
                    dR = Math.Round(dR / 16) * 16;
                    dG = Math.Round(dG / 16) * 16;
                    dB = Math.Round(dB / 16) * 16;
                    break;
                case ColorQuantizationMode.RGB555:
                    dR = Math.Round(dR / 8) * 8;
                    dG = Math.Round(dG / 8) * 8;
                    dB = Math.Round(dB / 8) * 8;
                    break;
                case ColorQuantizationMode.RGB565:
                    dR = Math.Round(dR / 8) * 8;
                    dG = Math.Round(dG / 4) * 4;
                    dB = Math.Round(dB / 8) * 8;
                    break;
                default:
                    return -1;
            }
            var iRGBOut = ColorIntExt.FromRGB(dR, dG, dB);
            return (iRGBOut);
        }

 
        protected override void CreateTrasformationMap()
        {

            string sMethod = nameof(CreateTrasformationMap);
            Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Creating trasformation map");

            foreach (var kvp in oColorHistogram)
            {
                int iCol = QuantizeColor(kvp.Value);
                oColorTransformationMap.Add(kvp.Key, iCol);
            }
        }

        protected override int[,]? ExecuteTransform(int[,]? oSource)
        {
            if (oSource == null)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            var oCols = new int[1, C];
            hashColorsPalette.Clear();
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = QuantizeColor( oSource[r, c]);
                    hashColorsPalette.Add(col); 
                    oRet[r, c] = col;
                }
            }
            return oRet;
        }
    }
}
