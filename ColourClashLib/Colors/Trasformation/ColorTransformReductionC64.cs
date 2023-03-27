using ColourClashLib.Color;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformReductionPalette
    {

        static int iArea = 8;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public enum C64ScreenMode
        {
            HiRes,
            HiResEnhancedPalette,
            Multicolor,
            MulticolorCaroline,
        }

        public C64ScreenMode ScreenMode { get; set; }= C64ScreenMode.HiResEnhancedPalette;

        public ColorTransformReductionC64()
        {
            type = ColorTransform.ColorReductionCBM64;
            description = "Reduce color to C64 palette";
        }
        protected override void CreateTrasformationMap()
        {
            colorPalette = new ColorPalette(); 
            colorPalette.Add(0x00000000);
            colorPalette.Add(0x00FFFFFF);
            colorPalette.Add(0x00894036);
            colorPalette.Add(0x007ABFC7);
            colorPalette.Add(0x008A46AE);
            colorPalette.Add(0x0068A941);
            colorPalette.Add(0x003E31A2);
            colorPalette.Add(0x00D0DC71);
            colorPalette.Add(0x00905F25);
            colorPalette.Add(0x005C4700);
            colorPalette.Add(0x00BB776D);
            colorPalette.Add(0x00555555);
            colorPalette.Add(0x00808080);
            colorPalette.Add(0x00ACEA88);
            colorPalette.Add(0x00ABABAB);       
        }

        int[,]? TohiRes(int[,]? oTmpDataSource)
        {
            TileManager oManager = new TileManager();
            oManager.Create(oTmpDataSource, 8, 8, 2, colorPalette.ToList(), TileBase.EnumColorReductionMode.Detailed);
            var oRet = oManager.TransformAndDither(oTmpDataSource);
            return oRet;
        }


        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oDataSource, oTmpData, colorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;

            switch (ScreenMode)
            {
                case C64ScreenMode.HiRes:
                    return TohiRes(oTmpData);
                case C64ScreenMode.HiResEnhancedPalette:
                    return TohiRes(oTmpData);
                default: return oTmpData;
            }
        }
    }
}