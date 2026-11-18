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
    /// <summary>
    /// Global color threshold: converts an image to a binary shape image.
    /// <para/>
    /// Colors close to one of the <see cref="ThresholdPalette"/> colors become background, the others foreground
    /// (or the opposite when <see cref="InvertResults"/> is set).
    /// </summary>
    public class GlobalThreshold
    {
        static string sC = nameof(GlobalThreshold);
        /// <summary>
        /// Background reference colors (magenta by default).
        /// </summary>
        public Palette ThresholdPalette { get; set; } = new();
        /// <summary>
        /// Maximum (squared RGB) distance from a threshold color to be considered background.
        /// </summary>
        public double ToleranceFactor { get; set; } = 0.0;
        /// <summary>
        /// Swaps foreground and background.
        /// </summary>
        public bool InvertResults { get; set; } = false;

        /// <summary>
        /// Creates a threshold on the default magenta background.
        /// </summary>
        public GlobalThreshold()
        {
            ThresholdPalette.Create(new List<int>() { ColorIntExt.FromRGB(255, 0, 255) });
        }

        /// <summary>
        /// Sets the threshold parameters (fluent).
        /// </summary>
        public GlobalThreshold WithThreshold(Palette thresholdPalette, double toleranceFactor = 0.0, bool invertResults = false)
        {
            ThresholdPalette = new Palette().Create(thresholdPalette);
            ToleranceFactor = toleranceFactor;
            InvertResults = invertResults;
            return this;
        }

        /// <summary>
        /// Creates a deep copy.
        /// </summary>
        public GlobalThreshold Clone()
        {
            return new GlobalThreshold()
            {
                ThresholdPalette = new Palette().Create(this.ThresholdPalette),
                ToleranceFactor = this.ToleranceFactor,
                InvertResults = this.InvertResults
            };
        }

        /// <summary>
        /// Sets a single threshold color and the parameters (fluent).
        /// </summary>
        public GlobalThreshold WithThreshold(int rgbThreshold, double toleranceFactor = 0.0, bool invertResults = false)
        {
            return WithThreshold(new Palette().Create(new List<int> { rgbThreshold }), toleranceFactor, invertResults);
        }

        /// <summary>
        /// Applies the threshold.
        /// </summary>
        /// <returns>A result whose output is the binary image (shape colors).</returns>
        public ColorTransformResult Process(ImageData image, CancellationToken cancellationToken = default)
        {
            string sM = nameof(Process);
            try
            {
                ImageData.AssertValid(image);
                var backgroundColor = InvertResults ? ColorDefaults.DefaultShapeForeColorInt : ColorDefaults.DefaultShapeBkgColorInt;
                var foregroundColor = InvertResults ? ColorDefaults.DefaultShapeBkgColorInt : ColorDefaults.DefaultShapeForeColorInt;

                // Colors within ToleranceFactor of any threshold color are background, all the others foreground
                var palette = new HistogramRGB().Create(image).ToPalette();

                var backgroundColors = palette.Where(x => ThresholdPalette.Where(y => x.Distance(y, ColorDistanceEvaluationMode.RGB) <= ToleranceFactor).Any()).ToList();
                var foregroundColors = palette.Where(x => !backgroundColors.Contains(x)).ToList();

                var colorTransformationMap = new ColorTransformationMap();
                foreach (var color in backgroundColors)
                {
                    colorTransformationMap.Add(color, backgroundColor);
                }
                foreach (var color in foregroundColors)
                {
                    colorTransformationMap.Add(color, foregroundColor);
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
