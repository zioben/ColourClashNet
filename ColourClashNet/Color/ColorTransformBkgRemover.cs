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
            foreach (var kvp in oColorHistogram)
            {
                oColorsPalette.Add(kvp.Key);    
                oColorTransformation.Add(kvp.Key, kvp.Key);
            }

            foreach (var kvp in oColorHistogram)
            {
                var Val = kvp;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    oColorsPalette.Remove(kvp.Key);
                    oColorTransformation[kvp.Key] = ColorBackground;
                }
            }
        }

        public override int[,] Transform(int[,] oDataSource)
        {
            return base.TransformBase(oDataSource);
        }
    }
}
