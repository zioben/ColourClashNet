using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using ColourClashNet.Color;
using ColourClashNet.Log;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformQuantization : ColorTransformBase
    {

        static string sClass = nameof(ColorTransformQuantization);
        public ColorTransformQuantization()
        {
            Type = ColorTransformType.ColorReductionQuantization;
            Description = "Reduces color bit spectrum";
        }

        public ColorQuantizationMode QuantizationMode { get; set; }

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.QuantizationMode:
                    if (Enum.TryParse<ColorQuantizationMode>(oValue?.ToString(), out var evm))
                    {
                        QuantizationMode = evm;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


        public int QuantizeColorR(int iRGB)
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
                case ColorQuantizationMode.RGB666:
                    dR = Math.Round(dR / 4) * 4;
                    dG = Math.Round(dG / 4) * 4;
                    dB = Math.Round(dB / 4) * 4;
                    break;
                default:
                    return ColorDefaults.DefaultInvalidColorRGB;
            }
            var iRGBOut = ColorIntExt.FromRGB(dR, dG, dB);
            return iRGBOut;
        }

        public int QuantizeColor1(int iRGB)
        {
            if (iRGB < 0)
                return iRGB;
            switch (QuantizationMode)
            {
                case ColorQuantizationMode.RGB888:
                    return iRGB;
                case ColorQuantizationMode.RGB111:
                    return iRGB & 0x00808080;
                case ColorQuantizationMode.RGB222:
                    return iRGB & 0x00C0C0C0;
                case ColorQuantizationMode.RGB333:
                    return iRGB & 0x00E0E0E0;
                case ColorQuantizationMode.RGB444:
                    return iRGB & 0x00F0F0F0;
                case ColorQuantizationMode.RGB555:
                    return iRGB & 0x00F8F8F8;
                case ColorQuantizationMode.RGB565:
                    return iRGB & 0x00F8FCF8;
                case ColorQuantizationMode.RGB666:
                    return iRGB & 0x00FCFCFC;
                default:
                    return ColorDefaults.DefaultInvalidColorRGB;
            }
        }

        public int QuantizeColor2(int iRGB)
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
                        return ColorIntExt.FromRGB(r, g, b);
                    }
                case ColorQuantizationMode.RGB222:
                    {
                        // 1100.0000
                        // 1122.0000
                        // 1122.3333
                        int r = iRGB.ToR() & 0xC0;
                        int g = iRGB.ToG() & 0xC0;
                        int b = iRGB.ToB() & 0xC0;
                        r |= r >> 2;
                        r |= r >> 4;
                        g |= g >> 2;
                        g |= g >> 4;
                        b |= b >> 2;
                        b |= b >> 4;
                        return ColorIntExt.FromRGB(r, g, b);
                        //int rgb = iRGB & 0x00C0C0C0;
                        //rgb |= rgb >> 2;
                        //rgb |= rgb >> 4;
                        //return rgb;
                    }
                case ColorQuantizationMode.RGB333:
                    {
                        // R         G         B
                        // 1110.0000 1110.0000 ....
                        // 1112.2200 1112.2200 ....
                        // 1112.2223.3312.2233 <- need mask to preserve G color channel
                        //           ^^
                        int r = iRGB.ToR() & 0xE0;
                        int g = iRGB.ToG() & 0xE0;
                        int b = iRGB.ToB() & 0xE0;
                        r |= r >> 3;
                        r |= r >> 6;
                        g |= g >> 3;
                        g |= g >> 6;
                        b |= b >> 3;
                        b |= b >> 6;
                        return ColorIntExt.FromRGB(r, g, b);
                        //int rgb = iRGB & 0x00E0E0E0;
                        //rgb |= rgb >> 3;
                        //rgb |= ((rgb >> 6) & 0x00010101);
                        //return rgb;
                    }
                case ColorQuantizationMode.RGB444:
                    {
                        // 1111.0000
                        // 1111.2222
                        int r = iRGB.ToR() & 0xF0;
                        int g = iRGB.ToG() & 0xF0;
                        int b = iRGB.ToB() & 0xF0;
                        r |= r >> 4;
                        g |= g >> 4;
                        b |= b >> 4;
                        return ColorIntExt.FromRGB(r, g, b);
                        //int rgb = iRGB & 0x00F0F0F0;
                        //rgb |= rgb >> 4;
                        //return rgb;
                    }
                case ColorQuantizationMode.RGB555:
                    {
                        // 1111.1000
                        // 1111.1222.22 <- need mask to preserve G color channel
                        int r = iRGB.ToR() & 0xF8;
                        int g = iRGB.ToG() & 0xF8;
                        int b = iRGB.ToB() & 0xF8;
                        r |= r >> 5;
                        g |= g >> 5;
                        b |= b >> 5;
                        return ColorIntExt.FromRGB(r, g, b);
                        //int rgb = iRGB & 0x00F8F8F8;
                        //rgb |= ((rgb >> 5) & 0x00070707);
                        //return rgb;
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
                        int r = iRGB.ToR() & 0xFC;
                        int g = iRGB.ToG() & 0xFC;
                        int b = iRGB.ToB() & 0xFC;
                        r |= r >> 6;
                        g |= g >> 6;
                        b |= b >> 6;
                        return ColorIntExt.FromRGB(r, g, b);
                        //int rgb = iRGB & 0x00FCFCFC;
                        //rgb |= ((rgb >> 6) & 0x00030303);
                        //return rgb;
                    }
                default:
                    return ColorDefaults.DefaultInvalidColorRGB;
            }
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
                    return ColorDefaults.DefaultInvalidColorRGB;
            }
        }


        protected override void CreateTrasformationMap()
        {

            string sMethod = nameof(CreateTrasformationMap);
            LogMan.Trace(sClass, sMethod, $"{Type} : Creating trasformation map");  

            foreach (var kvp in OutputHistogram.rgbHistogram)
            {
                int iCol = QuantizeColor(kvp.Value);
                ColorTransformationMapper.Add(kvp.Key, iCol);
            }
        }

        protected override int[,]? ExecuteTransform(int[,]? oSource, CancellationToken token)
        {
            if (oSource == null)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            var oCols = new int[1, C];
            OutputPalette = new Palette();
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = QuantizeColor(oSource[r, c]);
                    OutputPalette.Add(col);
                    oRet[r, c] = col;
                }
                token.ThrowIfCancellationRequested();   
            }
            return oRet;
        }
    }
}
