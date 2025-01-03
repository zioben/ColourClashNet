using ColourClashLib.Color;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionAmiga;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionCPC;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionCPC : ColorTransformReductionPalette
    {

        public enum CPCVideoMode
        {
            Mode0,
            Mode1,
            Mode2,
            Mode3,
        }

        public CPCVideoMode VideoMode { get; set; } =  CPCVideoMode.Mode0;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.CPCVideoMode:
                    if (Enum.TryParse<CPCVideoMode>(oValue?.ToString(), out var evm))
                    {
                        VideoMode = evm;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


        public ColorTransformReductionCPC()
        {
            Name = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to Amstrad CPC palette";
        }
        protected override void CreateTrasformationMap()
        {
            Palette = new ColorPalette(); 
            Palette.Add(0x00_00_00_00);
            Palette.Add(0x00_00_00_80);
            Palette.Add(0x00_00_00_FF);

            Palette.Add(0x00_80_00_00);
            Palette.Add(0x00_80_00_80);
            Palette.Add(0x00_80_00_FF);

            Palette.Add(0x00_FF_00_00);
            Palette.Add(0x00_FF_00_80);
            Palette.Add(0x00_FF_00_FF);

            Palette.Add(0x00_00_80_00);
            Palette.Add(0x00_00_80_80);
            Palette.Add(0x00_00_80_FF);

            Palette.Add(0x00_80_80_00);
            Palette.Add(0x00_80_80_80);
            Palette.Add(0x00_80_80_FF);

            Palette.Add(0x00_FF_80_00);
            Palette.Add(0x00_FF_80_80);
            Palette.Add(0x00_FF_80_FF);

            Palette.Add(0x00_00_FF_00);
            Palette.Add(0x00_00_FF_80);
            Palette.Add(0x00_00_FF_FF);

            Palette.Add(0x00_80_FF_00);
            Palette.Add(0x00_80_FF_80);
            Palette.Add(0x00_80_FF_FF);

            Palette.Add(0x00_FF_FF_00);
            Palette.Add(0x00_FF_FF_80);
            Palette.Add(0x00_FF_FF_FF);
        }

        int[,]? PreProcess(int[,]? oDataSource, bool bHalveRes, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;
            var oTmp = oDataSource;
            if (bHalveRes)
            {
                oTmp = HalveHorizontalRes(oDataSource);
            }
            var oTmpData = base.ExecuteTransform(oTmp,oToken);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oTmp, oTmpData, Palette, ColorDistanceEvaluationMode, oToken);
            }
            BypassDithering = true;
            Histogram.Create(oTmpData);
            Histogram.SortColorsDescending();
            return oTmpData;
        }


        int[,]? ToMode0(int[,]? oDataSource, CancellationToken oToken)
        {
            var oTmpH = HalveHorizontalRes(oDataSource);
            var oTmp = PreProcess(oDataSource, true, oToken);
            var oPalette = Histogram.ToColorPalette().rgbPalette.Take(16).ToList();
            base.Palette = ColorPalette.FromList(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp, oToken);
            if (Dithering != null)
            {
                oTmp2 = Dithering.Dither(oTmpH, oTmp2, base.Palette, ColorDistanceEvaluationMode, oToken);
            }
            var oRet = DoubleHorizontalRes(oTmp2);
            return oRet;
        }
        int[,]? ToMode1(int[,]? oDataSource, CancellationToken oToken)
        {
            var oTmp = PreProcess(oDataSource, false, oToken);
            var oPalette = Histogram.ToColorPalette().rgbPalette.Take(4).ToList();
            base.Palette = ColorPalette.FromList(oPalette);
            var oRet = base.ExecuteTransform(oTmp, oToken);
            if (Dithering != null)
            {
                oRet = Dithering.Dither(oDataSource, oRet, base.Palette, ColorDistanceEvaluationMode, oToken);
            }
            return oRet;
        }

        int[,]? ToMode2(int[,]? oDataSource, CancellationToken oToken)
        {
            var oTmp = PreProcess(oDataSource, false, oToken);
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
            var oPalette = Histogram.ToColorPalette().rgbPalette.Take(2).ToList();
            base.Palette = ColorPalette.FromList(oPalette);
            var oRet = base.ExecuteTransform(oTmp, oToken);
            if (Dithering != null)
            {
                oRet = Dithering.Dither(oDataSource, oRet, base.Palette, ColorDistanceEvaluationMode,oToken);
            }
            return oRet;
        }


        int[,]? ToMode3(int[,]? oDataSource, CancellationToken oToken)
        {
            var oTmpH = HalveHorizontalRes(oDataSource);
            var oTmp = PreProcess(oDataSource, true, oToken);
            var oPalette = Histogram.ToColorPalette().rgbPalette.Take(4).ToList();
            base.Palette = ColorPalette.FromList(oPalette);
            var oTmp2 = base.ExecuteTransform(oTmp,oToken);
            if (Dithering != null)
            {
                oTmp2 = Dithering.Dither(oTmpH, oTmp2, base.Palette, ColorDistanceEvaluationMode, oToken);
            }
            var oRet = DoubleHorizontalRes(oTmp2);
            return oRet;
        }



        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;

            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                    {                       
                        return ToMode0(oDataSource, oToken );
                    }
                case CPCVideoMode.Mode1:
                    {
                        return ToMode1(oDataSource, oToken);
                    }
                case CPCVideoMode.Mode2:
                    {
                        return ToMode2(oDataSource, oToken);
                    }
                case CPCVideoMode.Mode3:
                    {
                        return ToMode3(oDataSource, oToken);
                    }
                default: return null;
            }

        }
    }
}