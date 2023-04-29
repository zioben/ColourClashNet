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
            type = ColorTransform.ColorReductionEga;
            description = "Reduce color to EGA palette";
        }
        protected override void CreateTrasformationMap()
        {
            colorPalette.Reset();
            colorPalette.Add(0x00000000);
            colorPalette.Add(0x000000AA);
            colorPalette.Add(0x0000AA00);
            colorPalette.Add(0x0000AAAA);
            colorPalette.Add(0x00AA0000);
            colorPalette.Add(0x00AA00AA);
            colorPalette.Add(0x00AA5500);
            colorPalette.Add(0x00AAAAAA);
            colorPalette.Add(0x00555555);
            colorPalette.Add(0x005555FF);
            colorPalette.Add(0x0055FF55);
            colorPalette.Add(0x00FF5555);
            colorPalette.Add(0x00FF55FF);
            colorPalette.Add(0x00FFFF55);
            colorPalette.Add(0x00FFFFFF);
        }

    }
}