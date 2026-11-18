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
        /// Reduces the image to 32 clusters for the Extra Half-Brite mode.
        /// </summary>
        ImageData ToEHB(ImageData dataSource, ImageData dataPreProcessed, CancellationToken token = default)
        {

            // EHB clusters: each of the 32 base colors also provides its half-bright copy
            var transform = new ColorTransformClusteringEHB()
                .WithClustering(config)
                .WithColorDistanceEvaluationMode(config)
                .WithDithering(DitheringConfig) as ColorTransformClusteringEHB;
            transform.MaxColorsWanted = 32;
            transform.ProcessAdvance += (s,e)=> RaiseProcessPartialEvent(e);
            var result = transform.CreateAndProcessColors(dataPreProcessed, token);
            return new ImageData().Create(result.DataOut);
        }


        /// <summary>
        /// Runs the quantization and the mode specific encoding.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            ImageData? amigaData;

            var quantization = new ColorTransformQuantization();
            quantization.WithQuantization(ColorQuantizationMode.RGB444)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering(DitheringConfig);

            if (AmigaVideoMode == EnumAmigaVideoMode.ExtraHalfBright)
            {
                var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
                amigaData = ToEHB(ImageSource, resultQuantized.DataOut, token);
            }
            else
            {

                int maxColors = 0;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        maxColors = 16;
                        quantization.WithQuantization(ColorQuantizationMode.RGB444);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        maxColors = 64;
                        quantization.WithQuantization(ColorQuantizationMode.RGB666);
                        break;
                    default:
                        return null;
                }

                var quantizedResult = quantization.CreateAndProcessColors(ImageSource, token);

                // Compute the HAM base palette (16 or 64 colors) on the quantized image
                var cfg = config.Clone()
                    .WithMaxColorWanted(maxColors);
                var colorReduction = ColorTransformInternal.Alloc(cfg, cfg.AmigaInternalTransformationModel) as ColorTransformBase;
                colorReduction.SetProperties(cfg);
                var dataPreprocessedResult = colorReduction.CreateAndProcessColors(quantizedResult.DataOut, token);
                BypassDithering = true;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, ColorQuantizationMode.RGB444, token);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, ColorQuantizationMode.RGB666, token);
                        break;
                    default:
                        amigaData = null;
                        break;
                }
            }
            if (amigaData == null)
            {
                LogMan.Error(sC, sM, $"Failed to generate Amiga transformed data");
                return ColorTransformResult.CreateErrorResult(ImageSource,null, $"{sC}.{sM} : Amiga transformed data is null");
            }
            return ColorTransformResult.CreateValidResult(ImageSource, amigaData);
        }

    }
}
