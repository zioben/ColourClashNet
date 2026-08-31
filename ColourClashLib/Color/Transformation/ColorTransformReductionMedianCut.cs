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

        public int MaxColorsWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        public bool UseColorMean 
        { 
            get => config.UseColorMean;
            set => config.UseColorMean = value;
        }

        public ColorTransformReductionMedianCut WithProcessingParams(int maxColors, bool useColorMean)
        {
            MaxColorsWanted = maxColors;
            UseColorMean = useColorMean;
            return this;
        }

        public ColorTransformReductionMedianCut WithProcessingParams(ColorTransformConfig cfg) => WithProcessingParams(cfg.MaxColorsWanted, cfg.UseColorMean);





        int GetMedian(List<int> lList)
        {
            float fLim = lList.Sum() / 2f;
            if (fLim <= 0) return 0;

            float fSum = 0;
            for (int i = 0; i < lList.Count; i++)
            {
                fSum += lList[i];
                if (fSum > fLim) return i;
            }
            return lList.Count - 1;
        }


        int GetRange(List<int> lList)
        {
            int first = lList.FindIndex(x => x != 0);
            if (first < 0) return 0; // lista tutta a zero
            int last = lList.FindLastIndex(x => x != 0);
            return last - first;
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

        protected override ColorTransformResult CreateTransformationMap(CancellationToken oToken=default)
        {
            OutputPalette = new Palette();
            var SourceHistogram = new HistogramRGB().Create(ImageSource);
            if (SourceHistogram.ToPalette().Count < MaxColorsWanted)
            {
                foreach (var kvp in SourceHistogram.HistogramDictionary)
                {
                    TransformationMap.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                OutputPalette = TransformationMap.GetOutputPalette();
            }
            else
            {
                int iColorsMax = Math.Min(256, Math.Max(2, MaxColorsWanted));
                Partition(SourceHistogram.ToPalette(), iColorsMax / 2);
                OutputPalette = TransformationMap.GetOutputPalette();
            }
            return ColorTransformResult.CreateValidResult();
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken)
        {
            var ret = TransformationMap.Transform(ImageSource, oToken);
            if (ret != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, ret);
            }
            return new();
        }
    }
}