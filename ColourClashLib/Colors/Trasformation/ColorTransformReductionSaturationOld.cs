using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionSaturationOLd
        : ColorTransformBase
    {


        public ColorTransformReductionSaturationOLd() 
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

        bool AllowGray { get; set; } = false;

        protected override void CreateTrasformationMap()
        {
            ColorPalette.Reset();
           // ColorPalette.Add(0x00000000);
            //ColorPalette.Add(0x00444444);
            //ColorPalette.Add(0x00888888);
            //ColorPalette.Add(0x00CCCCCC);
           // ColorPalette.Add(0x00FFFFFF);
            //for (int i = 0; i < 256; i++)
            //{
            //    ColorPalette.Add(ColorIntExt.FromRGB(i, i, i));
            //}

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
              //  ColorPalette.Add(ColorIntExt.FromRGB(col, col, col));
                //ColorPalette.Add(ColorIntExt.FromRGB(0, col, col/2));
                //ColorPalette.Add(ColorIntExt.FromRGB(col, col/2, 0));
                //ColorPalette.Add(ColorIntExt.FromRGB(col/2, 0, col));
                //ColorPalette.Add(ColorIntExt.FromRGB(0, col / 2, col));
                //ColorPalette.Add(ColorIntExt.FromRGB(col/2, col, 0));
                //ColorPalette.Add(ColorIntExt.FromRGB(col , 0, col/2));
                if (AllowGray)
                {
                    ColorPalette.Add(ColorIntExt.FromRGB(col, col, col));
                }
                gray += step;
            }
        }

    }
}