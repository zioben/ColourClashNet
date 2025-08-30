using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Tile.TileBase;

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

        ColorTransformQuantization oQuantization = new ColorTransformQuantization();

        public ColorTransformReductionAmiga()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Commododre Amiga specific color reduction";
        }


        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.Amiga_VideoMode:
                    if (Enum.TryParse<EnumAMigaVideoMode>(oValue?.ToString(), out var evm ))
                    {
                        AmigaVideoMode = evm;
                        return this;
                    }
                    break;
                case ColorTransformProperties.UseColorMean:
                    if (Enum.TryParse<EnumHamFirstColorReductionMode>(oValue?.ToString(), out var cm))
                    {
                        HamColorReductionMode = cm;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


      

        int[,]? ToHam(int[,]? oDataSource, int[,]? oDataPreProcessed, ColorQuantizationMode eQuantization, CancellationToken oToken)
        {
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            oQuantization.QuantizationMode = eQuantization;

            var oRet = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                int iRgbPrev = ColorDefaults.DefaultInvalidColorRGB; ;
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
                            iRgbPrev = ColorDefaults.DefaultInvalidColorRGB; ;
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

        int[,]? ToEhb(int[,]? oDataSource, int[,]? oDataPreProcessed, CancellationToken oToken)
        {
            return oDataPreProcessed;
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            if( oDataSource== null ) 
            {
                return null;
            }

            ColorTransformInterface oColorReduction;
            var oQuantization = new ColorTransformQuantization();

            int iMaxColors = 0;

            switch (AmigaVideoMode)
            {
                case EnumAMigaVideoMode.ExtraHalfBright:
                    iMaxColors = 64;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB444;
                    break;
                case EnumAMigaVideoMode.Ham6:
                    iMaxColors = 16;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB444;
                    break;
                case EnumAMigaVideoMode.Ham8:
                    iMaxColors = 64;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB666;
                    break;
                default:
                    return null;
            }

            oQuantization.Create(oDataSource, InputFixedColorPalette);
            oQuantization.Dithering = Dithering;
            var oResultQuantized = oQuantization.ProcessColors(oDataSource);
            var oDataQuantized = oResultQuantized.DataOut;

            switch (HamColorReductionMode)
            {
                default:
                case  EnumHamFirstColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionMedianCut()
                        {   
                            UseColorMean = true, 
                            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode,
                            ColorsMaxWanted = iMaxColors,
                        };
                    }
                    break;
                case EnumHamFirstColorReductionMode.Detailed:
                    {
                        oColorReduction = new ColorTransformReductionCluster
                        {
                            TrainingLoop = 10,
                            UseClusterColorMean = true,
                            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode,
                            ColorsMaxWanted = iMaxColors,
                        };
                    }
                    break;
            }

            oColorReduction.Dithering = Dithering;
            oColorReduction.Create(oDataSource, InputFixedColorPalette);
            var oResultPreprocessed = oColorReduction.ProcessColors(oDataQuantized);
            var oDataPreprocessed = oResultPreprocessed.DataOut;
            BypassDithering = true;

            int[,] oRet;
            switch (AmigaVideoMode)
            {
                case EnumAMigaVideoMode.Ham6:
                    oRet = ToHam(oDataSource, oDataPreprocessed,  ColorQuantizationMode.RGB444, oToken );
                    break;
                case EnumAMigaVideoMode.Ham8:
                    oRet = ToHam(oDataSource, oDataPreprocessed, ColorQuantizationMode.RGB666, oToken );
                    break;
                case EnumAMigaVideoMode.ExtraHalfBright:
                    oRet = ToEhb(oDataSource, oDataPreprocessed, oToken );
                    break;
                default:
                    oRet = null;
                    break;
            }

            Create(oRet, InputFixedColorPalette);
            return oRet;
        }

    }
}
