using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionEga : ColorTransformBase
    {


        public ColorTransformReductionEga()
        {
            Name = "Fixed palette color reduction";
            Description = "Reduce color to EGA palette";
        }

        public int ColorsMax => hashColorsPalette.Count;
        public string PaletteFile { get; set; }
        protected override void CreateTrasformationMap()
        {

            hashColorsPalette.Add(0x00000000);
            hashColorsPalette.Add(0x000000AA);
            hashColorsPalette.Add(0x0000AA00);
            hashColorsPalette.Add(0x0000AAAA);
            hashColorsPalette.Add(0x00AA0000);
            hashColorsPalette.Add(0x00AA00AA);
            hashColorsPalette.Add(0x00AA5500);
            hashColorsPalette.Add(0x00AAAAAA);
            hashColorsPalette.Add(0x00555555);
            hashColorsPalette.Add(0x005555FF);
            hashColorsPalette.Add(0x0055FF55);
            hashColorsPalette.Add(0x00FF5555);
            hashColorsPalette.Add(0x00FF55FF);
            hashColorsPalette.Add(0x00FFFF55);
            hashColorsPalette.Add(0x00FFFFFF);

            if (oColorHistogram.Count < ColorsMax)
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