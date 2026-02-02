using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionAmiga;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionZxSpectrum : ColorTransformReductionPalette
    {
        static string sC = nameof(ColorTransformReductionZxSpectrum);

        public enum ZxPaletteMode
        {
            Both = 0,
            PaletteLo = 1,
            PaletteHi = 2,
            ImageZxReference = 3,
        }

        public enum ZxAutotuneMode
        {
            None = 0,
            Fast = 1,
            Detailed = 2,
        }

        public ColorTransformReductionZxSpectrum()
        {
            Type = ColorTransformType.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        public class ZxProcessingResults
        {
            public int ColorSeedInLow { get; internal set; }
            public int ColorSeedInHigh { get; internal set; }
            public ImageData? ProcessedImageLow { get; private set; }
            public ImageData? ProcessedImageHigh { get; private set; }
            public ImageData? MergeImage { get; internal set; }
            public double MergeError { get; internal set; }
            public TileManager? TileManagerLow { get; private set; }
            public TileManager? TileManagerHigh { get; private set; }

            internal bool Create(TileManager tileManLo, TileManager tileManHi, ImageData referenceImage, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
            {
                TileManagerLow = tileManLo;
                TileManagerHigh = tileManHi;
                ProcessedImageLow = tileManLo?.CreateImageFromTiles();
                ProcessedImageHigh = tileManHi?.CreateImageFromTiles();
                MergeImage = TileManager.MergeToImage(new List<TileManager?> { tileManLo, tileManHi });
                MergeError = ColorIntExt.EvaluateError(referenceImage, MergeImage, colorEvaluationMode, token);
                return true;
            }
        }


        int zxLowColorInSeed = 0x0080;
        public int ZxLowColorInSeed
        {
            get => zxLowColorInSeed;
            set
            {
                zxLowColorInSeed = Math.Min(Math.Max(8, value), zxHighColorInSeed);
            }
        }

        int zxHighColorInSeed = 0x00FF;
        public int ZxHighColorInSeed
        {
            get => zxHighColorInSeed;
            set
            {
                zxHighColorInSeed = Math.Min(Math.Max(zxLowColorInSeed, value), 0x00FF);
            }
        }

        public ZxPaletteMode PaletteMode { get; set; } = ZxPaletteMode.Both;
        public bool IncludeBlackInHighColor { get; set; } = true;
        public ZxAutotuneMode AutotuneMode { get; set; } = ZxAutotuneMode.Fast;
        public bool DitherLowColorImage { get; set; } = true;
        public bool DitherHighColorImage { get; set; } = true;
        public int ZxColorSeedOutLow { get; init; } = 0x00D8;
        public int ZxColorSeedOutHigh { get; init; } = 0x00FF;

        // ColorTransformType processingType { get; set; } = ColorTransformType.ColorReductionClustering;
        ColorTransformType processingType { get; set; } = ColorTransformType.ColorReductionFast;

        Dictionary<ColorTransformProperties, object> CreateTileProcessingParams(Palette palette, ColorDithering ditheringType)
        {
            var dict = new Dictionary<ColorTransformProperties, object>();
            dict[ColorTransformProperties.ColorDistanceEvaluationMode] = ColorDistanceEvaluationMode;
            dict[ColorTransformProperties.PriorityPalette] = palette;
            dict[ColorTransformProperties.DitheringType] = ditheringType;
            dict[ColorTransformProperties.DitheringStrength] = DitheringStrength;
            dict[ColorTransformProperties.MaxColorsWanted] = 2;
            dict[ColorTransformProperties.UseColorMean] = false;
            dict[ColorTransformProperties.ClusterTrainingLoop] = 5;
            return dict;
        }

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);

            switch (propertyName)
            {
                case ColorTransformProperties.ZxColLSeed:
                    ZxLowColorInSeed = ToInt(value);
                    break;
                case ColorTransformProperties.ZxColHSeed:
                    ZxHighColorInSeed = ToInt(value);
                    break;
                case ColorTransformProperties.ZxDitherLowColorImage:
                    DitherLowColorImage = ToBool(value);
                    break;
                case ColorTransformProperties.ZxDitherHighColorImage:
                    DitherHighColorImage = ToBool(value);
                    break;
                case ColorTransformProperties.ZxIncludeBlackInHighColorImage:
                    IncludeBlackInHighColor = ToBool(value);
                    break;
                case ColorTransformProperties.ZxPaletteMode:
                    PaletteMode = ToEnum<ZxPaletteMode>(value);
                    break;
                case ColorTransformProperties.ZxAutotuneMode:
                    AutotuneMode = ToEnum<ZxAutotuneMode>(value);
                    break;
                default:
                    break;
            }
            return this;
        }


        ColorTransformationMap CreateZxMap(int colorSeedIn, int colorSeedOut, bool mapTheBlack)
        {
            ColorTransformationMap oMap = new ColorTransformationMap();
            if (mapTheBlack)
            {
                oMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            }
            oMap.Add(ColorIntExt.FromRGB(0, 0, colorSeedIn), ColorIntExt.FromRGB(0, 0, colorSeedOut));
            oMap.Add(ColorIntExt.FromRGB(colorSeedIn, 0, 0), ColorIntExt.FromRGB(colorSeedOut, 0, 0));
            oMap.Add(ColorIntExt.FromRGB(colorSeedIn, 0, colorSeedIn), ColorIntExt.FromRGB(colorSeedOut, 0, colorSeedOut));
            oMap.Add(ColorIntExt.FromRGB(0, colorSeedIn, 0), ColorIntExt.FromRGB(0, colorSeedOut, 0));
            oMap.Add(ColorIntExt.FromRGB(0, colorSeedIn, colorSeedIn), ColorIntExt.FromRGB(0, colorSeedOut, colorSeedOut));
            oMap.Add(ColorIntExt.FromRGB(colorSeedIn, colorSeedIn, 0), ColorIntExt.FromRGB(colorSeedOut, colorSeedOut, 0));
            oMap.Add(ColorIntExt.FromRGB(colorSeedIn, colorSeedIn, colorSeedIn), ColorIntExt.FromRGB(colorSeedOut, colorSeedOut, colorSeedOut));
            return oMap;
        }


        ImageData CreateBestZxImage(CancellationToken token = default)
        {
            var zxPalette = new Palette();
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            var l = ZxColorSeedOutLow;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));
            l = ZxColorSeedOutHigh;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));

            var zxBaseTransform = new ColorTransformReductionPalette()
               .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
               .SetProperty(ColorTransformProperties.PriorityPalette, zxPalette)
               .SetProperty(ColorTransformProperties.DitheringType, DitheringType)
               .SetProperty(ColorTransformProperties.DitheringStrength, DitheringStrength)
               .Create(ImageSource);
            var zxBaseResult = zxBaseTransform.ProcessColors(token);
            var zxBaseImage = zxBaseResult.DataOut;
            return zxBaseImage;
        }

        TileManager CreateAndProcessTiles(bool loPalette, ImageData zxBestImage, ColorTransformationMap transformationMap, bool useDithering, CancellationToken token = default)
        {
            var dithering = useDithering ? DitheringType : ColorDithering.None;

            //Create best input image on input seed palette
            var zxBaseTransform = new ColorTransformReductionPalette()
               .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
               .SetProperty(ColorTransformProperties.PriorityPalette, transformationMap.GetInputPalette())
               .SetProperty(ColorTransformProperties.DitheringType, DitheringType)
               .SetProperty(ColorTransformProperties.DitheringStrength, DitheringStrength)
               .Create(ImageSource,ImageReference);
            var zxBaseResult = zxBaseTransform.ProcessColors(token);
            var zxBaseImage = zxBaseResult.DataOut;

            // transform to real ZX colors
            var zxRealImage = transformationMap.Transform(zxBaseImage, token);
            // evaluate tiles 8x8 - 2 colors per tile
            TileManager oTileManager = new TileManager().Create(8, 8, zxRealImage, loPalette ? 1.0 : 2.0, processingType, CreateTileProcessingParams(new Palette(), dithering), token); // ColorDithering.None);, token);//  ithering), token);
            var tileProcRes = oTileManager.ProcessColors(token);
            var normalization = ColorIntExt.GetMaxColorDistance(transformationMap.GetOutputPalette(), ColorDistanceEvaluationMode, token);
            var dError = oTileManager.RecalcGlobalTransformationError(zxBestImage, token);
            //LogMan.Warning(sC, "demo", $"{loPalette} : ERR = {dError}");
            //RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            //{
            //    ProcessingResults = ColorTransformResult.CreateValidResult(null, zxBaseImage)
            //});

            return oTileManager;
        }


        protected ZxProcessingResults ExecuteTransformZx(ImageData zxBestImage, int colorSeedInLo, int colorSeedInHi, CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformZx);
            var zxTransformationMapLo = CreateZxMap(colorSeedInLo, ZxColorSeedOutLow, true);
            var zxTransformationMapHi = CreateZxMap(colorSeedInHi, ZxColorSeedOutHigh, IncludeBlackInHighColor);

            List<Task<TileManager>> lTaskList = new List<Task<TileManager>>();
            var procResults = new ZxProcessingResults()
            {
                ColorSeedInLow = colorSeedInLo,
                ColorSeedInHigh = colorSeedInHi,
            };
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteLo:
                    {
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, DitherLowColorImage, token)));
                        lTaskList.Add(null);
                    }
                    break;
                case ZxPaletteMode.PaletteHi:
                    {
                        lTaskList.Add(null);
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                case ZxPaletteMode.Both:
                    {
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, DitherLowColorImage, token)));
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown ZX Spectrum palette mode: {PaletteMode}");
            }

            // Await task completed
            var lTaskListValid = lTaskList.Where(X => X != null).ToList();
            Task.WaitAll(lTaskListValid.ToArray());
            procResults.Create(lTaskList[0]?.Result, lTaskList[1]?.Result, ImageSource, ColorDistanceEvaluationMode, token);
            return procResults;
        }




        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            BypassDithering = true;

            var bestZxImage = CreateBestZxImage(token);
            //bestZxImage = ImageSource;
            if (PaletteMode == ZxPaletteMode.ImageZxReference)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, bestZxImage);
            }

            if (AutotuneMode == ZxAutotuneMode.None)
            {
                var processingResult = ExecuteTransformZx(bestZxImage, ZxLowColorInSeed, ZxHighColorInSeed, token);
                TransformationError = processingResult.MergeError;
                return ColorTransformResult.CreateValidResult(ImageSource, processingResult.MergeImage);// oTuple.Item1);
            }
            else
            {
                ZxProcessingResults bestResult = null;
                int step = 32;
                int cycle = 0;
                int start = ZxLowColorInSeed;
                int endL = 256 + (step / 2);
                int endH = 256 + (step / 2);
                int loop = AutotuneMode == ZxAutotuneMode.Fast ? 1 : 2;
                for (int i = 0; i < loop; i++)
                {
                    for (int low = start; low < endL; low += step)
                    {
                        int iL = Math.Min(255, low);
                        for (int high = low + step - 1; high < endH; high += step, cycle++)
                        {
                            int iH = Math.Min(255, high);
                            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                            {
                                ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"step {cycle} : Range [{iL} - {iH}]")
                            });
                            token.ThrowIfCancellationRequested();
                            var processingResult = ExecuteTransformZx(bestZxImage, iL, iH, token);

                            if ((bestResult?.MergeError ?? double.PositiveInfinity) > processingResult.MergeError)
                            {
                                bestResult = processingResult;
                                RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                                {
                                    ColorTransformInterface = this,
                                    CompletedPercent = ((cycle * step * step) / ((256.0 - ZxLowColorInSeed) * (256.0 - ZxLowColorInSeed))) * 100.0,
                                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, processingResult?.MergeImage, $"step {cycle} : Range [{iL} - {iH}] : Error = {processingResult?.MergeError}"),
                                });
                            }
                        }
                    }
                    step = 8;
                    start = (int)(bestResult.ColorSeedInLow - 16);
                    endL = (int)(bestResult.ColorSeedInLow + 16);
                    endH = bestResult.ColorSeedInHigh + 9;
                    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                    {
                        ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine Low palette : Range [{start} - {endL}]")
                    });
                    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                    {
                        ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine High palette : until {endH}")
                    });
                }


                if (bestResult?.MergeImage != null)
                {
                    return ColorTransformResult.CreateValidResult(ImageSource, bestResult?.MergeImage);// oFinalData);
                }
                else
                {
                    return ColorTransformResult.CreateErrorResult("Image process creation error");
                }
            }
        }
    }
}




     