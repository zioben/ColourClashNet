using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionEGA : ColorTransformToPalette
    {


        public ColorTransformReductionEGA()
        {
            Type = ColorTransform.ColorReductionEga;
            Description = "Reduce color to EGA palette";
        }
        protected override void CreateTrasformationMap()
        {
            SourceColorPalette.Reset();
            SourceColorPalette.rgbPalette.Add(0x00000000);
            SourceColorPalette.rgbPalette.Add(0x000000AA);
            SourceColorPalette.rgbPalette.Add(0x0000AA00);
            SourceColorPalette.rgbPalette.Add(0x0000AAAA);
            SourceColorPalette.rgbPalette.Add(0x00AA0000);
            SourceColorPalette.rgbPalette.Add(0x00AA00AA);
            SourceColorPalette.rgbPalette.Add(0x00AA5500);
            SourceColorPalette.rgbPalette.Add(0x00AAAAAA);
            SourceColorPalette.rgbPalette.Add(0x00555555);
            SourceColorPalette.rgbPalette.Add(0x005555FF);
            SourceColorPalette.rgbPalette.Add(0x0055FF55);
            SourceColorPalette.rgbPalette.Add(0x00FF5555);
            SourceColorPalette.rgbPalette.Add(0x00FF55FF);
            SourceColorPalette.rgbPalette.Add(0x00FFFF55);
            SourceColorPalette.rgbPalette.Add(0x00FFFFFF);
        }

    }
}