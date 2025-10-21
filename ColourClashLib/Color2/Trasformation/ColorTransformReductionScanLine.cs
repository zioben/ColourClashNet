using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

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




        protected override async Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            return await Task.Run(async () =>
            {

                BypassDithering = true;
                ColorListRow.Clear();

                var R = SourceData.GetLength(0);
                var C = SourceData.GetLength(1);
                var oRet = new int[R, C];
                //var oCols = new int[1, C];
                var oSourceNew = SourceData.Clone() as int[,];

                var oLineFixedPalette = FixedPalette;
                // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
                // MainPaletteUsed = false;
                if (CreateSharedPalette)
                {
                    var oMainHist = new Histogram();
                    await oMainHist.CreateAsync(SourceData, oToken);
                    var oMainPalette = oMainHist.ToColorPalette();
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
                            .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType);
                            oLineTrasf = oTrasf2;
                        }
                        else
                        {
                            var oTrasf2 = new ColorTransformReductionFast()
                            .SetProperty(ColorTransformProperties.MaxColorsWanted, ColorsMaxWanted)
                            .SetProperty(ColorTransformProperties.Fixed_Palette, oLineFixedPalette)
                            .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType);
                            oLineTrasf = oTrasf2;
                        }

                        await oLineTrasf.CreateAsync(SourceData, oToken);

                        var oMainRet = await oLineTrasf.ProcessColorsAsync(oToken);
                        oSourceNew = oMainRet.DataOut;
                        var oHistNew = new Histogram();
                        await oHistNew.CreateAsync(SourceData, oToken);
                        oLineFixedPalette = oHistNew.ToColorPalette();
                    }
                }
                oToken?.ThrowIfCancellationRequested();


                // Select the fixed-most used in histogram

                var oRowColors = new List<int>();
                await Parallel.ForAsync(0, R, oToken ?? CancellationToken.None, async (r, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    var oCols = new int[1, C];
                    for (int c = 0; c < C; c++)
                    {
                        oCols[0, c] = oSourceNew[r, c];
                    }
                    // Create row histogram and take the most used colors
                    var oHist = await Histogram.CreateColorHistogramAsync(oCols, oToken);//.SortColorsDescending();
                    var oNewPal = oHist.SortColorsDescending().ToColorPalette(LineReductionMaxColors);
                    // Create 
                    //--------------------------------------------------------------
                    //    Trace.WriteLine($"Row - {r}");
                    if (DitheringType == ColorDithering.None)
                    {
                        for (int c = 0; c < C; c++)
                        {
                            oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
                        }
                    }
                    else
                    {
                        var oTras = new ColorTransformReductionPalette()
                        .SetProperty(ColorTransformProperties.Fixed_Palette, oNewPal)
                        .SetProperty(ColorTransformProperties.MaxColorsWanted, oNewPal.Count)
                        .SetProperty(ColorTransformProperties.Dithering_Model, DitheringType);
                        await oTras.CreateAsync(oCols, oToken);
                        var oColRes = await oTras.ProcessColorsAsync(oToken);
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
                });
                if (oRet != null)
                {
                    return ColorTransformResults.CreateValidResult(SourceData, oRet);
                }
                return new();
            });

        }
    }
}