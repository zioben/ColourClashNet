using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionEGA : ColorTransformReductionPalette
    {
        public ColorTransformReductionEGA()
        {
            Name = ColorTransformType.ColorReductionEga;
            Description = "Reduce color to EGA palette";
        }
        protected override void CreateTrasformationMap()
        {
            Palette.Reset();
            Palette.Add(0x00000000);
            Palette.Add(0x000000AA);
            Palette.Add(0x0000AA00);
            Palette.Add(0x0000AAAA);
            Palette.Add(0x00AA0000);
            Palette.Add(0x00AA00AA);
            Palette.Add(0x00AA5500);
            Palette.Add(0x00AAAAAA);
            Palette.Add(0x00555555);
            Palette.Add(0x005555FF);
            Palette.Add(0x0055FF55);
            Palette.Add(0x00FF5555);
            Palette.Add(0x00FF55FF);
            Palette.Add(0x00FFFF55);
            Palette.Add(0x00FFFFFF);
        }

    }
}