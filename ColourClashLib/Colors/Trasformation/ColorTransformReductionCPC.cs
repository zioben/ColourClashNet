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
            oPalette = new ColorPalette(); 
            oPalette.Add(0x00_00_00_00);
            oPalette.Add(0x00_00_00_80);
            oPalette.Add(0x00_00_00_FF);

            oPalette.Add(0x00_80_00_00);
            oPalette.Add(0x00_80_00_80);
            oPalette.Add(0x00_80_00_FF);

            oPalette.Add(0x00_FF_00_00);
            oPalette.Add(0x00_FF_00_80);
            oPalette.Add(0x00_FF_00_FF);

            oPalette.Add(0x00_00_80_00);
            oPalette.Add(0x00_00_80_80);
            oPalette.Add(0x00_00_80_FF);

            oPalette.Add(0x00_80_80_00);
            oPalette.Add(0x00_80_80_80);
            oPalette.Add(0x00_80_80_FF);

            oPalette.Add(0x00_FF_80_00);
            oPalette.Add(0x00_FF_80_80);
            oPalette.Add(0x00_FF_80_FF);

            oPalette.Add(0x00_00_FF_00);
            oPalette.Add(0x00_00_FF_80);
            oPalette.Add(0x00_00_FF_FF);

            oPalette.Add(0x00_80_FF_00);
            oPalette.Add(0x00_80_FF_80);
            oPalette.Add(0x00_80_FF_FF);

            oPalette.Add(0x00_FF_FF_00);
            oPalette.Add(0x00_FF_FF_80);
            oPalette.Add(0x00_FF_FF_FF);
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
                oTmpData = dithering.Dither(oTmp, oTmpData, oPalette, ColorDistanceEvaluationMode);
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
            base.oPalette = ColorPalette.CreateColorPalette(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmp2 = dithering.Dither(oTmpH, oTmp2, base.oPalette, ColorDistanceEvaluationMode);
            }
            var oRet = DoubleHorizontalRes(oTmp2);
            return oRet;
        }
        int[,]? ToMode1(int[,]? oDataSource)
        {
            var oTmp = PreProcess(oDataSource, false);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(4).ToList();
            base.oPalette = ColorPalette.CreateColorPalette(oPalette);
            var oRet = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oRet = dithering.Dither(oDataSource, oRet, base.oPalette, ColorDistanceEvaluationMode);
            }
            return oRet;
        }

        int[,]? ToMode2(int[,]? oDataSource)
        {
            var oTmp = PreProcess(oDataSource, false);
            //ColorTransformReductionCluster oTrasf = new ColorTransformReductionCluster()
            //{
            //    ColorDistanceEvaluationMode = ColorDistanceEvaluationMode,
            //    ColorsMax = 16,
            //    dithering = dithering,
            //    TrainingLoop = 5,
            //    UseClusterColorMean = false
            //};
            //oTrasf.Create(oTmp, null);
            //var oRet = oTrasf.TransformAndDither(oTmp);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(2).ToList();
            base.oPalette = ColorPalette.CreateColorPalette(oPalette);
            var oRet = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oRet = dithering.Dither(oDataSource, oRet, base.oPalette, ColorDistanceEvaluationMode);
            }
            return oRet;
        }


        int[,]? ToMode3(int[,]? oDataSource)
        {
            var oTmpH = HalveHorizontalRes(oDataSource);
            var oTmp = PreProcess(oDataSource, true);
            var oPalette = colorHistogram.ToColorPalette().rgbPalette.Take(4).ToList();
            base.oPalette = ColorPalette.CreateColorPalette(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmp2 = dithering.Dither(oTmpH, oTmp2, base.oPalette, ColorDistanceEvaluationMode);
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