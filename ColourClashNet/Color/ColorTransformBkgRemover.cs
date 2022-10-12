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
        protected override void BuildTrasformation()
        {
            base.BuildTrasformation();
            foreach (var kvp in DictHistogram)
            {
                var Val = kvp.Value;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                    DictTransform.Add(kvp.Key, new ColorItem() );
                else
                    DictTransform.Add(kvp.Key, kvp.Key);
                DictTransform.Add(kvp.Key, kvp.Key);
            }
            ResultColors = DictTransform.Select( X=>X.Value).ToList().Distinct().ToList().Count;
        }
    }
}
