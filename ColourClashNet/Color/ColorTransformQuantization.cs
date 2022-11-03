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

        public ColorTransformQuantization()
        {
            Name = "Quantizator";
            Description = "Reduces color bit spectrum";
        }

        public ColorQuantizationMode QuantizationMode { get; set; }


        public int GetColorMask(ColorQuantizationMode colorHistMode)
        {
            switch (colorHistMode)
            {
                case ColorQuantizationMode.RGB888:
                    return -1;
                case ColorQuantizationMode.RGB333:
                    return 0x00C0C0C0;
                case ColorQuantizationMode.RGB444:
                    return 0x00F0F0F0;
                case ColorQuantizationMode.RGB555:
                    return 0x00F8F8F8;
                case ColorQuantizationMode.RGB565:
                    return 0x00F8FCF8;
                default:
                    return -1;
            }
        }
        public int GetFiller(ColorQuantizationMode colorHistMode)
        {
            return 0;
            switch (colorHistMode)
            {
                case ColorQuantizationMode.RGB888:
                    return 0;
                case ColorQuantizationMode.RGB333:
                    return 0x001F1F1F;
                case ColorQuantizationMode.RGB444:
                    return 0x000F0F0F;
                case ColorQuantizationMode.RGB555:
                    return 0x00070707;
                case ColorQuantizationMode.RGB565:
                    return 0x00070307;
                default:
                    return -1;
            }
        }

        public int Quantize(int iRGB, ColorQuantizationMode colorHistMode)
        {
            if (iRGB < 0)
                return iRGB;
            return (iRGB & GetColorMask(colorHistMode)) | GetFiller(colorHistMode);
        }

        protected override void CreateTrasformationMap()
        {
            foreach (var kvp in oColorHistogram)
            {
                int iCol = Quantize(kvp.Value, QuantizationMode);
                oColorTransformationMap.Add(kvp.Key, iCol);
            }
        }

        public override int[,] Transform(int[,] oSource)
        {
            if (oSource == null)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            var oCols = new int[1, C];
            var iMask = GetColorMask(QuantizationMode);
            var iFiller = GetFiller(QuantizationMode);  
            hashColorsPalette.Clear();
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = (oSource[r, c] & iMask) | iFiller;
                    hashColorsPalette.Add(col); 
                    oRet[r, c] = col;
                }
            }
            return oRet;
        }
    }
}
