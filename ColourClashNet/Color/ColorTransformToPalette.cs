using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformToPalette : ColorTransformBase
    {

        public ColorTransformToPalette()
        {
            Description = "Color palette trasformation";
        }

        protected override void CreateTrasformationMap()
        {
            foreach (var kvp in ColorHistogram)
            {
                ColorTransformationPalette.Add(kvp.Key);
            }
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oHasSet = new  HashSet<int>();

            foreach (var rgb in oDataSource)
            {
                oHasSet.Add(rgb);
            }
            oHasSet.Remove(-1);
            foreach (var rgb in oHasSet)
            {
                ColorTransformationMap.Add(rgb, rgb);
            }
            var oColorList = ColorTransformationPalette;
            Parallel.ForEach( oHasSet, rgb =>
            {
                ColorTransformationMap[rgb] = ColorTransformBase.GetNearestColor(rgb, ColorTransformationPalette, ColorDistanceEvaluationMode);
            });

            return base.ExecuteTransform(oDataSource); 
        }

    }
}
