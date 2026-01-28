using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionAmiga;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionZxSpectrum : ColorTransformReductionPalette
    {
        static string sC = nameof(ColorTransformReductionZxSpectrum);

        public enum ZxPaletteMode
        {
            PaletteLo = 0,
            PaletteHi = 1,
            Both = 2,
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
            public ImageData? MergeImage { get; private set; }
            public double MergeError { get; private set; }
            public TileManager? TileManagerLow { get; private set; }
            public TileManager? TileManagerHigh { get; private set; }

            internal bool Create(TileManager tileManLo, TileManager tileManHi, ImageData referenceImage, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token= default)
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

        ColorTransformType processingType { get; set; } = ColorTransformType.ColorReductionFast;

        Dictionary<ColorTransformProperties, object> CreateProcessingParams(Palette palette, ColorDithering ditheringType)
        {
            var dict = new Dictionary<ColorTransformProperties, object>();
            dict[ColorTransformProperties.ColorDistanceEvaluationMode] = ColorDistanceEvaluationMode;
            dict[ColorTransformProperties.Fixed_Palette] = palette;
            dict[ColorTransformProperties.Forced_Palette] = palette;
            dict[ColorTransformProperties.Dithering_Type] = ColorDithering.None;  //DitheringType;
            dict[ColorTransformProperties.Dithering_Strength] = DitheringStrength;
            dict[ColorTransformProperties.MaxColorsWanted] = 2;
            dict[ColorTransformProperties.UseColorMean] = false;
            dict[ColorTransformProperties.ClusterTrainingLoop] = 5;
            return dict;
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
        public bool AutoTune { get; set; } = true;
        public bool DitherLowColorImage { get; set; } = true;
        public bool DitherHighColorImage { get; set; } = true;
        public int ZxColorSeedOutLow { get; init; } = 0x00D8;
        public int ZxColorSeedOutHigh { get; init; } = 0x00FF;

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);

            switch (propertyName)
            {
                case ColorTransformProperties.Zx_ColL_Seed:
                    ZxLowColorInSeed = ToInt(value);
                    break;
                case ColorTransformProperties.Zx_ColH_Seed:
                    ZxHighColorInSeed = ToInt(value);
                    break;
                case ColorTransformProperties.Zx_DitherLowColorImage:
                    DitherHighColorImage = ToBool(value);
                    break;
                case ColorTransformProperties.Zx_DitherHighColorImage:
                    DitherHighColorImage = ToBool(value);
                    break;
                case ColorTransformProperties.Zx_IncludeBlackInHighColorImage:
                    IncludeBlackInHighColor = ToBool(value);
                    break;
                case ColorTransformProperties.Zx_PaletteMode:
                    PaletteMode = ToEnum<ZxPaletteMode>(value);
                    break;
                case ColorTransformProperties.Zx_Autotune:
                    AutoTune = ToBool(value);
                    break;
                default:
                    break;
            }
            return this;
        }

      

        ColorTransformationMap CreateZxMap(int colorSeedIn, int  colorSeedOut, bool mapTheBlack)
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

        TileManager CreateAndProcessTiles(ColorTransformationMap transformationMap, bool useDithering, CancellationToken token = default)
        {
            var dithering = useDithering ? DitheringType : ColorDithering.None;

            //Create best input image on input seed palette
            var oPreProcessing = new ColorTransformReductionPalette()
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, transformationMap.GetInputPalette())
                .SetProperty(ColorTransformProperties.Dithering_Type, dithering )
                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrength)
                .Create(SourceData);
            var oPreRes = oPreProcessing.ProcessColors(token);

            // transform to real ZX colors
            var oPreData = transformationMap.Transform(oPreRes.DataOut, token);
            // evaluate tiles 8x8 - 2 colors per tile
            TileManager oTileManager = new TileManager().Create(8, 8, oPreData, processingType, CreateProcessingParams(new Palette(), dithering), token);
            var tileProcRes = oTileManager.ProcessColors(token);
            var dError = oTileManager.RecalcGlobalTransformationError(SourceData, token);
            return oTileManager;
        }


        protected  ZxProcessingResults ExecuteTransformZx(int colorSeedInLo, int  colorSeedInHi, CancellationToken token=default)
        {
            string sM = nameof(ExecuteTransformZx);
            var zxTransformationMapLo = CreateZxMap(colorSeedInLo, ZxColorSeedOutLow, true);
            var zxTransformationMapHi = CreateZxMap(colorSeedInHi, ZxColorSeedOutHigh, IncludeBlackInHighColor);
            List <Task<TileManager>> lTaskList = new List<Task<TileManager>>();
            var procResults = new ZxProcessingResults()
            {
                ColorSeedInLow = colorSeedInLo,
                ColorSeedInHigh = colorSeedInHi,
            };
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteLo:
                    {
                        lTaskList.Add(Task<TileManager>.Run(()=> CreateAndProcessTiles(zxTransformationMapLo, DitherLowColorImage, token)));
                        lTaskList.Add(null);
                    }
                    break;
                case ZxPaletteMode.PaletteHi:
                    {
                        lTaskList.Add(null);
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                case ZxPaletteMode.Both:
                    {
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(zxTransformationMapLo, DitherLowColorImage, token)));
                        lTaskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown ZX Spectrum palette mode: {PaletteMode}");
            }

            // Await task completed
            var lTaskListValid = lTaskList.Where(X => X != null).ToList(); 
            Task.WaitAll(lTaskListValid.ToArray());
            procResults.Create(lTaskList[0]?.Result, lTaskList[1]?.Result, SourceData, ColorDistanceEvaluationMode, token);
            return procResults;
        }



        object locker = new object();
        private double currentMinError;


      

        protected override ColorTransformResults ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            BypassDithering = true;

            if (!AutoTune)
            {
                var processingResult = ExecuteTransformZx(ZxLowColorInSeed, ZxHighColorInSeed, token);
                TransformationError = processingResult.MergeError;
                return ColorTransformResults.CreateValidResult(SourceData, processingResult.MergeImage);// oTuple.Item1);
            }
            else
            {
                // First tuning pass
                // Processing Color Range - [LBest, HBest]
                int bestSeedLow = ZxLowColorInSeed;
                int bestSeedHigh = ZxHighColorInSeed;
                var processingResult = ExecuteTransformZx(bestSeedLow, bestSeedHigh, token);
                var minError = double.PositiveInfinity;

                RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                {
                    ColorTransformInterface = this,
                    CompletedPercent = 0,
                    ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, processingResult.MergeImage, "first tuning", minError),
                });

                // Cycle setup
                int iStep = 0;
                bool bExit = false;
                while (!bExit)
                {
                    iStep++;
                    // Evaluate increasing low values and decreasing high values
                    var iL = bestSeedLow + 8;
                    var iH = bestSeedHigh - 8;
                    List<Task<ZxProcessingResults>> tasklist = new();
                    // Evaluate increasing low values; 
                    tasklist.Add(Task<ZxProcessingResults>.Run(() => ExecuteTransformZx(iL, bestSeedHigh, token)));
                    // Evaluate decreasing high values
                    tasklist.Add(Task<ZxProcessingResults>.Run(() => ExecuteTransformZx(bestSeedLow, iH, token)));
                    Task.WhenAll(tasklist);
                    var resultSeedLow = tasklist[0].Result;
                    var resultSeedHigh = tasklist[1].Result;
                    var errorSeedLow = resultSeedLow.MergeError;
                    var errorSeedHigh = resultSeedHigh.MergeError;
                    LogMan.Warning(sC, sM, $"------------------------------------");
                    LogMan.Warning(sC, sM, $"Current       [{bestSeedLow} - {bestSeedHigh}] -> Error = {minError:f1}");
                    LogMan.Warning(sC, sM, $"Low increment [{iL} - {bestSeedHigh}] -> Error = {errorSeedLow:f1}");
                    LogMan.Warning(sC, sM, $"High decrement[{bestSeedLow} - {iH}] -> Error = {errorSeedHigh:f1} ");
                    if (errorSeedLow <= errorSeedHigh)
                    {
                        bestSeedLow = iL;
                        if (minError >= errorSeedLow)
                        {
                            minError = errorSeedLow;
                            ZxLowColorInSeed = iL;
                            processingResult = resultSeedLow;
                        }
                    }
                    else
                    {
                        bestSeedHigh = iH;
                        if (minError >= errorSeedHigh)
                        {
                            minError = errorSeedHigh;
                            ZxHighColorInSeed = iH;
                            processingResult = resultSeedHigh;
                        }
                    }
                    currentMinError = Math.Min(errorSeedLow, errorSeedHigh);
                    if (iH <= iL)
                    {
                        LogMan.Debug(sC, sM, $"Exit tuning cycle - high < low");
                        bExit = true;
                    }

                    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                    {
                        ColorTransformInterface = this,
                        CompletedPercent = ((iStep * 8.0) / 256.0) * 100.0,
                        ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, processingResult?.MergeImage, $"step {iStep}", minError),
                    });
                }

                if (processingResult?.MergeImage != null)
                {
                    return ColorTransformResults.CreateValidResult(SourceData, processingResult?.MergeImage);// oFinalData);
                }
                else
                {
                    return ColorTransformResults.CreateErrorResult("Image process creation error");
                }
            }
        }

    }
}