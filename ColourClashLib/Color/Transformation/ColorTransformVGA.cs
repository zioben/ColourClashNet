using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformVGA : ColorTransformBase
    {
        static string sC = nameof(ColorTransformVGA);

        public ColorTransformVGA()
        {
            Type = ColorTransformType.ColorReductionVGA;
            Description = "Commododre Amiga specific color reduction";
        }


        public int MaxColorUsed
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }   

        public ColorTransformVGA WithProperties(int iMaxColors)
        {
            MaxColorUsed = iMaxColors;
            return this;
        }

        public ColorTransformVGA WithProperties(ColorTransformConfig cfg) => WithProperties(cfg.MaxColorsWanted);



        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            var oQuantization = new ColorTransformQuantization();
            oQuantization.WithQuantization(ColorQuantizationMode.RGB666)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering( ColorDithering.None);
            var oResultQuantized = oQuantization.CreateAndProcessColors(ImageSource, token);

            var oCluster = new ColorTransformClustering()
            .WithProcessingParams(MaxColorUsed, 10, ColorSelectionMode.EvaluateColorMean)
            .WithColorDistanceEvaluationMode(config)
            .WithDithering(config)
            .WithReferencePalette(config);

            oCluster.ProcessPartial += (_, args) => RaiseProcessPartialEvent(args);

            var oClusterResult = oCluster.CreateAndProcessColors(oResultQuantized.DataOut, token);
            if(oClusterResult.IsSuccess)
            {
                 return ColorTransformResult.CreateValidResult(ImageSource, oClusterResult.DataOut);
            }
            return ColorTransformResult.CreateErrorResult("Processing error");
        }

    }
}
