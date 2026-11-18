using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Base class for dithering algorithms.
    /// <para/>
    /// Implements the <see cref="Dither"/> template: the algorithm specific <see cref="DitherImplementation"/>
    /// runs first, then the optional <see cref="DitheringFx"/> post effect is applied.
    /// </summary>
    public abstract partial class DitherBase : DitherInterface
    {
        static string sC = nameof(DitherBase);

        /// <inheritdoc/>
        public string Description { get; protected init; }

        /// <inheritdoc/>
        public ColorDithering Type { get; protected init; }

        /// <summary>Post-dithering effect applied after the algorithm.</summary>
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

        /// <inheritdoc/>
        public double DitheringStrenght { get; set; } = 1.0;

        /// <inheritdoc/>
        public abstract DitherInterface Create();

        /// <summary>
        /// Algorithm specific dithering step.
        /// </summary>
        /// <param name="imageReference">Original image</param>
        /// <param name="imageProcessed">Color-reduced image</param>
        /// <param name="colorEvaluationMode">Color distance metric</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The dithered image wrapped in a result.</returns>
        protected abstract ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);

        /// <summary>
        /// Dithers <paramref name="imageProcessed"/> towards the colors of <paramref name="imageReference"/>,
        /// then applies <see cref="DitheringFx"/>.
        /// </summary>
        /// <param name="imageReference">The original image to be used as a reference for dithering</param>
        /// <param name="imageProcessed">The image to be dithered</param>
        /// <param name="colorEvaluationMode">The mode for evaluating color distance</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The result of the dithering operation</returns>
        public ColorTransformResult Dither(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
        {
            string sM = nameof(Dither);
            var ret = DitherImplementation(imageReference, imageProcessed, colorEvaluationMode, token);
            if (!ret.IsSuccess)
            {
                return ret;
            }
            var retD = ApplyDitherFx(imageProcessed, ret, token);
            return retD;
        }

        /// <summary>
        /// Applies the <see cref="DitheringFx"/> effect: every other row (or column) of the dithered
        /// image is replaced with the corresponding undithered pixels of <paramref name="imageProcessed"/>.
        /// </summary>
        /// <param name="imageProcessed">Undithered, color-reduced image</param>
        /// <param name="ditheredResult">Result of the dithering step</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The result with the effect applied, or <paramref name="ditheredResult"/> when no effect is set.</returns>
        protected ColorTransformResult ApplyDitherFx(ImageData imageProcessed, ColorTransformResult ditheredResult, CancellationToken token = default)
        {
            string sM = nameof(ApplyDitherFx);
            if (DitheringFx == ColorDitheringFx.None)
            {
                return ditheredResult;
            }            
            if (imageProcessed == null)
            {
                return ColorTransformResult.CreateErrorResult(sC, sM, imageProcessed, ditheredResult?.DataOut, $"{Type} : invalid undithered image");
            }
            if (ditheredResult == null || !ditheredResult.IsSuccess || ditheredResult.DataOut == null )
            {
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, imageProcessed, $"{Type} : invalid dithering result", ditheredResult);
            }
            try
            {
                var matrixIn = imageProcessed.GetMatrix();
                var matrixOut = ditheredResult.DataOut.GetMatrix();
                // Start row/column and step of the pixels restored from the undithered image
                int rowStart = 0;
                int rowStep = 1;
                int columnStart = 0;
                int columnStep = 1;

                switch (DitheringFx)
                {
                    case ColorDitheringFx.ScanlineEven:
                        rowStart = 1; 
                        rowStep =2;
                        break;
                    case ColorDitheringFx.ScanlineOdd:
                        rowStart = 0;
                        rowStep = 2;
                        break;
                    case ColorDitheringFx.ColumnEven:
                        columnStart = 1;
                        columnStep =2;
                        break;
                    case ColorDitheringFx.ColumnOdd:
                        columnStart = 0;
                        columnStep = 2;
                        break;
                    default:
                        return ColorTransformResult.CreateErrorResult(sC, sM, imageProcessed, ditheredResult.DataOut, $"{Type} : unsupported dither fx {DitheringFx}");
                }

               
                for (var r = rowStart; r < matrixIn.GetLength(0); r += rowStep)
                {
                    for (int c = columnStart; c < matrixIn.GetLength(1); c += columnStep)
                    {
                        matrixOut[r, c] = matrixIn[r, c];
                    }
                    token.ThrowIfCancellationRequested();
                }

                var imageFx = new ImageData().Create(matrixOut);
                return ColorTransformResult.CreateValidResult(ditheredResult.DataOut, imageFx);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : dithering effect cancelled", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, imageProcessed, ditheredResult.DataOut, $"{Type} : dithering effect cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : dithering effect failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, imageProcessed, ditheredResult.DataOut, $"{Type} : dithering effect failed", ex);
            }

        }
    }
}
