using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionPalette : ColorTransformBase
    {
        //-------------------------------------------------------------
        // Generic Fixed Palette Management 
        //-------------------------------------------------------------
        public ColorTransformReductionPalette()
        {
            Type = ColorTransformType.ColorReductionGenericPalette;
            Description = "Color palette trasformation";
        }

        protected async override Task<bool> CreateTrasformationMapAsync(CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {                
                TransformationMap.Reset();
                Parallel.ForEach(sourceDataContainer.ColorPalette.rgbPalette, rgb =>
                {
                    TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, FixedPalette, this.ColorDistanceEvaluationMode));
                });
                return true;
            });
        }

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            string sM = nameof(ExecuteTransformAsync);

            var oProcessed = await TransformationMap.TransformAsync(SourceData, oToken);

            if (oProcessed != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, oProcessed);
            }
            else
            {
                return new();
            }
        }
    }
}
