using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Reduces the bit depth of each RGB channel (see <see cref="ColorQuantizationMode"/>).
    /// </summary>
    public class ColorTransformQuantization : ColorTransformMapBase<QuantizationOptions>
    {

        static string sC = nameof(ColorTransformQuantization);
        /// <summary>
        /// Creates the quantization.
        /// </summary>
        public ColorTransformQuantization()
        {
            Description = "Reduces color bit spectrum";
        }





        /// <summary>
        /// Quantizes a color to the given channel bit depth.
        /// <para/>
        /// The kept high bits are replicated into the low bits, so that the full range is preserved
        /// (e.g. 4 bit 0xF becomes 0xFF, not 0xF0).
        /// </summary>
        /// <param name="quantizationMode">Target bit depth</param>
        /// <param name="color">Packed color; non-color values are returned unchanged</param>
        /// <returns>The quantized color.</returns>
        public static int QuantizeColor(ColorQuantizationMode quantizationMode, int color)
        {
            if (color < 0)
                return color;
            switch (quantizationMode)
            {
                case ColorQuantizationMode.RGB888:
                    return color;
                case ColorQuantizationMode.RGB111:
                    {
                        int r = color.ToR() > 127 ? 0xFF : 0;
                        int g = color.ToG() > 127 ? 0xFF : 0;
                        int b = color.ToB() > 127 ? 0xFF : 0;
                        return ColorIntExt.FromRGB(r,g,b);
                    }
                case ColorQuantizationMode.RGB222:
                    {
                        // Replicate the kept bits into the lower bits so that full scale maps to 0xFF:
                        // 1100.0000
                        // 1122.0000
                        // 1122.3333
                        int rgb = color & 0x00C0C0C0;
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
                        int rgb = color & 0x00E0E0E0;
                        rgb |= rgb >> 3;
                        rgb |= ((rgb >> 6) & 0x00030303);
                        return rgb;
                    }
                case ColorQuantizationMode.RGB444:
                    {
                        // 1111.0000
                        // 1111.2222                      
                        int rgb = color & 0x00F0F0F0;
                        rgb |= rgb >> 4;
                        return rgb;
                    }
                case ColorQuantizationMode.RGB555:
                    {
                        // 1111.1000
                        // 1111.1222.22 <- need mask to preserve G color channel                      
                        int rgb = color & 0x00F8F8F8;
                        rgb |= ((rgb >> 5) & 0x00070707);
                        return rgb;
                    }
                case ColorQuantizationMode.RGB565:
                    {
                        // Different depth per channel: replicate the bits channel by channel
                        int r = color.ToR() & 0xF8;
                        int g = color.ToG() & 0xFC;
                        int b = color.ToB() & 0xF8;
                        r |= r >> 5;
                        g |= g >> 6;
                        b |= b >> 5;
                        return ColorIntExt.FromRGB(r, g, b);
                    }
                case ColorQuantizationMode.RGB666:
                    {

                        // 1111.1100
                        // 1111.1122.2222 <- need mask to preserve G color channel
                        int rgb = color & 0x00FCFCFC;
                        rgb |= ((rgb >> 6) & 0x00030303);
                        return rgb;
                    }
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }

        /// <summary>
        /// Quantizes a color with the current <see cref="QuantizationOptions.Mode"/>.
        /// </summary>
        public int QuantizeColor(int rgb)
            => QuantizeColor(Options.Mode, rgb);

        /// <summary>
        /// Maps each source color to its quantized value.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token=default)
        {
            string sM = nameof(CreateTransformationMap);
            LogDebug(sC, sM, $"{Type} : quantizing {ImageSource.Colors} colors to {Options.Mode}");

            TransformationMap.Reset();
            var rgbList = ImageSource.ColorPalette.ToList();
            foreach (var rgb in rgbList)
            {
                int quantized = QuantizeColor(Options.Mode, rgb);
                TransformationMap.Add(rgb, quantized);
            }
            return ColorTransformResult.CreateValidResult();
        }

    }
}
