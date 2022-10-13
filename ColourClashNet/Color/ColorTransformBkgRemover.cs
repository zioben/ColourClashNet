using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformBkgRemover: ColorTransformIdentity
    {
        public List<ColorItem> ColorBackgroundList { get; set; } = new List<ColorItem>();
        public ColorItem ColorBackground { get; set; } = new ColorItem(0, 0, 0);
        protected override void BuildTrasformation()
        {
            base.BuildTrasformation();
            int iColorRemoved = 0;  
            foreach (var kvp in DictTransform)
            {
                var Val = kvp.Key;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    iColorRemoved++;
                    DictTransform[kvp.Key] = ColorBackground;
                }
            }
            ResultColors -= iColorRemoved;
        }
    }
}
