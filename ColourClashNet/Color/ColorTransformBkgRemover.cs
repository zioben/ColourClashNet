using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
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
            foreach (var kvp in ColorHistogram)
            {
                ColorTransformationPalette.Add(kvp.Key);
                ColorTransformationMap.Add(kvp.Key, kvp.Key);
            }

            foreach (var kvp in ColorHistogram)
            {
                var Val = kvp;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    ColorTransformationPalette.Remove(kvp.Key);
                    ColorTransformationMap[kvp.Key] = ColorBackground;
                }
            }
        }
    }
}
