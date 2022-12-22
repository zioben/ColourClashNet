using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformToPalette
    {

        static int iArea = 8;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public ColorTransformReductionC64()
        {
            Type = ColorTransform.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
        }
        protected override void CreateTrasformationMap()
        {
            ColorPalette = new ColorPalette(); 
            ColorPalette.Add(0x00000000);
            ColorPalette.Add(0x00FFFFFF);
            ColorPalette.Add(0x00894036);
            ColorPalette.Add(0x007ABFC7);
            ColorPalette.Add(0x008A46AE);
            ColorPalette.Add(0x0068A941);
            ColorPalette.Add(0x003E31A2);
            ColorPalette.Add(0x00D0DC71);
            ColorPalette.Add(0x00905F25);
            ColorPalette.Add(0x005C4700);
            ColorPalette.Add(0x00BB776D);
            ColorPalette.Add(0x00555555);
            ColorPalette.Add(0x00808080);
            ColorPalette.Add(0x00ACEA88);
            ColorPalette.Add(0x00ABABAB);
        }


        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, ColorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;


            var oTrasform = new ColorTransformReductionMedianCut()
            {
                ColorDistanceEvaluationMode = ColorDistanceEvaluationMode,
                ColorsMax = 4,
                Dithering = Dithering
            };
            oTrasform.Create(oTmpData);
            var oTmpData2 = oTrasform.TransformAndDither(oDataSource);
            BypassDithering = true;
            return oTmpData2;
            //   return oTmpData;

            int R = oTmpData.GetLength(0);
            int C = oTmpData.GetLength(1);
            int[,] oRet = new int[R, C];

            List<ColorTile> lDataBlock = new List<ColorTile>();

            //Parallel.For(0, R / 8, r =>
            for (int r = 0; r < R / iTile; r++)
            {
                for (int c = 0; c < C / iTile; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    ColorTile oTile = new ColorTile()
                    {
                        r = r,
                        c = c,
                        TileData = new int[iArea, iArea],
                    };
                    for (int rr = 0; rr < iArea; rr++)
                    {
                        var rPos = Math.Min(R - 1, Math.Max(0, rr - iOffS + r * iTile));
                        for (int cc = 0; cc < iArea; cc++)
                        {
                            var cPos = Math.Min(C - 1, Math.Max(0, cc - iOffS + c * iTile));
                            var rgb = oTmpData[rPos, cPos];
                            oTile.TileData[rr, cc] = rgb;
                        }
                    }
                    lDataBlock.Add(oTile);
                }
            }

            Parallel.ForEach(lDataBlock, oTile =>
            {

                var TileDataProc = oTile.Process(null);

                for (int rr = 0; rr < iTile; rr++)
                {
                    for (int cc = 0; cc < iTile; cc++)
                    {
                        oRet[oTile.r * iTile + rr, oTile.c * iTile + cc] = TileDataProc[rr + iOffS, cc + iOffS];
                    }
                }
            });

            return oRet;
        }
    }
}