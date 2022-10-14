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
        public List<ColorItem> ColorBackgroundList { get; set; } = new List<ColorItem>();
        public ColorItem ColorBackground { get; set; } = new ColorItem(0, 0, 0);


        protected override void BuildTrasformation()
        {
            DictColorTransformation.Clear();
            foreach (var kvp in DictColorHistogram)
            {
                DictColorTransformation.Add(kvp.Key, kvp.Key);
            }
            ColorsUsed = DictColorHistogram.Count();

            int iColorRemoved = 0;  
            foreach (var kvp in DictColorTransformation)
            {
                var Val = kvp.Key;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    iColorRemoved++;
                    DictColorTransformation[kvp.Key] = ColorBackground;
                }
            }
            ColorsUsed -= iColorRemoved;
        }
    }
}
