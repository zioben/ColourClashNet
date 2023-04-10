using ColourClashLib.Color;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionCPC;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionCPC : ColorTransformReductionPalette
    {

        public enum CPCScreenMode
        {
            Mode0,
            Mode1,
            Mode2,
            Mode3,
        }

        public CPCScreenMode ScreenMode { get; set; }=  CPCScreenMode.Mode0;

        public ColorTransformReductionCPC()
        {
            type = ColorTransform.ColorReductionCBM64;
            description = "Reduce color to Amstrad CPC palette";
        }
        protected override void CreateTrasformationMap()
        {
            colorPalette = new ColorPalette(); 
            colorPalette.Add(0x00000000);
            colorPalette.Add(0x00000080);
            colorPalette.Add(0x000000FF);

            colorPalette.Add(0x00800000);
            colorPalette.Add(0x00800080);
            colorPalette.Add(0x008000FF);

            colorPalette.Add(0x00FF0000);
            colorPalette.Add(0x00FF0080);
            colorPalette.Add(0x00FF00FF);

            colorPalette.Add(0x00008000);
            colorPalette.Add(0x00008080);
            colorPalette.Add(0x000080FF);

            colorPalette.Add(0x00808000);
            colorPalette.Add(0x00808080);           
            colorPalette.Add(0x008080FF);

            colorPalette.Add(0x00FF8000);
            colorPalette.Add(0x00FF8080);
            colorPalette.Add(0x00FF80FF);

            colorPalette.Add(0x0000FF00);
            colorPalette.Add(0x0000FF80);
            colorPalette.Add(0x0000FFFF);

            colorPalette.Add(0x00808000);
            colorPalette.Add(0x00808080);
            colorPalette.Add(0x008080FF);

            colorPalette.Add(0x00FFFF00);
            colorPalette.Add(0x00FFFF80);
            colorPalette.Add(0x00FFFFFF);
        }

        int[,]? PreProcess(int[,]? oDataSource, bool bHalveRes )
        {
            if (oDataSource == null)
                return null;
            var oTmp = oDataSource;
            if (bHalveRes)
            {
                oTmp = HalveHorizontalRes(oDataSource);
            }
            var oTmpData = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oTmp, oTmpData, colorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;
            colorHistogram.Create(oTmpData);
            colorHistogram.SortColorsDescending();
            return oTmpData;
        }


        int[,]? ToMode0(int[,]? oDataSource)
        {
            var oTmpH = HalveHorizontalRes(oDataSource);
            var oTmp = PreProcess(oDataSource, true);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(16).ToList();
            colorPalette = ColorPalette.CreateColorPalette(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmp2 = dithering.Dither(oTmpH, oTmp2, colorPalette, ColorDistanceEvaluationMode);
            }
            var oRet = DoubleHorizontalRes(oTmp2);
            return oRet;
        }
        int[,]? ToMode1(int[,]? oDataSource)
        {
            var oTmp = PreProcess(oDataSource, false);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(4).ToList();
            colorPalette = ColorPalette.CreateColorPalette(oPalette);
            var oRet = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oRet = dithering.Dither(oDataSource, oRet, colorPalette, ColorDistanceEvaluationMode);
            }
            return oRet;
        }

        int[,]? ToMode2(int[,]? oDataSource)
        {
            var oTmp = PreProcess(oDataSource, false);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(2).ToList();
            colorPalette = ColorPalette.CreateColorPalette(oPalette);
            var oRet = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oRet = dithering.Dither(oDataSource, oRet, colorPalette, ColorDistanceEvaluationMode);
            }
            return oRet;
        }


        int[,]? ToMode3(int[,]? oDataSource)
        {
            var oTmpH = HalveHorizontalRes(oDataSource);
            var oTmp = PreProcess(oDataSource, true);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(4).ToList();
            colorPalette = ColorPalette.CreateColorPalette(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmp2 = dithering.Dither(oTmpH, oTmp2, colorPalette, ColorDistanceEvaluationMode);
            }
            var oRet = DoubleHorizontalRes(oTmp2);
            return oRet;
        }



        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            switch (ScreenMode)
            {
                case CPCScreenMode.Mode0:
                    {                       
                        return ToMode0(oDataSource);
                    }
                case CPCScreenMode.Mode1:
                    {
                        return ToMode1(oDataSource);
                    }
                case CPCScreenMode.Mode2:
                    {
                        return ToMode2(oDataSource);
                    }
                case CPCScreenMode.Mode3:
                    {
                        return ToMode3(oDataSource);
                    }
                default: return null;
            }

        }
    }
}