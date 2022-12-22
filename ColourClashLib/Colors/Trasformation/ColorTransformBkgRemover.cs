using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            Type = ColorTransform.ColorRemover;
            Description = "Substitute a colorlist with a single color";
        }

        public List<int> ColorBackgroundList { get; set; } = new List<int>();
        public int ColorBackground { get; set; } = 0;

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Creating trasformation map");
            foreach (var rgb in SourceColorPalette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                TransformationColorMap.Add(rgb, rgb);
            }

            foreach (var rgb in SourceColorPalette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                if (ColorBackgroundList.Any(X => X.Equals(rgb)))
                {
                    SourceColorPalette.rgbPalette.Remove(rgb);
                    TransformationColorMap[rgb] = ColorBackground;
                }
            }
        }
    }
}
