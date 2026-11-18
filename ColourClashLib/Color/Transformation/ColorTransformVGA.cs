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

        public ColorTransformVGA WithProperties(int maxColors)
        {
            MaxColorUsed = maxColors;
            return this;
        }

        public ColorTransformVGA WithProperties(ColorTransformConfig cfg) => WithProperties(cfg.MaxColorsWanted);



        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            var quantization = new ColorTransformQuantization();
            quantization.WithQuantization(ColorQuantizationMode.RGB666)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering( ColorDithering.None);
            var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);

            var cluster = new ColorTransformClustering()
            .WithProcessingParams(MaxColorUsed, 10, ColorSelectionMode.EvaluateColorMean)
            .WithColorDistanceEvaluationMode(config)
            .WithDithering(config)
            .WithReferencePalette(config);

            cluster.ProcessPartial += (_, args) => RaiseProcessPartialEvent(args);

            var clusterResult = cluster.CreateAndProcessColors(resultQuantized.DataOut, token);
            if(clusterResult.IsSuccess)
            {
                 return ColorTransformResult.CreateValidResult(ImageSource, clusterResult.DataOut);
            }
            return ColorTransformResult.CreateErrorResult("Processing error");
        }

    }
}
