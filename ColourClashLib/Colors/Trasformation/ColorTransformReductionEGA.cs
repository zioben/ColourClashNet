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
            Type = ColorTransform.ColorReductionEga;
            Description = "Reduce color to EGA palette";
        }
        protected override void CreateTrasformationMap()
        {
            ColorPalette.Reset();
            ColorPalette.Add(0x00000000);
            ColorPalette.Add(0x000000AA);
            ColorPalette.Add(0x0000AA00);
            ColorPalette.Add(0x0000AAAA);
            ColorPalette.Add(0x00AA0000);
            ColorPalette.Add(0x00AA00AA);
            ColorPalette.Add(0x00AA5500);
            ColorPalette.Add(0x00AAAAAA);
            ColorPalette.Add(0x00555555);
            ColorPalette.Add(0x005555FF);
            ColorPalette.Add(0x0055FF55);
            ColorPalette.Add(0x00FF5555);
            ColorPalette.Add(0x00FF55FF);
            ColorPalette.Add(0x00FFFF55);
            ColorPalette.Add(0x00FFFFFF);
        }

    }
}