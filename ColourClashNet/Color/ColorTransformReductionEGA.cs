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
            hashColorsPalette.Add(0x00000000);
            hashColorsPalette.Add(0x000000AA);
            hashColorsPalette.Add(0x0000AA00);
            hashColorsPalette.Add(0x0000AAAA);
            hashColorsPalette.Add(0x00AA0000);
            hashColorsPalette.Add(0x00AA00AA);
            hashColorsPalette.Add(0x00AA5500);
            hashColorsPalette.Add(0x00AAAAAA);
            hashColorsPalette.Add(0x00555555);
            hashColorsPalette.Add(0x005555FF);
            hashColorsPalette.Add(0x0055FF55);
            hashColorsPalette.Add(0x00FF5555);
            hashColorsPalette.Add(0x00FF55FF);
            hashColorsPalette.Add(0x00FFFF55);
            hashColorsPalette.Add(0x00FFFFFF);
        }

    }
}