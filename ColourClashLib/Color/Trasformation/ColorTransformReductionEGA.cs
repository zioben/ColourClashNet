using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionEGA : ColorTransformReductionPalette
    {
        public ColorTransformReductionEGA()
        {
            Type = ColorTransformType.ColorReductionEga;
            Description = "Reduce color to EGA palette";
            CreatePalette();
        }
        void CreatePalette()
        {
            SetProperty(
                ColorTransformProperties.PriorityPalette,
                new List<int>
                {
                    0x00_00_00_00,
                    0x00_00_00_AA,
                    0x00_00_AA_00,
                    0x00_00_AA_AA,
                    0x00_AA_00_00,
                    0x00_AA_00_AA,
                    0x00_AA_55_00,
                    0x00_AA_AA_AA,
                    0x00_55_55_55,
                    0x00_55_55_FF,
                    0x00_55_FF_55,
                    0x00_FF_55_55,
                    0x00_FF_55_FF,
                    0x00_FF_FF_55,
                    0x00_FF_FF_FF,
                });
        }
    }
}