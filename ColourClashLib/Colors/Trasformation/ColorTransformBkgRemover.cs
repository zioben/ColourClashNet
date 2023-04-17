using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            type = ColorTransform.ColorRemover;
            description = "Substitute a colorlist with a single color";
        }

        public List<int> ColorBackgroundList { get; set; } = new List<int>();
        public int ColorBackground { get; set; } = 0;

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            if (ColorDefaults.Trace)
                Trace.TraceInformation($"{sClass}.{sMethod} ({type}) : Creating trasformation map");
            foreach (var rgb in oPalette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                colorTransformationMap.Add(rgb, rgb);
            }

            foreach (var rgb in oPalette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                if (ColorBackgroundList.Any(X => X.Equals(rgb)))
                {
                    oPalette.Remove(rgb);
                    colorTransformationMap.rgbTransformationMap[rgb] = ColorBackground;
                }
            }
        }
    }
}
