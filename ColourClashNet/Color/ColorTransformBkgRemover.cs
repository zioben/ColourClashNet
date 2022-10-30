using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {

        public ColorTransformBkgRemover()
        {
            Name = "Bkg Remover";
            Description = "Substitute a colorlist with a single color";
        }

        public List<int> ColorBackgroundList { get; set; } = new List<int>();
        public int ColorBackground { get; set; } = 0;

      

        protected override void CreateTrasformationMap()
        {
            foreach (var kvp in oColorHistogram)
            {
                hashColorsPalette.Add(kvp.Key);
                oColorTransformationMap.Add(kvp.Key, kvp.Key);
            }

            foreach (var kvp in oColorHistogram)
            {
                var Val = kvp;
                if (ColorBackgroundList.Any(X => X.Equals(Val)))
                {
                    hashColorsPalette.Remove(kvp.Key);
                    oColorTransformationMap[kvp.Key] = ColorBackground;
                }
            }
        }

        public override int[,]? Transform(int[,]? oDataSource, Dictionary<Parameters, object>? oParameters)
        {
            return ApplyTransform(oDataSource);
        }
    }
}
