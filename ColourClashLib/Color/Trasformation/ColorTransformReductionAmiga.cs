using ColourClashNet.Defaults;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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
        static string sC = nameof(ColorTransformReductionAmiga);


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


        ImageData ToHam(ImageData oDataSource, ImageData oDataPreProcessed, ColorQuantizationMode eQuantization)
        {

            var oQuantization = new ColorTransformQuantization();
            oQuantization.QuantizationMode = eQuantization;
            oQuantization.Create(oDataPreProcessed);

            var oHamData = new int[oDataSource.Rows, oDataSource.Columns];
            for (int r = 0; r < oDataSource.Rows; r++)
            {
                int iRgbPrev = ColorDefaults.DefaultInvalidColorInt; ;
                var oPalette = new Palette();
                for (int c = 1; c < oDataSource.Columns; c++)
                {
                    if (iRgbPrev < 0)
                    {
                        iRgbPrev = oDataPreProcessed.DataX[r, c];
                        oHamData[r, c] = iRgbPrev;
                        continue;
                    }
                    else
                    {
                        int iRgbSrc = oQuantization.QuantizeColor( oDataSource.DataX[r, c] );
                        if (iRgbSrc < 0)
                        {
                            iRgbPrev = ColorDefaults.DefaultInvalidColorInt;
                            oHamData[r, c] = iRgbSrc;
                            continue;
                        }
                        int pR = iRgbPrev.ToR();
                        int pG = iRgbPrev.ToG();
                        int pB = iRgbPrev.ToB();
                        int sR = iRgbSrc.ToR();
                        int sG = iRgbSrc.ToG();
                        int sB = iRgbSrc.ToB();
                        int iHamO = oQuantization.QuantizeColor( oDataPreProcessed.DataX[r,c] );
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
                        oHamData[r,c] = oCol;   
                    }
                }
            }
            return new ImageData().Create( oHamData );
        }

        //async Task<int[,]?> ToEhb(int[,]? oDataSource, int[,]? oDataPreProcessed, CancellationToken? oToken)
        //{
        //    return await Task.Run(() => { return new int[,]; } );
        //}

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformAsync);

            ColorTransformInterface oColorReduction;
            var oQuantization = new ColorTransformQuantization()
                .SetProperty(ColorTransformProperties.Fixed_Palette, FixedPalette)
                .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType);

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

            var oResultQuantized = await oQuantization.CreateAndProcess(SourceData, token);

            switch (HamColorReductionMode)
            {
                default:
                case  EnumHamFirstColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionMedianCut()
                            .SetProperty(ColorTransformProperties.UseColorMean, true)
                            .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                            .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                            .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType);
                    }
                    break;
                case EnumHamFirstColorReductionMode.Detailed:
                    {
                        oColorReduction = new ColorTransformReductionCluster()
                            .SetProperty(ColorTransformProperties.UseColorMean, true)
                            .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                            .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                            .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
                            .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10);
                    }
                    break;
            }


            var oDataPreprocessedResult = await oColorReduction.CreateAndProcess(oResultQuantized.DataOut, token);
            BypassDithering = true;

            ImageData? oAmigaData; 
            switch (AmigaVideoMode)
            {
                case EnumAMigaVideoMode.Ham6:
                    oAmigaData = ToHam(SourceData, oDataPreprocessedResult.DataOut, ColorQuantizationMode.RGB444 );
                    break;
                case EnumAMigaVideoMode.Ham8:
                    oAmigaData = ToHam(SourceData, oDataPreprocessedResult.DataOut, ColorQuantizationMode.RGB666 );
                    break;
                case EnumAMigaVideoMode.ExtraHalfBright:
                    oAmigaData = null;
                    //oRet = ToEhb(oDataSource, oDataPreprocessed, oToken );
                    break;
                default:
                    oAmigaData = null;
                    break;
            }
            if (oAmigaData == null)
            {
                LogMan.Error(sC, sM, $"Failed to generate Amiga transformed data");
                return ColorTransformResults.CreateErrorResult(SourceData,null, $"{sC}.{sM} : Amiga transformed data is null");
            }
            return ColorTransformResults.CreateValidResult(SourceData, oAmigaData);
        }

    }
}
