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
    public class ColorTransformReductionZxSpectrumV2 : ColorTransformReductionPalette
    {
        static string sClass = nameof(ColorTransformReductionZxSpectrumV2);

        public enum ZxPaletteMode
        {
            PaletteLo = 0,
            PaletteHi = 1,
            Both = 2,
        }

        public ColorTransformReductionZxSpectrumV2()
        {
            Type = ColorTransformType.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        ColorTransformType processingType { get; } = ColorTransformType.ColorReductionGenericPalette;
        Dictionary<ColorTransformProperties, object> CreateProcessingParams( Palette palette, ColorDithering ditheringType )
        {
            var dict = new Dictionary<ColorTransformProperties, object>();
            dict[ColorTransformProperties.ColorDistanceEvaluationMode] = ColorDistanceEvaluationMode;
            dict[ColorTransformProperties.Fixed_Palette] = palette;
            dict[ColorTransformProperties.Forced_Palette] = palette;
            dict[ColorTransformProperties.Dithering_Type] = ditheringType;
            dict[ColorTransformProperties.Dithering_Strength] = DitheringStrength;
            dict[ColorTransformProperties.MaxColorsWanted] = 2;
            return dict;
        }


        public int ZxLowColorInSeed { get; set; } = 0x0080;
        public int ZxHighColorInSeed { get; set; } = 0x00FF;
        public ZxPaletteMode PaletteMode { get; set; } = ZxPaletteMode.Both;
        public bool IncludeBlackInHighColor { get; set; } = true;
        public bool AutoTune { get; set; } = true;
        public bool DitherLowColorImage { get; set; } = true;
        public bool DitherHighColorImage { get; set; } = true;
        public int ZxLowColorOutSeed { get; init; } = 0x00D8;
        public int ZxHighColorOutSeed { get; init; } = 0x00FF;

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

      

        ColorTransformationMap CreateZxMap(int iColIn, int iColOut, bool bUseBlack)
        {
            ColorTransformationMap oMap = new ColorTransformationMap();
            if (bUseBlack)
            {
                oMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            }
            oMap.Add(ColorIntExt.FromRGB(0, 0, iColIn), ColorIntExt.FromRGB(0, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColIn, 0, 0), ColorIntExt.FromRGB(iColOut, 0, 0));
            oMap.Add(ColorIntExt.FromRGB(iColIn, 0, iColIn), ColorIntExt.FromRGB(iColOut, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(0, iColIn, 0), ColorIntExt.FromRGB(0, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(0, iColIn, iColIn), ColorIntExt.FromRGB(0, iColOut, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColIn, iColIn, 0), ColorIntExt.FromRGB(iColOut, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(iColIn, iColIn, iColIn), ColorIntExt.FromRGB(iColOut, iColOut, iColOut));
            return oMap;
        }

        TileManager CreateAndProcessTiles(Palette oPalette, bool bUseDithering, CancellationToken token = default)
        {
            var dithering = bUseDithering ? DitheringType : ColorDithering.None;

            var oPreProcessing = new ColorTransformReductionPalette()
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, oPalette)
                .SetProperty(ColorTransformProperties.Dithering_Type, dithering )
                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrength)
                .Create(SourceData);
            var oPreRes = oPreProcessing.ProcessColors(token);
            var oPreData = oPreRes.DataOut;

            TileManager oTileManager = TileManager.CreateTileManager(8, 8, oPreData, processingType, CreateProcessingParams(oPalette, dithering), token);
            var tileProcRes = oTileManager.ProcessColors(token);
            oTileManager.RecalcGlobalTransformationError(SourceData, token);
            return oTileManager;
        }

        int[,] MergeTempImage(int[,] tempImage1, int[,] tempImage2)
        {
            if (tempImage1 == null || tempImage2 == null)
                return null;
            int R1 = tempImage1.GetLength(0);
            int C1 = tempImage1.GetLength(1);
            int R2 = tempImage2.GetLength(0);
            int C2 = tempImage2.GetLength(1);
            int R = Math.Max(R1, R2);
            int C = C1 + C2;
            var ret = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                if (r < R1)
                {
                    for (int c = 0; c < C1; c++)
                    {
                        ret[r, c] = tempImage1[r, c];
                    }
                }
                if (r < R2)
                {
                    for (int c = 0; c < C2; c++)
                    {
                        ret[r, C1 + c] = tempImage2[r, c];
                    }
                }
            }
            return ret;
        }

        protected async Task<Tuple<int[,]?, double, TileManager, TileManager>>? ExecuteTransformZxAsync(int iColLSeed, int iColHSeed, CancellationToken oToken)
        {
            var zxMapLo = CreateZxMap(iColLSeed, ZxLowColorOutSeed, true);
            var zxMapHi = CreateZxMap(iColHSeed, ZxHighColorOutSeed, IncludeBlackInHighColor);
            var zxMap = new ColorTransformationMap();
            List<Task<TileManager>> lTaskList = new();

            // Create the task job
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteHi:
                    {
                        //zxMapHi = CreateZxMap(iColHSeed, iColHSeed, true);
                        zxMap = zxMapHi;
                        // lTaskList.Add(CreateAndProcessTiles(zxMapHi.GetInputPalette(), DitherLowColorImage, oToken));
                    }
                    break;
                case ZxPaletteMode.PaletteLo:
                    {
                        zxMap = zxMapLo;
                        // lTaskList.Add(CreateAndProcessTiles(zxMapLo.GetInputPalette(), DitherHighColorImage, oToken));
                    }
                    break;
                case ZxPaletteMode.Both:
                    {
                        foreach (var rgbLO in zxMapLo.rgbTransformationMap)
                        {
                            zxMap.Add(rgbLO.Key, rgbLO.Value);
                        }
                        foreach (var rgbHI in zxMapHi.rgbTransformationMap)
                        {
                            zxMap.Add(rgbHI.Key, rgbHI.Value);
                        }
                        //lTaskList.Add(CreateAndProcessTiles(zxMapLo.GetInputPalette(), DitherLowColorImage, oToken));
                        // lTaskList.Add(CreateAndProcessTiles(zxMapHi.GetInputPalette(), DitherHighColorImage, oToken));
                    }
                    break;
                default:
                    return null;
            }

            // Await task completed
            await Task.WhenAll(lTaskList);
            var lTM = lTaskList.Select(X => X.Result).ToList();

            // Merge data with best tile approximation
            ImageData oResultData = null;// await TileManager.MergeDataAsync(SourceData, lTM, oToken);
            // Evaluate error on processed image
            var dError = ColorIntExt.EvaluateError(SourceData, oResultData, ColorDistanceEvaluationMode, oToken);

            var oRestltDataMapped = zxMap.Transform(oResultData, oToken);
            if (lTM.Count == 1)
            {
                return new Tuple<int[,]?, double, TileManager, TileManager>(oRestltDataMapped.matrix, dError, lTaskList[0].Result, null);
            }
            else
            {
                return new Tuple<int[,]?, double, TileManager, TileManager>(oRestltDataMapped.matrix, dError, lTaskList[0].Result, lTaskList[1].Result);
            }
        }



        object locker = new object();

        protected override ColorTransformResults ExecuteTransform(CancellationToken oToken)
        {
            string sMethod = nameof(ExecuteTransform);

            BypassDithering = true;

            return ColorTransformResults.CreateErrorResult("Autotuning not implemented in V2");
            //if (!AutoTune)
            //{
            //    var oTuple = ExecuteTransformZxAsync(ZxLowColorInSeed, ZxHighColorInSeed, oToken);
            //    TransformationError = oTuple.Item2;
            //    return ColorTransformResults.CreateValidResult(SourceData, null);// oTuple.Item1);
            //}

            //// Processing Color Range - [LBest, HBest]
            //int LBest = ZxLowColorInSeed;
            //int HBest = ZxHighColorInSeed;
            //var oBest = ExecuteTransformZxAsync(LBest, HBest, oToken);
            //var dMinError = oBest.Item2;
            //var dError = double.PositiveInfinity;
            //int iStep = 0;
            //bool bExit = false;
            //int[,] tempImage = MergeTempImage(oBest.Item1, oBest.Item1);
            //RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            //{
            //    ColorTransformInterface = this,
            //    CompletedPercent = 0,
            //   // TempImage = tempImage,
            //   // Message = "First Tuning",
            //});
            //while (!bExit)// (iL + 1) <= iH && dError <= dMinError)
            //{
            //    var iL = LBest + 8;
            //    var iH = HBest - 8;
            //    List<Task<Tuple<int[,]?, double, TileManager, TileManager>>> tasklist = new();
            //    // Evaluate increasing low values; 
            //    tasklist.Add(Task.Run(() => ExecuteTransformZxAsync(iL, HBest, oToken)));
            //    // Evaluate decreasing high values
            //    tasklist.Add(Task.Run(()=> ExecuteTransformZxAsync(LBest, iH, oToken)));
            //    Task.WhenAll(tasklist);
            //    var oResultL = tasklist[0].Result;
            //    var oResultH = tasklist[1].Result;

            //    // Create Temp image with both preprocess images
            //    tempImage = MergeTempImage(oResultL.Item1, oResultH.Item1);
            //    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            //    {
            //        ColorTransformInterface = this,
            //        CompletedPercent = 100.0 * (++iStep) * 8.0 / 256.0,
            //        //Message = $"Step {iStep} : Autotuning between [{iL},{HBest}] - [{LBest},{iH}]",
            //        //TempImage = tempImage,
            //        //ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, tasklist[0].Result.Item1)
            //    });
            //    var dErrL = oResultL.Item2;
            //    var dErrH = oResultH.Item2;
            //    LogMan.Warning(sClass, sMethod, $"------------------------------------");
            //    LogMan.Warning(sClass, sMethod, $"Ref   error = {dMinError:f1} [{LBest} - {HBest}]");
            //    LogMan.Warning(sClass, sMethod, $"L-Err error = {dErrL:f1} [{iL} - {HBest}]");
            //    LogMan.Warning(sClass, sMethod, $"H-Err error = {dErrH:f1} [{LBest} - {iH}]");
            //    if (dErrL < dErrH)
            //    {
            //        HBest = iH;
            //        if (dErrL < dMinError)
            //        {
            //            ZxHighColorInSeed = iH;
            //            oBest = oResultH;
            //        }
            //    }
            //    else
            //    {
            //        LBest = iL;
            //        if (dErrH < dMinError)
            //        {
            //            ZxLowColorInSeed = iL;
            //            oBest = oResultL;
            //        }
            //    }
            //    dError = Math.Min(dErrL, dErrH);
            //    if (iH < iL)
            //        bExit = true;
            //    if (dError > dMinError)
            //        bExit = true;
            //    else
            //    {
            //        dMinError = dError;
            //    }
            // }

            //TransformationMap.Reset();
            //BypassDithering = true;
            //var oFinalData = oBest.Item1;
            //if (oFinalData != null)
            //{
            //    return ColorTransformResults.CreateValidResult(SourceData, null);// oFinalData);
            //}
            //else
            //{
            //    return ColorTransformResults.CreateErrorResult("Error creating final data");
            //}
        }



    }
}