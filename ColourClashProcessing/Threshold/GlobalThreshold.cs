using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace ColourClashProcessing.Threshold
{
    public class GlobalThreshold
    {
        static string sC = nameof(GlobalThreshold);
        public Palette ThresholdPalette { get; set; } = new();
        public double ToleranceFactor { get; set; } = 0.0;
        public bool InvertResults { get; set; } = false;

        public GlobalThreshold()
        {
            ThresholdPalette.Create(new List<int>() { ColorIntExt.FromRGB(255, 0, 255) });
        }

        public GlobalThreshold WithThreshold(Palette thresholdPalette, double toleranceFactor = 0.0, bool invertResults = false)
        {
            ThresholdPalette = new Palette().Create(thresholdPalette);
            ToleranceFactor = toleranceFactor;
            InvertResults = invertResults;
            return this;
        }

        public GlobalThreshold Clone()
        {
            return new GlobalThreshold()
            {
                ThresholdPalette = new Palette().Create(this.ThresholdPalette),
                ToleranceFactor = this.ToleranceFactor,
                InvertResults = this.InvertResults
            };
        }

        public GlobalThreshold WithThreshold(int rgbThreshold, double toleranceFactor = 0.0, bool invertResults = false)
        {
            return WithThreshold(new Palette().Create(new List<int> { rgbThreshold }), toleranceFactor, invertResults);
        }

        public ColorTransformResult Process(ImageData image, CancellationToken cancellationToken = default)
        {
            string sM = nameof(Process);
            try
            {
                ImageData.AssertValid(image);
                var bgCol = InvertResults ? ColorDefaults.DefaultShapeForeColorInt : ColorDefaults.DefaultShapeBkgColorInt;
                var fgCol = InvertResults ? ColorDefaults.DefaultShapeBkgColorInt : ColorDefaults.DefaultShapeForeColorInt;

                var palette = new HistogramRGB().Create(image).ToPalette();

                var bkgColList = palette.Where(x => ThresholdPalette.Where(y => x.Distance(y, ColorDistanceEvaluationMode.RGB) <= ToleranceFactor).Any()).ToList();
                var foreColList = palette.Where(x => !bkgColList.Contains(x)).ToList();

                var colorTransformationMap = new ColorTransformationMap();
                foreach (var col in bkgColList)
                {
                    colorTransformationMap.Add(col, bgCol);
                }
                foreach (var col in foreColList)
                {
                    colorTransformationMap.Add(col, fgCol);
                }

                var imageOut =colorTransformationMap.Transform(image, cancellationToken);
                return ColorTransformResult.CreateValidResult(image, imageOut, $"Global threshold applied with tolerance factor {ToleranceFactor} and invert results set to {InvertResults}.", 1.0);
            }

            catch (OperationCanceledException cex)
            {
                LogMan.Exception(sC, sM, $"Operation canceled",cex);
                return ColorTransformResult.CreateErrorResult(image, null, $"Operation canceled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                throw;
            }
        }
    }
}
