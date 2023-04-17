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
            oPalette.Reset();
            oPalette.Add(0x00000000);
            oPalette.Add(0x000000AA);
            oPalette.Add(0x0000AA00);
            oPalette.Add(0x0000AAAA);
            oPalette.Add(0x00AA0000);
            oPalette.Add(0x00AA00AA);
            oPalette.Add(0x00AA5500);
            oPalette.Add(0x00AAAAAA);
            oPalette.Add(0x00555555);
            oPalette.Add(0x005555FF);
            oPalette.Add(0x0055FF55);
            oPalette.Add(0x00FF5555);
            oPalette.Add(0x00FF55FF);
            oPalette.Add(0x00FFFF55);
            oPalette.Add(0x00FFFFFF);
        }

    }
}