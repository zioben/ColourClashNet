using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
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

        protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);

            switch (propertyName)
            {
                case ColorTransformProperties.UseFixedPalette:
                    CreateSharedPalette=ToBool(value);
                    break;

                case ColorTransformProperties.MaxColorsWanted:
                    ColorsMaxWanted = ToInt(value);
                    break;

                case ColorTransformProperties.UseClustering:
                    LineReductionClustering = ToBool(value);
                    break;
                case ColorTransformProperties.UseColorMean:
                        UseColorMean = ToBool(value);
                    break;
                case ColorTransformProperties.MaxColorChangePerLine:
                    LineReductionMaxColors = ToInt(value);
                    break;
                case ColorTransformProperties.UseSharedPalette:
                    CreateSharedPalette = ToBool(value);
                    break;
                default:
                    break;
            }
            return this;
        }




        protected override ColorTransformResults ExecuteTransform(CancellationToken oToken=default)
        {

            BypassDithering = true;
            ColorListRow.Clear();

            var oRet = new int[SourceData.Rows, SourceData.Columns];
            //var oCols = new int[1, C];
            var oSourceNew = new Imaging.ImageData().Create(SourceData);

            var oLineFixedPalette = FixedPalette;
            // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
            // MainPaletteUsed = false;
            if (CreateSharedPalette)
            {
                var oMainHist = Histogram.CreateHistogram(SourceData);
                var oMainPalette = oMainHist.ToPalette();
                if (oMainPalette.Count <= ColorsMaxWanted)
                {
                    oLineFixedPalette = oMainPalette;
                }
                else
                {
                    ColorTransformInterface oLineTrasf;
                    if (LineReductionClustering)
                    {
                        var oTrasf2 = new ColorTransformReductionCluster()
                        .SetProperty(ColorTransformProperties.MaxColorsWanted, ColorsMaxWanted)
                        .SetProperty(ColorTransformProperties.UseColorMean, UseColorMean)
                        .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 30)
                        .SetProperty(ColorTransformProperties.Fixed_Palette, oLineFixedPalette)
                        .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType);
                        oLineTrasf = oTrasf2;
                    }
                    else
                    {
                        var oTrasf2 = new ColorTransformReductionFast()
                        .SetProperty(ColorTransformProperties.MaxColorsWanted, ColorsMaxWanted)
                        .SetProperty(ColorTransformProperties.Fixed_Palette, oLineFixedPalette)
                        .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType);
                        oLineTrasf = oTrasf2;
                    }

                    oLineTrasf.Create(SourceData);

                    var oMainRet = oLineTrasf.ProcessColors(oToken);
                    oSourceNew = oMainRet.DataOut;
                    var oHistNew = Histogram.CreateHistogram(SourceData);
                    oLineFixedPalette = oHistNew.ToPalette();
                }
            }
            oToken.ThrowIfCancellationRequested();


            // Select the fixed-most used in histogram

            var oRowColors = new List<int>();
            //Parallel.For(0, SourceData.Rows, r  =>
            Parallel.For(0, SourceData.Rows, r =>
            {
                oToken.ThrowIfCancellationRequested();
                var oCols = new int[1, SourceData.Columns];
                for (int c = 0; c < SourceData.Columns; c++)
                {
                    oCols[0, c] = oSourceNew.DataX[r, c];
                }
                // Create row histogram and take the most used colors
                var oHist = Histogram.CreateHistogram(new ImageData().Create(oCols));//.SortColorsDescending();
                var oNewPal = oHist.SortColorsDescending().ToPalette(LineReductionMaxColors);
                // Create 
                //--------------------------------------------------------------
                //    Trace.WriteLine($"Row - {r}");
                if (DitheringType == ColorDithering.None)
                {
                    for (int c = 0; c < SourceData.Columns; c++)
                    {
                        oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
                    }
                }
                else
                {
                    var oTras = new ColorTransformReductionPalette()
                    .SetProperty(ColorTransformProperties.Fixed_Palette, oNewPal)
                    .SetProperty(ColorTransformProperties.MaxColorsWanted, oNewPal.Count)
                    .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
                    .Create(new ImageData().Create(oCols));
                    var oColRes = oTras.ProcessColors(oToken);
                    for (int c = 0; c < SourceData.Columns; c++)
                    {
                        oRet[r, c] = oColRes.DataOut.DataX[0, c];
                    }
                }
                //--------------------------------------------------------------
                // Acceptable
                //for (int c = 0; c < C; c++)
                //{
                //    oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
                //}
            });
            if (oRet != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, new ImageData().Create(oRet));
            }
            return new();

        }
    }
}
