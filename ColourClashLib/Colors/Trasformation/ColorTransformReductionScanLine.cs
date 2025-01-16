using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
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
            Name = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }
        public int ColorsMaxWanted { get; set; } = -1;
        public int FixedColorPerLine { get; set; } = 0;
        public int ChangingColorPerLine { get; set; } = 7;
        public bool Clustering { get; set; }
        public bool ClusteringUseColorMean { get; set; }

        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.MaxColorsWanted:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        ColorsMaxWanted = l;
                        return this;
                    }
                    break;
                case ColorTransformProperties.ScanlineUseClustering:
                    if (bool.TryParse(oValue.ToString(), out var cl))
                    {
                        Clustering = cl;
                        return this;
                    }
                    break;
                case ColorTransformProperties.UseColorMean:
                    if (bool.TryParse(oValue?.ToString(), out var cm))
                    {
                        ClusteringUseColorMean = cm;
                        return this;
                    }
                    break;
                case ColorTransformProperties.MaxColorChangePerLine:
                    if (int.TryParse(oValue.ToString(), out var ccl))
                    {
                        ChangingColorPerLine = ccl;
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
            var oCols = new int[1, C];

            // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
            ColorTransformInterface oTrasf;
            if (Clustering)
            {
                var oTrasf2 = new ColorTransformReductionCluster();
                oTrasf2.ColorsMaxWanted = ColorsMaxWanted;
                oTrasf2.UseClusterColorMean = ClusteringUseColorMean;
                oTrasf2.TrainingLoop = 30;
                oTrasf = oTrasf2;
            }
            else
            {
                var oTrasf2 = new ColorTransformReductionFast();
                oTrasf2.ColorsMaxWanted = ColorsMaxWanted;
                oTrasf = oTrasf2;
            }
            oTrasf.Create(oSource, null);
            oTrasf.Dithering = this.Dithering;  
            var oFirstRes = oTrasf.TransformAndDither(oSource);
            if( oFirstRes == null || oFirstRes.DataOut == null)
            {
                return null;
            }
            var oFirstHist = new ColorHistogram();
            oFirstHist.Create(oFirstRes.DataOut);
            var oFirstPal = oFirstHist.ToColorPalette();
            // Step 2: Using the target image to reduce to a fix part and a scanline part (16 -> 8 fixed + 8 selectable)
            // Select the fixed-most used in histogram

            //            var oPalette1 = oTrasf.Palette;
            //          var oPalette = 

            return oFirstRes.DataOut;
            HashSet<int> oRowColors = new HashSet<int>();

            for (int r = 0; r < R; r++)
            {
                Array.Clear(oCols);
                for (int c = 0; c < C; c++)
                {
                    oCols[0, c] = oSource[r, c];
                }
                oTrasf.Create(oCols, FixedColorPalette);
                var oColsRes = oTrasf.TransformAndDither(oCols);
                var oColsTrasf = oColsRes.DataOut;
                if (oColsTrasf == null)
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = -1;
                    }
                }
                else
                {
                    for (int c = 0; c < C; c++)
                    {
                        oRet[r, c] = oColsTrasf[0, c];
                    }
                }

                oRowColors.Clear();
                foreach (var kvp in oTrasf.ColorTransformationMapper.rgbTransformationMap)
                {
                    oRowColors.Add(kvp.Value);
                }
                ColorListRow.Add(oRowColors.ToList());
            }
            return oRet;
        }

    }
}