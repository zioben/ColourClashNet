using ColourClashNet.Colors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionZxSpectrum : ColorTransformToPalette
    {

        static int iArea = 8;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public ColorTransformReductionZxSpectrum()
        {
            Type = ColorTransform.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        HashSet<int> hPalette = new HashSet<int>();

        public int ColL { get; set; } = 0x00AA;
        public int ColH { get; set; } = 0x00FF;

        int iColOutL = 0x00CC;
        int iColOutH = 0x00FF;
        

        protected override void CreateTrasformationMap()
        {
            int c00 = ColorIntExt.FromRGB(0, 0, ColL);
            int c01 = ColorIntExt.FromRGB(ColL, 0, 0);
            int c02 = ColorIntExt.FromRGB(ColL, 0, ColL);
            int c03 = ColorIntExt.FromRGB(0, ColL, 0);
            int c04 = ColorIntExt.FromRGB(0, ColL, ColL);
            int c05 = ColorIntExt.FromRGB(ColL, ColL, 0);
            int c06 = ColorIntExt.FromRGB(0, 0, ColH);
            int c07 = ColorIntExt.FromRGB(ColH, 0, 0);
            int c08 = ColorIntExt.FromRGB(ColH, 0, ColH);
            int c09 = ColorIntExt.FromRGB(0, ColH, 0);
            int c10 = ColorIntExt.FromRGB(0, ColH, ColH);
            int c11 = ColorIntExt.FromRGB(ColH, ColH, 0);
            int c12 = ColorIntExt.FromRGB(0, 0, 0);
            int c13 = ColorIntExt.FromRGB(ColL, ColL, ColL);
            int c14 = ColorIntExt.FromRGB(ColH, ColH, ColH);

            ColorTransformationPalette.Clear();
            ColorTransformationPalette.Add(c00);
            ColorTransformationPalette.Add(c01);
            ColorTransformationPalette.Add(c02);
            ColorTransformationPalette.Add(c03);
            ColorTransformationPalette.Add(c04);
            ColorTransformationPalette.Add(c05);
            ColorTransformationPalette.Add(c06);
            ColorTransformationPalette.Add(c07);
            ColorTransformationPalette.Add(c08);
            ColorTransformationPalette.Add(c09);
            ColorTransformationPalette.Add(c10);
            ColorTransformationPalette.Add(c11);
            ColorTransformationPalette.Add(c12);
            ColorTransformationPalette.Add(c13);
            ColorTransformationPalette.Add(c14);
        }

        internal class Tile
        {
            internal int r { get; set; }
            internal int c { get; set; }
            internal int[,] TileData { get; set; }
            //internal int[,] TileDataProc { get; set; }

            internal ColorTransformReductionCluster oReduction = new ColorTransformReductionCluster()
            {
                ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
                ColorsMax = 2,
                UseClusterColorMean = false, 
                 TrainingLoop = 2,
            };

            internal int[,] Process( ColorDitherInterface oDither )
            {
                oReduction.Create(TileData);
                oReduction.Dithering = oDither; 
                return oReduction.TransformAndDither(TileData);
            }

        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, ColorTransformationPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;
         //   return oTmpData;

            int R = oTmpData.GetLength(0);
            int C = oTmpData.GetLength(1);
            int[,] oRet = new int[R, C];

            List<Tile> lDataBlock= new List<Tile>();

            //Parallel.For(0, R / 8, r =>
            for( int r =0; r < R/iTile; r++ )
            {
                for (int c = 0; c < C / iTile; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    Tile oTile = new Tile()
                    {
                        r = r,
                        c = c,
                        TileData = new int[iArea, iArea],
                    };
                    for (int rr = 0; rr < iArea; rr++)
                    {
                        var rPos = Math.Min( R-1, Math.Max(0, rr-iOffS + r * iTile) );
                        for (int cc = 0; cc < iArea; cc++)
                        {
                            var cPos = Math.Min( C-1, Math.Max(0, cc-iOffS + c * iTile) );
                            var rgb = oTmpData[rPos,cPos];
                            oTile.TileData[rr, cc]=rgb;
                        }
                    }
                    lDataBlock.Add(oTile);
                }
            }

            Parallel.ForEach( lDataBlock, oTile =>
                {

                    var   TileDataProc = oTile.Process( null );
                  
                    for (int rr = 0; rr < iTile; rr++)
                    {
                        for (int cc = 0; cc < iTile; cc++)
                        {
                            oRet[oTile.r * iTile + rr, oTile.c * iTile + cc] = TileDataProc[rr+iOffS, cc+iOffS];
                        }
                    }
                });

            //return oRet;
            int c00 = ColorIntExt.FromRGB(0, 0, ColL);
            int c01 = ColorIntExt.FromRGB(ColL, 0, 0);
            int c02 = ColorIntExt.FromRGB(ColL, 0, ColL);
            int c03 = ColorIntExt.FromRGB(0, ColL, 0);
            int c04 = ColorIntExt.FromRGB(0, ColL, ColL);
            int c05 = ColorIntExt.FromRGB(ColL, ColL, 0);
            int c06 = ColorIntExt.FromRGB(0, 0, ColH);
            int c07 = ColorIntExt.FromRGB(ColH, 0, 0);
            int c08 = ColorIntExt.FromRGB(ColH, 0, ColH);
            int c09 = ColorIntExt.FromRGB(0, ColH, 0);
            int c10 = ColorIntExt.FromRGB(0, ColH, ColH);
            int c11 = ColorIntExt.FromRGB(ColH, ColH, 0);
            int c12 = ColorIntExt.FromRGB(0, 0, 0);
            int c13 = ColorIntExt.FromRGB(ColL, ColL, ColL);
            int c14 = ColorIntExt.FromRGB(ColH, ColH, ColH);
            ColorTransformationMap.Clear();
            ColorTransformationMap.Add(c00, ColorIntExt.FromRGB(0, 0, iColOutL));
            ColorTransformationMap.Add(c01, ColorIntExt.FromRGB(iColOutL, 0, 0));
            ColorTransformationMap.Add(c02, ColorIntExt.FromRGB(iColOutL, 0, iColOutL));
            ColorTransformationMap.Add(c03, ColorIntExt.FromRGB(0, iColOutL, 0));
            ColorTransformationMap.Add(c04, ColorIntExt.FromRGB(0, iColOutL, iColOutL));
            ColorTransformationMap.Add(c05, ColorIntExt.FromRGB(iColOutL, iColOutL, 0));
            ColorTransformationMap.Add(c06, ColorIntExt.FromRGB(0, 0, iColOutH));
            ColorTransformationMap.Add(c07, ColorIntExt.FromRGB(iColOutH, 0, 0));
            ColorTransformationMap.Add(c08, ColorIntExt.FromRGB(iColOutH, 0, iColOutH));
            ColorTransformationMap.Add(c09, ColorIntExt.FromRGB(0, iColOutH, 0));
            ColorTransformationMap.Add(c10, ColorIntExt.FromRGB(0, iColOutH, iColOutH));
            ColorTransformationMap.Add(c11, ColorIntExt.FromRGB(iColOutH, iColOutH, 0));
            ColorTransformationMap.Add(c12, ColorIntExt.FromRGB(0, 0, 0));
            ColorTransformationMap.Add(c13, ColorIntExt.FromRGB(iColOutL, iColOutL, iColOutL));
            ColorTransformationMap.Add(c14, ColorIntExt.FromRGB(iColOutH, iColOutH, iColOutH));
            var oZxRet = ColorTransformBase.ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}