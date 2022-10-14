using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformQuantization: ColorTransformBase
    {
        public ColorQuantizationMode QuantizationMode { get; set; }
        protected override void BuildTrasformation()
        {
            DictColorTransformation.Clear();
            foreach (var kvp in DictColorHistogram)
            {
                DictColorTransformation.Add(kvp.Key, kvp.Key.Quantize(QuantizationMode));
            }
            ColorsUsed = DictColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}
