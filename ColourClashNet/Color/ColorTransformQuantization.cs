using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformQuantization: ColorTransformBase
    {

        public ColorQuantizationMode QuantizationMode { get; set; }
        protected override void BuildTrasformation()
        {
            DictTransform.Clear();
            foreach (var kvp in DictHistogram)
            {
                DictTransform.Add(kvp.Key, kvp.Key.Quantize(QuantizationMode));
            }
            ResultColors = DictTransform.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}
