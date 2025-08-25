using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionScanLine : ColorTransformBase
    {
        public ColorTransformReductionScanLine()
        {
            Type = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }

        public bool CreateSharedPalette { get; set; } = true;
        public int ColorsMaxWanted { get; set; } = 16;
        public int LineReductionMaxColors { get; set; } = 8;
        public bool LineReductionClustering { get; set; } = false;
        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();
        public bool UseColorMean { get; set; } = true;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.UseFixedPalette:
                    if (bool.TryParse(oValue.ToString(), out var cfp))
                    {
                        CreateSharedPalette = cfp;
                        return this;
                    }
                    break;

                case ColorTransformProperties.MaxColorsWanted:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        ColorsMaxWanted = l;
                        return this;
                    }
                    break;
                case ColorTransformProperties.UseClustering:
                    if (bool.TryParse(oValue.ToString(), out var cl))
                    {
                        LineReductionClustering = cl;
                        return this;
                    }
                    break;
                case ColorTransformProperties.UseColorMean:
                    if (bool.TryParse(oValue.ToString(), out var cm))
                    {
                        UseColorMean = cm;
                        return this;
                    }
                    break;
                case ColorTransformProperties.MaxColorChangePerLine:
                    if (int.TryParse(oValue.ToString(), out var ccl))
                    {
                        LineReductionMaxColors = ccl;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return this;
        }


       

        protected override int[,]? ExecuteTransform(int[,]? oSource, CancellationToken oToken)
        {
            if (oSource == null)
                return null;

            this.BypassDithering = true;
            ColorListRow.Clear();

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            //var oCols = new int[1, C];
            var oSourceNew = oSource;

            var oLineFixedPalette = InputFixedColorPalette;
            // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
            // MainPaletteUsed = false;
            if (CreateSharedPalette)
            {
                var oMainHist = new ColorHistogram().Create(oSource);
                var oMainPalette = oMainHist.ToColorPalette();
                if (oMainPalette.Count > ColorsMaxWanted)
                {
                    ColorTransformInterface oLineTrasf;
                    if (LineReductionClustering)
                    {
                        var oTrasf2 = new ColorTransformReductionCluster();
                        oTrasf2.ColorsMaxWanted = ColorsMaxWanted;
                        oTrasf2.UseClusterColorMean = UseColorMean;
                        oTrasf2.TrainingLoop = 30;
                        oLineTrasf = oTrasf2;
                    }
                    else
                    {
                        var oTrasf2 = new ColorTransformReductionFast();
                        oTrasf2.ColorsMaxWanted = ColorsMaxWanted;
                        oLineTrasf = oTrasf2;
                    }

                    oLineTrasf.Create(oSource, oLineFixedPalette);
                    oLineTrasf.Dithering = this.Dithering;

                    var oMainRet = oLineTrasf.ProcessColors(oSource);
                    oSourceNew = oMainRet.DataOut;
                    oLineFixedPalette = new ColorHistogram().Create(oSourceNew).ToColorPalette();
                }
                else
                {
                    oLineFixedPalette = oMainPalette;
                }
            }

           //return oSourceNew;

            // Select the fixed-most used in histogram



            var oRowColors = new List<int>();
            Parallel.For(0, R, r =>
            //for(int r= 0; r < R; r++)
            {
                var oCols = new int[1, C];
                for (int c = 0; c < C; c++)
                {
                    oCols[0, c] = oSourceNew[r, c];
                }
                // Create row histogram and take the most used colors
                var oRowHist = ColorHistogram.CreateColorHistogram(oCols).SortColorsDescending();
                var oRowPal = oRowHist.ToColorPalette();
                var oNewPal = ColorPalette.CreateColorPalette(oRowPal.ToList().Take(LineReductionMaxColors));
                // Create 
                //--------------------------------------------------------------
                //    Trace.WriteLine($"Row - {r}");
                if (Dithering == null || Dithering.Type == ColorDithering.None)
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
                    }
                }
                else
                {
                    var oTras = new ColorTransformReductionPalette()
                                        .Create(oSource, oRowPal)
                                        .SetDithering(Dithering)
                                        .SetProperty(ColorTransformProperties.Output_Palette, oNewPal);
                    var oColRes = oTras.ProcessColors(oCols);
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = oColRes.DataOut[0, c];
                    }
                }
                //--------------------------------------------------------------
                // Acceptable
                //for (int c = 0; c < C; c++)
                //{
                //    oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
                //}

            }
            );
            return oRet;
        }

    }
}