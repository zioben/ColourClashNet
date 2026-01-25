using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionMedianCut : ColorTransformBase
    {

        public ColorTransformReductionMedianCut()
        {
            Type = ColorTransformType.ColorReductionMedianCut;
            Description = "Median partition color reduction";
        }

        public int ColorsMaxWanted { get; set; } = -1;
        public bool UseColorMean { get; set; } = true;

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);

            switch (propertyName)
            {
                case ColorTransformProperties.MaxColorsWanted:                 
                        ColorsMaxWanted = ToInt(value);
                    break;
               
                case ColorTransformProperties.UseColorMean:
                        UseColorMean = ToBool(value);
                    break;
                default:
                    break;
            }
            return this;
        }


        int GetMedian(List<int> lList)
        {
            float fLim = lList.Sum() / 2;
            if (fLim <= 0)
            {
                return 0;
            }
            int i = 0;
            float fSum = lList[i];
            while (fSum <= fLim && i < lList.Count())
            {
                i++;
                fSum += lList[i];
            }
            return i;
        }

        int GetRange(List<int> lList)
        {
            var f = lList.IndexOf(lList.FirstOrDefault(X => X != 0));
            lList.Reverse();
            var l = lList.Count() - lList.IndexOf(lList.FirstOrDefault(X => X != 0)) - 1;
            lList.Reverse();
            return l - f;
        }

        void Partition(Palette oPalette, int iMaxColor)
        {
            var rgbList = oPalette.ToList();
            if (iMaxColor > 0)
            {

                var lR = new int[256].ToList();
                var lG = new int[256].ToList();
                var lB = new int[256].ToList();
                foreach (var rgb in rgbList)
                {
                    lR[rgb.ToR()]++;
                    lG[rgb.ToG()]++;
                    lB[rgb.ToB()]++;
                }
                int ird = GetRange(lR);
                int igd = GetRange(lG);
                int ibd = GetRange(lB);
                Palette hInf = new Palette();
                Palette hSup = new Palette();
                if (ird > igd && ird > ibd)
                {
                    var irm = GetMedian(lR);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToR() <= irm)
                            hInf.Add(rgb);
                        else
                            hSup.Add(rgb);
                    }
                }
                else if (igd > ibd)
                {
                    var igm = GetMedian(lG);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToG() <= igm)
                            hInf.Add(rgb);
                        else
                            hSup.Add(rgb);
                    }
                }
                else
                {
                    var ibm = GetMedian(lB);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToB() <= ibm)
                            hInf.Add(rgb);
                        else
                            hSup.Add(rgb);
                    }
                }
                Partition(hInf, iMaxColor / 2);
                Partition(hSup, iMaxColor / 2);
                return;
            }
            else
            {
                if (oPalette.Count == 0)
                    return;
                var iRGB = ColorIntExt.GetColorMean(oPalette, ColorMeanMode.UseColorPalette);
                foreach (var rgb in rgbList)
                {
                    if (!TransformationMap.rgbTransformationMap.ContainsKey(rgb))
                    {
                        TransformationMap.Add(rgb, iRGB);
                    }
                }
            }
        }

        Palette OutputPalette = new Palette();

        protected override ColorTransformResults CreateTransformationMap(CancellationToken oToken=default)
        {
            OutputPalette = new Palette();
            var SourceHistogram = Histogram.CreateHistogram(SourceData);
            if (SourceHistogram.ToPalette().Count < ColorsMaxWanted)
            {
                foreach (var kvp in SourceHistogram.rgbHistogram)
                {
                    TransformationMap.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                OutputPalette = TransformationMap.GetOutputPalette();
            }
            else
            {
                int iColorsMax = Math.Min(256, Math.Max(2, ColorsMaxWanted));
                Partition(SourceHistogram.ToPalette(), iColorsMax / 2);
                OutputPalette = TransformationMap.GetOutputPalette();
            }
            return ColorTransformResults.CreateValidResult();
        }

        protected override ColorTransformResults ExecuteTransform(CancellationToken oToken)
        {
            var ret = TransformationMap.Transform(SourceData, oToken);
            if (ret != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, ret);
            }
            return new();
        }
    }
}