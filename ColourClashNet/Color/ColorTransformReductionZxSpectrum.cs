using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionZxSpectrum : ColorTransformBase
    {


        public ColorTransformReductionZxSpectrum()
        {
            Name = "Zx Spectrum color reduction";
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        public int MaxColors { get; set; } = -1;

        protected override void CreateTrasformationMap()
        {

            hashColorsPalette.Add(0x00000000);
            hashColorsPalette.Add(0x000000ee);
            hashColorsPalette.Add(0x00ee0000);
            hashColorsPalette.Add(0x00ee00ee);
            hashColorsPalette.Add(0x0000ee00);
            hashColorsPalette.Add(0x0000eeee);
            hashColorsPalette.Add(0x00eeee00);
            hashColorsPalette.Add(0x00ffffff);
            hashColorsPalette.Add(0x000000ff);
            hashColorsPalette.Add(0x00ff0000);
            hashColorsPalette.Add(0x00ff00ff);
            hashColorsPalette.Add(0x0000ff00);
            hashColorsPalette.Add(0x0000ffff);
            hashColorsPalette.Add(0x00ffff00);
            hashColorsPalette.Add(0x00ffffff);

            if (oColorHistogram.Count < MaxColors)
            {
                foreach (var kvp in oColorHistogram)
                {
                    oColorTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            var listAll = oColorHistogram.Select(X => X.Key).ToList();
            var listMax = hashColorsPalette;
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                oColorTransformationMap[X] = oItem; 
            });
        }

        public override int[,]? Transform(int[,]? oDataSource)
        {
            return base.ApplyTransform(oDataSource);
        }
    }
}