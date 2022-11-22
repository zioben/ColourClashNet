using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionEga : ColorTransformToPalette
    {


        public ColorTransformReductionEga()
        {
            Type = ColorTransform.ColorReductionEga;
            Description = "Reduce color to EGA palette";
        }
        protected override void CreateTrasformationMap()
        {
            ColorTransformationPalette.Clear();
            ColorTransformationPalette.Add(0x00000000);
            ColorTransformationPalette.Add(0x000000AA);
            ColorTransformationPalette.Add(0x0000AA00);
            ColorTransformationPalette.Add(0x0000AAAA);
            ColorTransformationPalette.Add(0x00AA0000);
            ColorTransformationPalette.Add(0x00AA00AA);
            ColorTransformationPalette.Add(0x00AA5500);
            ColorTransformationPalette.Add(0x00AAAAAA);
            ColorTransformationPalette.Add(0x00555555);
            ColorTransformationPalette.Add(0x005555FF);
            ColorTransformationPalette.Add(0x0055FF55);
            ColorTransformationPalette.Add(0x00FF5555);
            ColorTransformationPalette.Add(0x00FF55FF);
            ColorTransformationPalette.Add(0x00FFFF55);
            ColorTransformationPalette.Add(0x00FFFFFF);
        }

    }
}