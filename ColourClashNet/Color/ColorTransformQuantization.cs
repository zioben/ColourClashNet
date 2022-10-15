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

        public int Quantize(int iRGB, ColorQuantizationMode colorHistMode)
        {
            if (iRGB < 0)
                return iRGB;
            switch (colorHistMode)
            {
                case ColorQuantizationMode.RGB888:
                    return iRGB;
                case ColorQuantizationMode.RGB333:
                    return iRGB & 0x001f1f1f;
                case ColorQuantizationMode.RGB444:
                    return iRGB & 0x000f0f0f;
                case ColorQuantizationMode.RGB555:
                    return iRGB & 0x00070707;
                case ColorQuantizationMode.RGB565:
                    return iRGB & 0x00070307;
                default:
                    return -1;
            }
        }


        protected override void BuildTrasformation()
        {
            ListColorTransformation.Clear();
            foreach (var kvp in ListColorHistogram)
            {
                ListColorTransformation.Add(kvp.Key, Quantize(kvp.Key, QuantizationMode));
            }
            ColorsUsed = ListColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}
