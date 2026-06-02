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
        static string sClass = nameof(DitherBase);
        public string Description { get; protected init; }
        public ColorDithering Type { get; protected init; }
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.Full;
        public double DitheringStrenght { get; set; } = 1.0;
        public abstract DitherInterface Create();
        protected abstract ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);

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

                switch (DitheringFx)
                {
                    case ColorDitheringFx.ScanlineEven:
                        for (var r = 1; r < mi.GetLength(0); r+=2)
                        {
                            for (int c = 0; c < mi.GetLength(1); c++)
                            {
                                mo[r, c] = mi[r, c];
                            }
                        }
                        break;
                    case ColorDitheringFx.ScanlineOdd:
                        for (var r = 0; r < mi.GetLength(0); r += 2)
                        {
                            for (int c = 0; c < mi.GetLength(1); c++)
                            {
                                mo[r, c] = mi[r, c];
                            }
                        }
                        break;
                    case ColorDitheringFx.ColumnEven:
                        for (var r = 0; r < mi.GetLength(0); r ++)
                        {
                            for (int c = 1; c < mi.GetLength(1); c+=2)
                            {
                                mo[r, c] = mi[r, c];
                            }
                        }
                        break;
                    case ColorDitheringFx.ColumnOdd:
                        for (var r = 0; r < mi.GetLength(0); r ++)
                        {
                            for (int c = 0; c < mi.GetLength(1); c+=2)
                            {
                                mo[r, c] = mi[r, c];
                            }
                        }
                        break;
                    default:
                        return ColorTransformResult.CreateErrorResult($"unsupported dither fx : {DitheringFx}");
                }
                var imageFX = new ImageData().Create(mo);
                return ColorTransformResult.CreateValidResult(ditheredResult.DataOut, imageFX);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sM, ex);
                return ColorTransformResult.CreateErrorResult(ex);
            }

        }
    }
}
