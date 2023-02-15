using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionChromaEnhancer : ColorTransformToPalette
    {


        public ColorTransformReductionChromaEnhancer() 
        {
            Type = ColorTransform.ColorReductionEga;
            Description = "Expand color crominance";
        }

        int iLumLevels = 2;
        public int LuminanceLevels 
        {
            get => iLumLevels;
            set
            { 
                iLumLevels = Math.Max(1, value);
            }
        } 

        protected override void CreateTrasformationMap()
        {
            ColorPalette.Reset();
            ColorPalette.Add(0x00000000);
            ColorPalette.Add(0x00FFFFFF);
            double step = 256 / iLumLevels;
            double gray = step;
            for ( int i = 0; i < LuminanceLevels; i++ ) 
            {
                int col = Math.Min(255, (int)gray);
                ColorPalette.Add( ColorIntExt.FromRGB(0, 0, col) );
                ColorPalette.Add(ColorIntExt.FromRGB(0, col, col));
                ColorPalette.Add(ColorIntExt.FromRGB(0, col, 0));
                ColorPalette.Add(ColorIntExt.FromRGB(col, col, 0));
                ColorPalette.Add(ColorIntExt.FromRGB(col, 0, 0));
                ColorPalette.Add(ColorIntExt.FromRGB(col, 0, col));
                gray += step;
            }
        }

    }
}