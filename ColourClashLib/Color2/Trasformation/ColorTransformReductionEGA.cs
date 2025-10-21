using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionEGA : ColorTransformReductionPalette
    {
        public ColorTransformReductionEGA()
        {
            Type = ColorTransformType.ColorReductionEga;
            Description = "Reduce color to EGA palette";
            CreatePalette();
        }
        void CreatePalette()
        {
            SetProperty(
                ColorTransformProperties.Fixed_Palette,
                new List<int>
                {
                    0x00000000,
                    0x000000AA,
                    0x0000AA00,
                    0x0000AAAA,
                    0x00AA0000,
                    0x00AA00AA,
                    0x00AA5500,
                    0x00AAAAAA,
                    0x00555555,
                    0x005555FF,
                    0x0055FF55,
                    0x00FF5555,
                    0x00FF55FF,
                    0x00FFFF55,
                    0x00FFFFFF,
                });
        }
    }
}