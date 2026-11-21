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
    /// Colors close to one of the <see cref="ThresholdOptions.BackgroundColors"/> become background, the others foreground
    /// (or the opposite when <see cref="ThresholdOptions.InvertResults"/> is set).
    /// </summary>
    public class GlobalThreshold
    {
        static string sC = nameof(GlobalThreshold);

        /// <summary>
        /// Threshold settings.
        /// </summary>
        /// <exception cref="ArgumentNullException">Set to null.</exception>
        public ThresholdOptions Options
        {
            get => options;
            set => options = value ?? throw new ArgumentNullException(nameof(value));
        }
        ThresholdOptions options = new();

        /// <summary>
        /// Applies the threshold.
        /// </summary>
        /// <returns>A result whose output is the binary image (shape colors), or an error result.</returns>
        public ColorTransformResult Process(ImageData image, CancellationToken cancellationToken = default)
        {
            string sM = nameof(Process);
            try
            {
                ImageData.AssertValid(image);
                var opts = Options;
                var errors = opts.Validate().ToList();
                if (errors.Count > 0)
                {
                    LogMan.Error(sC, sM, $"invalid options : {string.Join("; ", errors)}");
                    return ColorTransformResult.CreateErrorResult(sC, sM, image, null, $"invalid options : {string.Join("; ", errors)}");
                }
                var thresholdPalette = new Palette().Create(opts.BackgroundColors);
                var backgroundColor = opts.InvertResults ? ColorDefaults.DefaultShapeForeColorInt : ColorDefaults.DefaultShapeBkgColorInt;
                var foregroundColor = opts.InvertResults ? ColorDefaults.DefaultShapeBkgColorInt : ColorDefaults.DefaultShapeForeColorInt;

                // Colors within Tolerance of any background color are background, all the others foreground
                var palette = new HistogramRGB().Create(image).ToPalette();

                var backgroundColors = palette.Where(x => thresholdPalette.Where(y => x.Distance(y, ColorDistanceEvaluationMode.RGB) <= opts.Tolerance).Any()).ToList();
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
                return ColorTransformResult.CreateValidResult(image, imageOut, $"Global threshold applied with tolerance {opts.Tolerance} and invert results set to {opts.InvertResults}.", 1.0);
            }

            catch (OperationCanceledException cex)
            {
                LogMan.Exception(sC, sM, $"Operation canceled",cex);
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, "threshold cancelled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, "threshold failed", ex);
            }
        }
    }
}
