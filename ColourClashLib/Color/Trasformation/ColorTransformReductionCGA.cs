using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCGA : ColorTransformReductionPalette
    {

        public enum CGAVideoMode
        {
            Mode4_0L,
            Mode4_0H,
            Mode4_1L,
            Mode4_1H,
            Mode5_1L,
            Mode5_1H,
            RGBI,
            Composite_1,
            Composite_2,
        }

        List<int> paletteFull;
        List<int> paletteMode4_0L;
        List<int> paletteMode4_0H;
        List<int> paletteMode4_1L;
        List<int> paletteMode4_1H;
        List<int> paletteMode5_1L;
        List<int> paletteMode5_1H;
        List<int> paletteRGBI;
        List<int> paletteComposite_1;
        List<int> paletteComposite_2;

        public ColorTransformReductionCGA()
        {
            Type = ColorTransformType.ColorReductionEga;
            Description = "Reduce color to CGA palette";
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

            paletteFull = FixedPalette.ToList();

            paletteMode4_0L = new List<int>()
            {
                paletteFull[0],
                paletteFull[2],
                paletteFull[4],
                paletteFull[6],
            };

            paletteMode4_0H = new List<int>()
            {
                paletteFull[0],
                paletteFull[10],
                paletteFull[12],
                paletteFull[14],
            };

            paletteMode4_1L = new List<int>()
            {
                paletteFull[0],
                paletteFull[3],
                paletteFull[5],
                paletteFull[7],
            };

            paletteMode4_1H = new List<int>()
            {
                paletteFull[0],
                paletteFull[11],
                paletteFull[13],
                paletteFull[15],
            };

            paletteMode5_1L = new List<int>()
            {
                paletteFull[0],
                paletteFull[3],
                paletteFull[4],
                paletteFull[7],
            };

            paletteMode5_1H = new List<int>()
            {
                paletteFull[0],
                paletteFull[11],
                paletteFull[12],
                paletteFull[15],
            };
        }
    }
}