using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Log;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionZxSpectrum : ColorTransformReductionPalette
    {
        static string sClass = nameof(ColorTransformReductionZxSpectrum);

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


        public int ColLSeed { get; set; } = 0x0080;
        public int ColHSeed { get; set; } = 0x00FF;
        public ZxPaletteMode PaletteMode { get; set; } = ZxPaletteMode.Both;
        public bool IncludeBlackInHighColor { get; set; } = true;
        public bool AutoTune { get; set; } = true;  
        public bool DitherHighColor { get; set; } = true;

        int iColOutL = 0x00D8;
        int iColOutH = 0x00FF;

        public override ColorTransformInterface SetProperty( ColorTransformProperties eProperty, object oValue )
        {
            base.SetProperty(eProperty, oValue);

            switch ( eProperty) 
            {
                case ColorTransformProperties.Zx_ColL_Seed:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        ColLSeed= l;
                    }
                    break;
                case ColorTransformProperties.Zx_ColH_Seed:
                    if (int.TryParse(oValue.ToString(), out var h))
                    {
                        ColHSeed = h;
                    }
                    break;
                case ColorTransformProperties.Zx_DitherHighColor:
                    if (bool.TryParse(oValue?.ToString(), out var d))
                    {
                        DitherHighColor = d;    
                    }
                    break;
                case ColorTransformProperties.Zx_IncludeBlackInHighColor:
                    if (bool.TryParse(oValue?.ToString(), out var b))
                    {
                        IncludeBlackInHighColor = b;
                    }
                    break;
                case ColorTransformProperties.Zx_PaletteMode:
                    {
                        if (Enum.TryParse<ZxPaletteMode>(oValue?.ToString(), out var eMode))
                        {
                            PaletteMode = eMode;
                        }
                    }
                    break;
                case ColorTransformProperties.Zx_Autotune:
                    {
                        if (bool.TryParse(oValue?.ToString(), out var bAutotune))
                        {
                            AutoTune = bAutotune;
                        }
                    }
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

        async Task<TileManager> CreateAndProcessTilesAsync(Palette oPalette, bool bUseDithering, CancellationToken? oToken)
        {
            var oPreProcessing = new ColorTransformReductionPalette()
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, oPalette)
                .SetProperty(ColorTransformProperties.Dithering_Type, bUseDithering ? DitheringType : ColorDithering.None)
                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);
            await oPreProcessing.CreateAsync(SourceData, oToken);
            var oPreRes = await oPreProcessing.ProcessColorsAsync(oToken);
            var oPreData = oPreRes.DataOut;
            TileManager oTileManager = TileManager.Create(8, 8, 2)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, oPalette)
                .SetProperty(ColorTransformProperties.Forced_Palette, oPalette)
                .SetProperty(ColorTransformProperties.Dithering_Type, bUseDithering ? DitheringType : ColorDithering.None)
                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);
            var retC = await oTileManager.CreateAsync(oPreData, oToken);
            var retP = await oTileManager.ProcessColorsAsync(oToken);
            this.TransformationError = await oTileManager.EvaluateImageErrorAsync(SourceData,oToken);
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
                        ret[r,c] = tempImage1[r,c];
                    }
                }
                if (r < R2)
                {
                    for (int c = 0; c < C2; c++)
                    {
                        ret[r, C1+c] = tempImage2[r, c];
                    }
                }
            }
            return ret;
        }

        protected async Task<Tuple<int[,]?, double, TileManager, TileManager>>? ExecuteTransformZxAsync(int iColLSeed, int iColHSeed, CancellationToken? oToken)
        {
            var iil = iColLSeed;
            var iih = iColHSeed;
            int iol = iColOutL;
            int ioh = iColOutH;

            var zxMapLo = CreateZxMap(iil, iColLSeed, true);
            var zxMapHi = CreateZxMap(iih, iColHSeed, IncludeBlackInHighColor);
            var zxMap = new ColorTransformationMap();
            List<Task<TileManager>> lTaskList = new();

            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteHi:
                    {
                        zxMapHi = CreateZxMap(iih, iColHSeed, true);
                        zxMap = zxMapHi;
                        lTaskList.Add(CreateAndProcessTilesAsync(zxMapHi.GetInputPalette(), true, oToken));
                    }
                    break;
                case ZxPaletteMode.PaletteLo:
                    {
                        zxMap = zxMapLo;
                        lTaskList.Add(CreateAndProcessTilesAsync(zxMapLo.GetInputPalette(), DitherHighColor, oToken));
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
                        lTaskList.Add(CreateAndProcessTilesAsync(zxMapLo.GetInputPalette(), true, oToken));
                        lTaskList.Add(CreateAndProcessTilesAsync(zxMapHi.GetInputPalette(), DitherHighColor, oToken));
                    }
                    break;
                default:
                    return null;
            }

            await Task.WhenAll(lTaskList);
            var lTM = lTaskList.Select(X => X.Result).ToList();
            var oResultData = await TileManager.MergeDataAsync(SourceData, lTM, oToken);
            var oRestltDataMapped = await zxMap.TransformAsync(oResultData, oToken);
            var dError = await ColorIntExt.EvaluateErrorAsync( SourceData, oResultData, ColorDistanceEvaluationMode, oToken);
            if (lTM.Count == 1)
            {
                return new Tuple<int[,]?, double, TileManager, TileManager>(oResultData, dError, lTaskList[0].Result, null);
            }
            else
            {
                return new Tuple<int[,]?, double, TileManager, TileManager>(oResultData, dError, lTaskList[0].Result, lTaskList[1].Result);
            }
        }

        object locker = new object();

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            string sMethod = nameof(ExecuteTransformAsync);

            BypassDithering = true;

            if (!AutoTune)
            {
                var oTuple = await ExecuteTransformZxAsync(ColLSeed, ColHSeed, oToken);
                return ColorTransformResults.CreateValidResult(SourceData, oTuple.Item1);
            }

            // Processing Color Range - [LBest, HBest]
            int LBest = ColLSeed;
            int HBest = ColHSeed;
            var oBest = await ExecuteTransformZxAsync(LBest, HBest, oToken);
            var dMinError = oBest.Item2;
            var dError = double.PositiveInfinity;
            int iL = 0;
            int iH = 5000;
            int iStep = 0;
            bool bExit = false;
            int[,] tempImage = MergeTempImage(oBest.Item1, oBest.Item1);
            RaiseProcessPartialEvent(new ColorTransformEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 0,
                TempImage = tempImage,
                Message = "First Tuning",
            });
            while (!bExit)// (iL + 1) <= iH && dError <= dMinError)
            {
                iL = LBest + 8;
                iH = HBest - 8;
                List<Task<Tuple<int[,]?, double, TileManager, TileManager>>> tasklist = new();
                tasklist.Add(Task.Run(async () => await ExecuteTransformZxAsync(iL, HBest, oToken)));
                tasklist.Add(Task.Run(async () => await ExecuteTransformZxAsync(LBest, iH, oToken)));
                await Task.WhenAll(tasklist);
                var oBaseL = tasklist[0].Result;// await ExecuteTransformZxAsync(iL, HBest, oToken);
                var oBaseH = tasklist[1].Result;// await ExecuteTransformZxAsync(LBest, iH, oToken);

                tempImage = MergeTempImage(oBaseL.Item1, oBaseH.Item1);
                RaiseProcessPartialEvent(new ColorTransformEventArgs()
                {
                    ColorTransformInterface = this,
                    CompletedPercent = 100.0 * (++iStep) * 8.0 / 256.0,
                    Message = $"Step {iStep} : Autotuning between [{iL},{HBest}] - [{LBest},{iH}]",
                    TempImage = tempImage,
                    //ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, tasklist[0].Result.Item1)
                });
                var dErrL = oBaseL.Item2;
                var dErrH = oBaseH.Item2;
                if (dErrL < dErrH)
                {
                    LBest = iL;
                    if (dErrL < dMinError)
                    {
                        ColLSeed = iL;
                        oBest = oBaseL;
                    }
                }
                else
                {
                    HBest = iH;
                    if (dErrH < dMinError)
                    {
                        ColHSeed = iH;
                        oBest = oBaseH;
                    }
                }
                dError = Math.Min(dErrL, dErrH);
                LogMan.Message(sClass, sMethod, $"Working with : LO = {iL:D3} and HI = {iH:D3} : Error = {dError:f2} --- Best Error = {oBest.Item2:f2}");
                if (iH < iL)
                    bExit = true;
                if (dError > dMinError)
                    bExit = true;
                else
                {
                    dMinError = dError;
                }
            }

            TransformationMap.Reset();
            BypassDithering = true;
            var oFinalData = oBest.Item1;
            if (oFinalData != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, oFinalData);
            }
            else
            {
                return ColorTransformResults.CreateErrorResult("Error creating final data");
            }
        }



    }
}