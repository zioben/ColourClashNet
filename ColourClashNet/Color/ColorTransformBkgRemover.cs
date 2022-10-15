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
            ListColorTransformation.Clear();
            foreach (var kvp in ListColorHistogram)
            {
                ListColorTransformation.Add(kvp.Key, kvp.Key);
            }
            ColorsUsed = ListColorHistogram.Count();

            int iColorRemoved = 0;
            foreach (var kvp in ListColorHistogram)
            {
                var Val = kvp.Key;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    iColorRemoved++;
                    ListColorHistogram[kvp.Key] = ColorBackground;
                }
            }
            ColorsUsed -= iColorRemoved;
        }
    }
}
