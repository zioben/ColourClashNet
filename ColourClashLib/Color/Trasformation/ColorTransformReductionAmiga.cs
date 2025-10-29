using ColourClashNet.Defaults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionAmiga : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionAmiga);


        public enum EnumAMigaVideoMode
        {
            Ham6,
            Ham8,
            ExtraHalfBright
        }

        public enum EnumHamFirstColorReductionMode
        {
            Fast,
            Detailed
        }

        public EnumAMigaVideoMode AmigaVideoMode { get; set; } = EnumAMigaVideoMode.Ham6;

        public EnumHamFirstColorReductionMode HamColorReductionMode { get; set; } = EnumHamFirstColorReductionMode.Fast;

        public ColorTransformReductionAmiga()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Commododre Amiga specific color reduction";
        }


        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.Amiga_VideoMode:
                    if (Enum.TryParse<EnumAMigaVideoMode>(oValue?.ToString(), out var evm ))
                    {
                        AmigaVideoMode = evm;
                    }
                    break;
                case ColorTransformProperties.UseColorMean:
                    if (Enum.TryParse<EnumHamFirstColorReductionMode>(oValue?.ToString(), out var cm))
                    {
                        HamColorReductionMode = cm;
                    }
                    break;
                default:
                    break;
            }
            return this;
        }

        // Not Needed
        // protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)


        async Task<int[,]?> ToHamAsync(int[,]? oDataSource, int[,]? oDataPreProcessed, ColorQuantizationMode eQuantization, CancellationToken? oToken)
        {
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);

            var oQuantization = new ColorTransformQuantization();
            oQuantization.QuantizationMode = eQuantization;
            await oQuantization.CreateAsync(oDataPreProcessed, null);

            var oRet = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                int iRgbPrev = ColorDefaults.DefaultInvalidColorInt; ;
                var oPalette = new Palette();
                for (int c = 1; c < C; c++)
                {
                    if (iRgbPrev < 0)
                    {
                        iRgbPrev = oDataPreProcessed[r, c];
                        oRet[r, c] = iRgbPrev;
                        continue;
                    }
                    else
                    {
                        int iRgbSrc = oQuantization.QuantizeColor( oDataSource[r, c] );
                        if (iRgbSrc < 0)
                        {
                            iRgbPrev = ColorDefaults.DefaultInvalidColorInt;
                            oRet[r, c] = iRgbSrc;
                            continue;
                        }
                        int pR = iRgbPrev.ToR();
                        int pG = iRgbPrev.ToG();
                        int pB = iRgbPrev.ToB();
                        int sR = iRgbSrc.ToR();
                        int sG = iRgbSrc.ToG();
                        int sB = iRgbSrc.ToB();
                        int iHamO = oQuantization.QuantizeColor( oDataPreProcessed[r,c] );
                        int iHamR = ColorIntExt.FromRGB(sR, pG, pB);
                        int iHamG = ColorIntExt.FromRGB(pR, sG, pB);
                        int iHamB = ColorIntExt.FromRGB(pR, pG, sB);
                        oPalette.Reset();
                        oPalette.Add(iHamO);
                        oPalette.Add(iHamR);
                        oPalette.Add(iHamG);
                        oPalette.Add(iHamB);
                        var oCol = oQuantization.QuantizeColor( ColorIntExt.GetNearestColor(iRgbSrc, oPalette, ColorDistanceEvaluationMode) );
                        iRgbPrev = oCol;
                        oRet[r,c] = oCol;   
                    }
                }
            }
            return oRet;
        }

        //async Task<int[,]?> ToEhb(int[,]? oDataSource, int[,]? oDataPreProcessed, CancellationToken? oToken)
        //{
        //    return await Task.Run(() => { return new int[,]; } );
        //}

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            string sM = nameof(ExecuteTransformAsync);

            ColorTransformInterface oColorReduction;
            var oQuantization = new ColorTransformQuantization()
                .SetProperty(ColorTransformProperties.Fixed_Palette, FixedPalette)
                .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType);

            int iMaxColors = 0;

            switch (AmigaVideoMode)
            {
                case EnumAMigaVideoMode.ExtraHalfBright:
                    iMaxColors = 64;
                    oQuantization.SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode.RGB444);
                    break;
                case EnumAMigaVideoMode.Ham6:
                    iMaxColors = 16;
                    oQuantization.SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode.RGB444);
                    break;
                case EnumAMigaVideoMode.Ham8:
                    iMaxColors = 64;
                    oQuantization.SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode.RGB666);
                    break;
                default:
                    return null;
            }

            await oQuantization.CreateAsync(SourceData, oToken);
            var oResultQuantized = await oQuantization.ProcessColorsAsync(oToken);
            var oDataQuantized = oResultQuantized.DataOut;

            switch (HamColorReductionMode)
            {
                default:
                case  EnumHamFirstColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionMedianCut()
                            .SetProperty(ColorTransformProperties.UseColorMean, true)
                            .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                            .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                            .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType);
                    }
                    break;
                case EnumHamFirstColorReductionMode.Detailed:
                    {
                        oColorReduction = new ColorTransformReductionCluster()
                            .SetProperty(ColorTransformProperties.UseColorMean, true)
                            .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                            .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                            .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType)
                            .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10);
                    }
                    break;
            }

            await oColorReduction.CreateAsync(SourceData,oToken);
            var oDataPreprocessedResult = await oColorReduction.ProcessColorsAsync(oToken);
            var oDataPreprocessed = oDataPreprocessedResult.DataOut;
            BypassDithering = true;

            int[,]? oRet = null; 
            switch (AmigaVideoMode)
            {
                case EnumAMigaVideoMode.Ham6:
                    oRet = await ToHamAsync(SourceData, oDataPreprocessed,  ColorQuantizationMode.RGB444, oToken );
                    break;
                case EnumAMigaVideoMode.Ham8:
                    oRet = await ToHamAsync(SourceData, oDataPreprocessed, ColorQuantizationMode.RGB666, oToken );
                    break;
                case EnumAMigaVideoMode.ExtraHalfBright:
                    oRet = null;
                    //oRet = ToEhb(oDataSource, oDataPreprocessed, oToken );
                    break;
                default:
                    oRet = null;
                    break;
            }
            if (oRet != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, oRet);
            }
            return new();
        }

    }
}
