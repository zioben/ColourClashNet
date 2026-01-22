using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Defaults;
using ColourClashNet.Log;


namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformQuantization : ColorTransformBase
    {

        static string sC = nameof(ColorTransformQuantization);
        public ColorTransformQuantization()
        {
            Type = ColorTransformType.ColorReductionQuantization;
            Description = "Reduces color bit spectrum";
        }

        public ColorQuantizationMode QuantizationMode { get; set; }

        protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);
            switch (propertyName)
            {
                case ColorTransformProperties.QuantizationMode:
                        QuantizationMode = ToEnum<ColorQuantizationMode>(value);
                    break;
                default:
                    break;
            }
            return this;
        }

        public int QuantizeColor(int iRGB)
        {
            if (iRGB < 0)
                return iRGB;
            switch (QuantizationMode)
            {
                case ColorQuantizationMode.RGB888:
                    return iRGB;
                case ColorQuantizationMode.RGB111:
                    {
                        int r = iRGB.ToR() > 127 ? 0xFF : 0;
                        int g = iRGB.ToG() > 127 ? 0xFF : 0;
                        int b = iRGB.ToB() > 127 ? 0xFF : 0;
                        return ColorIntExt.FromRGB(r,g,b);
                    }
                case ColorQuantizationMode.RGB222:
                    {
                        // 1100.0000
                        // 1122.0000
                        // 1122.3333
                        int rgb = iRGB & 0x00C0C0C0;
                        rgb |= rgb >> 2;
                        rgb |= rgb >> 4;
                        return rgb;
                    }
                case ColorQuantizationMode.RGB333:
                    {
                        // R         G         B
                        // 1110.0000 1110.0000 ....
                        // 1112.2200 1112.2200 ....
                        // 1112.2223.3312.2233 <- need mask to preserve G color channel
                        //           ^^                      
                        int rgb = iRGB & 0x00E0E0E0;
                        rgb |= rgb >> 3;
                        rgb |= ((rgb >> 6) & 0x00010101);
                        return rgb;
                    }
                case ColorQuantizationMode.RGB444:
                    {
                        // 1111.0000
                        // 1111.2222                      
                        int rgb = iRGB & 0x00F0F0F0;
                        rgb |= rgb >> 4;
                        return rgb;
                    }
                case ColorQuantizationMode.RGB555:
                    {
                        // 1111.1000
                        // 1111.1222.22 <- need mask to preserve G color channel                      
                        int rgb = iRGB & 0x00F8F8F8;
                        rgb |= ((rgb >> 5) & 0x00070707);
                        return rgb;
                    }
                case ColorQuantizationMode.RGB565:
                    {
                        int r = iRGB.ToR() & 0xF8;
                        int g = iRGB.ToG() & 0xFC;
                        int b = iRGB.ToB() & 0xF8;
                        r |= r >> 5;
                        g |= g >> 6;
                        b |= b >> 5;
                        return ColorIntExt.FromRGB(r, g, b);
                    }
                case ColorQuantizationMode.RGB666:
                    {
                        // 1111.1100
                        // 1111.1122.2222 <- need mask to preserve G color channel
                        int rgb = iRGB & 0x00FCFCFC;
                        rgb |= ((rgb >> 6) & 0x00030303);
                        return rgb;
                    }
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }


        protected override ColorTransformResults CreateTransformationMap(CancellationToken oToken=default)
        {
            string sM = nameof(CreateTransformationMap);
            LogMan.Trace(sC, sM, $"{Type} : Creating trasformation map");

            TransformationMap.Reset();
            var rgbList = SourceData.ColorPalette.ToList();
            foreach (var rgb in rgbList)
            {
                int rgbQ = QuantizeColor(rgb);
                TransformationMap.Add(rgb, rgbQ);
            }
            return ColorTransformResults.CreateValidResult();
        }

        // Not needed
        //protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)

       
    }
}
