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
    /// <summary>
    /// Commodore Amiga specific color reductions: HAM6, HAM8 and Extra Half-Brite.
    /// <para/>
    /// HAM (Hold And Modify) pixels either use a base palette color or copy the previous pixel changing only one channel:
    /// the image is quantized, a base palette is computed, then each line is encoded choosing the best HAM option per pixel.
    /// </summary>
    public class ColorTransformCbmAmiga : ColorTransformBase
    {
        static string sC = nameof(ColorTransformCbmAmiga);


        /// <summary>
        /// Amiga video modes.
        /// </summary>
        public enum EnumAmigaVideoMode
        {
            /// <summary>
            /// Hold And Modify, 16 base colors, 4 bits per channel (OCS/ECS).
            /// </summary>
            Ham6,
            /// <summary>
            /// Hold And Modify, 64 base colors, 6 bits per channel (AGA).
            /// </summary>
            Ham8,
            /// <summary>
            /// 32 colors plus their half-bright copies (64 colors).
            /// </summary>
            ExtraHalfBright
        }

        /// <summary>
        /// Target video mode.
        /// </summary>
        public EnumAmigaVideoMode AmigaVideoMode 
        { 
            get => config.AmigaVideoMode;
            set => config.AmigaVideoMode = value;
        }
            
        /// <summary>
        /// Reduction used to compute the base palette.
        /// </summary>
        public  ColorTransformType HamColorTransformationModel
        { 
            get => config.AmigaInternalTransformationModel;
            set => config.AmigaInternalTransformationModel = value;
        }

        /// <summary>
        /// Creates the Amiga transformation.
        /// </summary>
        public ColorTransformCbmAmiga()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Commodore Amiga specific color reduction";
        }


        /// <summary>
        /// Sets the Amiga settings (fluent).
        /// </summary>
        public ColorTransformCbmAmiga WithAmigaProperties(EnumAmigaVideoMode videoMode, ColorTransformType hamColorTransformationModel )
        {
            AmigaVideoMode = videoMode;
            HamColorTransformationModel = hamColorTransformationModel;
            return this;
        }

        /// <summary>
        /// Sets the Amiga settings from a configuration (fluent).
        /// </summary>
        public ColorTransformCbmAmiga WithAmigaProperties(ColorTransformConfig cfg) => WithAmigaProperties(cfg.AmigaVideoMode, cfg.AmigaInternalTransformationModel);





        /// <summary>
        /// Encodes the image in HAM mode, line by line.
        /// </summary>
        /// <param name="dataSource">Original image</param>
        /// <param name="dataPreProcessed">Image reduced to the base palette</param>
        /// <param name="quantization">Channel bit depth of the HAM mode</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The HAM image as displayed.</returns>
        ImageData ToHam(ImageData dataSource, ImageData dataPreProcessed, ColorQuantizationMode quantization, CancellationToken token = default)
        {

            var quantizer = new ColorTransformQuantization();
            quantizer.QuantizationMode = quantization;
            quantizer.Create(dataPreProcessed);

            var hamData = new int[dataSource.Rows, dataSource.Columns];
            for (int r = 0; r < dataSource.Rows; r++)
            {
                int rgbPrev = ColorDefaults.DefaultInvalidColorInt;
                var palette = new Palette();
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < dataSource.Columns; c++)
                {
                    // First pixel of a line (or after a tagged pixel): there is no previous color to modify,
                    // so a base palette color must be used
                    if (rgbPrev < 0)
                    {
                        rgbPrev = dataPreProcessed.matrix[r, c];
                        hamData[r, c] = rgbPrev;
                        continue;
                    }
                    else
                    {
                        int rgbSrc = quantizer.QuantizeColor( dataSource.matrix[r, c] );
                        if (rgbSrc < 0)
                        {
                            rgbPrev = ColorDefaults.DefaultInvalidColorInt;
                            hamData[r, c] = rgbSrc;
                            continue;
                        }
                        int prevR = rgbPrev.ToR();
                        int prevG = rgbPrev.ToG();
                        int prevB = rgbPrev.ToB();
                        int srcR = rgbSrc.ToR();
                        int srcG = rgbSrc.ToG();
                        int srcB = rgbSrc.ToB();
                        // HAM candidates: the base palette color, or the previous pixel with one channel modified
                        int hamOriginal = quantizer.QuantizeColor( dataPreProcessed.matrix[r,c] );
                        int hamR = ColorIntExt.FromRGB(srcR, prevG, prevB);
                        int hamG = ColorIntExt.FromRGB(prevR, srcG, prevB);
                        int hamB = ColorIntExt.FromRGB(prevR, prevG, srcB);
                        palette.Reset();
                        palette.Add(hamOriginal);
                        palette.Add(hamR);
                        palette.Add(hamG);
                        palette.Add(hamB);
                        var hamColor = quantizer.QuantizeColor( ColorIntExt.GetNearestColor(rgbSrc, palette, ColorDistanceEvaluationMode) );
                        rgbPrev = hamColor;
                        hamData[r,c] = hamColor;
                    }
                }
            }
            return new ImageData().Create( hamData );
        }



        /// <summary>
        /// Reduces the image to 32 Extra Half-Brite clusters (each base color also provides its half-bright copy).
        /// </summary>
        /// <returns>A valid result with the EHB image, or an error result.</returns>
        ColorTransformResult ToEHB(ImageData dataSource, ImageData dataPreProcessed, CancellationToken token = default)
        {
            string sM = nameof(ToEHB);
            var transform = ForwardEventsFrom(new ColorTransformClusteringEHB());
            transform.WithClustering(config)
                .WithColorDistanceEvaluationMode(config)
                .WithDithering(DitheringConfig);
            transform.MaxColorsWanted = 32;
            var result = transform.CreateAndProcessColors(dataPreProcessed, token);
            if (result == null || !result.IsSuccess || result.DataOut == null)
            {
                LogMan.Error(sC, sM, $"{Type} : EHB clustering failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, dataSource, $"{Type} : EHB clustering failed", result);
            }
            return ColorTransformResult.CreateValidResult(dataSource, result.DataOut);
        }


        /// <summary>
        /// Runs the quantization and the mode specific encoding.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            try
            {
                var quantization = ForwardEventsFrom(new ColorTransformQuantization());
                quantization.WithQuantization(ColorQuantizationMode.RGB444)
                             .WithReferencePalette(ReferencePalette)
                             .WithDithering(DitheringConfig);

                int maxColors;
                ColorQuantizationMode hamQuantization;
                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.ExtraHalfBright:
                        {
                            var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
                            if (resultQuantized == null || !resultQuantized.IsSuccess)
                            {
                                LogMan.Error(sC, sM, $"{Type} : RGB444 quantization failed");
                                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : RGB444 quantization failed", resultQuantized);
                            }
                            var ehbResult = ToEHB(ImageSource, resultQuantized.DataOut, token);
                            return ehbResult.IsSuccess
                                ? ColorTransformResult.CreateValidResult(ImageSource, ehbResult.DataOut)
                                : ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : EHB conversion failed", ehbResult);
                        }
                    case EnumAmigaVideoMode.Ham6:
                        maxColors = 16;
                        hamQuantization = ColorQuantizationMode.RGB444;
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        maxColors = 64;
                        hamQuantization = ColorQuantizationMode.RGB666;
                        break;
                    default:
                        LogMan.Error(sC, sM, $"{Type} : unsupported video mode {AmigaVideoMode}");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : unsupported video mode {AmigaVideoMode}");
                }

                quantization.WithQuantization(hamQuantization);
                var quantizedResult = quantization.CreateAndProcessColors(ImageSource, token);
                if (quantizedResult == null || !quantizedResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : {hamQuantization} quantization failed");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {hamQuantization} quantization failed", quantizedResult);
                }

                // Compute the HAM base palette (16 or 64 colors) on the quantized image
                var cfg = config.Clone()
                    .WithMaxColorWanted(maxColors);
                var colorReduction = ForwardEventsFrom(ColorTransformInternal.Alloc(cfg, cfg.AmigaInternalTransformationModel));
                colorReduction.SetProperties(cfg);
                var dataPreprocessedResult = colorReduction.CreateAndProcessColors(quantizedResult.DataOut, token);
                if (dataPreprocessedResult == null || !dataPreprocessedResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : HAM base palette reduction failed");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : HAM base palette reduction failed", dataPreprocessedResult);
                }
                BypassDithering = true;

                var amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, hamQuantization, token);
                return ColorTransformResult.CreateValidResult(ImageSource, amigaData);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : operation cancelled", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : {AmigaVideoMode} conversion failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : {AmigaVideoMode} conversion failed", ex);
            }
        }

    }
}
