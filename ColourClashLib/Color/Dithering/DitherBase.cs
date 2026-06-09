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
    /// Base class for Dithering algorithms
    /// </summary>
    public abstract partial class DitherBase : DitherInterface
    {
        static string sC = nameof(DitherBase);
        public string Description { get; protected init; }
        public ColorDithering Type { get; protected init; }
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.Full;
        public double DitheringStrenght { get; set; } = 1.0;
        public abstract DitherInterface Create();
        protected abstract ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);

        /// <summary>
        /// Dither the imageProcessed using the imageReference as reference for the colors to be dithered, and the colorEvaluationMode to evaluate the color distance between the original and the dithered colors.
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

        protected ColorTransformResult ApplyDitherFx(ImageData imageProcessed, ColorTransformResult ditheredResult, CancellationToken token = default)
        {
            string sM = nameof(ApplyDitherFx);
            if (DitheringFx == ColorDitheringFx.Full)
            {
                return ditheredResult;
            }            
            if (imageProcessed == null)
            {
                return ColorTransformResult.CreateErrorResult("invalid input imageProcessed");
            }
            if (ditheredResult == null || !ditheredResult.IsSuccess || ditheredResult.DataOut == null )
            {
                return ColorTransformResult.CreateErrorResult("invalid input preprocessed result");
            }
            try
            {
                var mi = imageProcessed.GetMatrix();
                var mo = ditheredResult.DataOut.GetMatrix();
                int rs = 0;
                int stepr = 1;
                int cs = 0;
                int stepc = 1;

                switch (DitheringFx)
                {
                    case ColorDitheringFx.ScanlineEven:
                        rs = 1; 
                        stepr=2;
                        break;
                    case ColorDitheringFx.ScanlineOdd:
                        rs = 0;
                        stepr = 2;
                        break;
                    case ColorDitheringFx.ColumnEven:
                        cs = 1;
                        stepc=2;
                        break;
                    case ColorDitheringFx.ColumnOdd:
                        cs = 0;
                        stepc = 2;
                        break;
                    default:
                        return ColorTransformResult.CreateErrorResult($"unsupported dither fx : {DitheringFx}");
                }

               
                for (var r = rs; r < mi.GetLength(0); r += stepr)
                {
                    for (int c = cs; c < mi.GetLength(1); c+=stepc)
                    {
                        mo[r, c] = mi[r, c];
                    }
                    token.ThrowIfCancellationRequested();
                }

                var imageFX = new ImageData().Create(mo);
                return ColorTransformResult.CreateValidResult(ditheredResult.DataOut, imageFX);
            }
            catch (TaskCanceledException et)
            {
                LogMan.Exception(sC, sM, $"{Type}", et);
                return ColorTransformResult.CreateErrorResult(et);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type}", ex);
                return ColorTransformResult.CreateErrorResult(ex);
            }

        }
    }
}
