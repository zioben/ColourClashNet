using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformQuantization: ColorTransformBase
    {
        public ColorQuantizationMode QuantizationMode { get; set; }


        public int GetColorMask(ColorQuantizationMode colorHistMode)
        {
            switch (colorHistMode)
            {
                case ColorQuantizationMode.RGB888:
                    return -1;
                case ColorQuantizationMode.RGB333:
                    return 0x00E0E0E0;
                case ColorQuantizationMode.RGB444:
                    return 0x00f0f0f0;
                case ColorQuantizationMode.RGB555:
                    return 0x00f8f8f8;
                case ColorQuantizationMode.RGB565:
                    return 0x00f8fcf8;
                default:
                    return -1;
            }
        }

        public int Quantize(int iRGB, ColorQuantizationMode colorHistMode)
        {
            if (iRGB < 0)
                return iRGB;
            return iRGB & GetColorMask(colorHistMode);
        }

        protected override void BuildTrasformation()
        {
            oColorTransformation.Clear();
            foreach (var kvp in oColorHistogram)
            {
                oColorTransformation.Add(kvp.Key, Quantize(kvp.Value, QuantizationMode));
            }
            ColorsUsed = oColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }

        public new int[,] Transform(int[,] oSource)
        {
            if (oSource == null)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            var oCols = new int[1, C];
            var iMask = GetColorMask(QuantizationMode);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    oRet[r, c] = oSource[r, c] & iMask;
                }
            }
            return oRet;
        }

    }
}
