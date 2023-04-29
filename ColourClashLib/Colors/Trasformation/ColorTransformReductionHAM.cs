using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashLib.Colors.Tile.TileBase;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionAmiga : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionAmiga);


        public enum EnumVideoMode
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

        public EnumVideoMode VideoMode { get; set; } = EnumVideoMode.Ham6;

        public EnumHamFirstColorReductionMode HamColorReductionMode { get; set; } = EnumHamFirstColorReductionMode.Fast;

        ColorTransformQuantization oQuantization = new ColorTransformQuantization();

        public ColorTransformReductionAmiga()
        {
            type = ColorTransform.ColorReductionClustering;
            description = "Commododre Amiga specific color reduction";
        }

        protected override void CreateTrasformationMap()
        {
        }

        int[,]? ToHam(int[,]? oDataSource, int[,]? oDataPreProcessed, ColorQuantizationMode eQuantization )
        {
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            oQuantization.QuantizationMode = eQuantization;

            var oRet = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                int iRgbPrev = -1;
                var oPalette = new ColorPalette();
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
                            iRgbPrev = -1;
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

        int[,]? ToEhb(int[,]? oDataSource, int[,]? oDataPreProcessed)
        {
            return oDataPreProcessed;
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if( oDataSource== null ) 
            {
                return null;
            }

            ColorTransformInterface oColorReduction;
            var oQuantization = new ColorTransformQuantization();

            int iMaxColors = 0;

            switch (VideoMode)
            {
                case EnumVideoMode.ExtraHalfBright:
                    iMaxColors = 64;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB444;
                    break;
                case EnumVideoMode.Ham6:
                    iMaxColors = 16;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB444;
                    break;
                case EnumVideoMode.Ham8:
                    iMaxColors = 64;
                    oQuantization.QuantizationMode = ColorQuantizationMode.RGB666;
                    break;
                default:
                    return null;
            }

            oQuantization.Create(oDataSource, FixedColorPalette);
            oQuantization.dithering = dithering;
            var oDataQuantized = oQuantization.TransformAndDither(oDataSource);
           
            switch (HamColorReductionMode)
            {
                default:
                case  EnumHamFirstColorReductionMode.Fast:
                    {
                        oColorReduction = new ColorTransformReductionMedianCut()
                        {   
                            UseClusterColorMean = true, 
                            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode,
                            ColorsMax = iMaxColors,
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
                            ColorsMax = iMaxColors,
                        };
                    }
                    break;
            }

            oColorReduction.dithering = dithering;
            oColorReduction.Create(oDataSource, FixedColorPalette);
            var oDataPreprocessed = oColorReduction.TransformAndDither(oDataQuantized);
            BypassDithering = true;

            int[,] oRet;
            switch (VideoMode)
            {
                case EnumVideoMode.Ham6:
                    oRet = ToHam(oDataSource, oDataPreprocessed,  ColorQuantizationMode.RGB444 );
                    break;
                case EnumVideoMode.Ham8:
                    oRet = ToHam(oDataSource, oDataPreprocessed, ColorQuantizationMode.RGB666 );
                    break;
                case EnumVideoMode.ExtraHalfBright:
                    oRet = ToEhb(oDataSource, oDataPreprocessed);
                    break;
                default:
                    oRet = null;
                    break;
            }

            Create(oRet, FixedColorPalette);
            return oRet;
        }

    }
}
