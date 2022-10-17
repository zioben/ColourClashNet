using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformBkgRemover: ColorTransformBase
    {
        public List<int> ColorBackgroundList { get; set; } = new List<int>();
        public int ColorBackground { get; set; } = 0;


        protected override void BuildTrasformation()
        {
            oColorTransformation.Clear();
            foreach (var kvp in oColorHistogram)
            {
                oColorTransformation.Add(kvp.Key, kvp.Key);
            }
            ColorsUsed = oColorHistogram.Count();

            int iColorRemoved = 0;
            foreach (var kvp in oColorHistogram)
            {
                var Val = kvp;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    iColorRemoved++;
                    oColorTransformation[kvp.Key] = ColorBackground;
                }
            }
            ColorsUsed -= iColorRemoved;
        }
    }
}
