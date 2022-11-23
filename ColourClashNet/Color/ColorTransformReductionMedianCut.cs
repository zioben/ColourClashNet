using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionMedianCut : ColorTransformBase
    {

        public ColorTransformReductionMedianCut()
        {
            Type = ColorTransform.ColorReductionMedianCut;
            Description = "Median partition color reduction";
        }

        public int ColorsMax { get; set; } = -1;
        public bool UseClusterColorMean { get; set; } = true;

        int GetMedian(List<int> lList)
        {
            float fLim = lList.Sum()/2;
            if(fLim <= 0 ) 
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
            var l = lList.Count() - lList.IndexOf(lList.FirstOrDefault(X => X != 0))-1;
            lList.Reverse();
            return l - f;
        }

        void Partition(HashSet<int> hPalette, int iMaxColor )
        {
            if (iMaxColor > 0)
            {

                var lR = new int[256].ToList();
                var lG = new int[256].ToList();
                var lB = new int[256].ToList();
                foreach (var rgb in hPalette)
                {
                    lR[rgb.ToR()]++;
                    lG[rgb.ToG()]++;
                    lB[rgb.ToB()]++;
                    //lR[rgb.ToR()]=1;
                    //lG[rgb.ToG()]=1;
                    //lB[rgb.ToB()]=1;
                }
                int ird = GetRange(lR);
                int igd = GetRange(lG);// lG.IndexOf(lG.LastOrDefault(X => X != 0)) - lG.IndexOf(lG.FirstOrDefault(X => X != 0));
                int ibd = GetRange(lB); //lB.IndexOf(lB.LastOrDefault(X => X != 0)) - lB.IndexOf(lB.FirstOrDefault(X => X != 0));
                HashSet<int> hInf = new HashSet<int>();
                HashSet<int> hSup = new HashSet<int>();
                if (ird > igd && ird > ibd)
                {
                    var irm = GetMedian(lR);
                    foreach (var rgb in hPalette)
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
                    foreach (var rgb in hPalette)
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
                    foreach (var rgb in hPalette)
                    {
                        if (rgb.ToB() <= ibm)
                            hInf.Add(rgb);
                        else
                            hSup.Add(rgb);
                    }
                }
                Partition(hInf, iMaxColor / 2);
                Partition(hSup, iMaxColor / 2);
            }
            else
            {
                if (hPalette.Count == 0)
                    return;
                int Count = 0;
                double R = 0;
                double G = 0;
                double B = 0;
                foreach (var rgb in hPalette)
                {
                    int iHistValue = ColorHistogram[rgb];
                    Count += iHistValue;
                    R += iHistValue * rgb.ToR();
                    G += iHistValue * rgb.ToG();
                    B += iHistValue * rgb.ToB();
                }
                R /= Count;
                G /= Count;
                B /= Count;
                var iRGBMean = ColorIntExt.FromRGB(R, G, B);
                ColorTransformationPalette.Add(iRGBMean);
                int irgbOut = 0;
                if (UseClusterColorMean)
                {
                    irgbOut = 0;
                }
                else
                {
                    irgbOut = GetNearestColor(iRGBMean, hPalette, ColorDistanceEvaluationMode);
                }
                foreach (var rgb in hPalette)
                {
                    ColorTransformationMap.Add(rgb, irgbOut);
                }
            }
        }

        protected override void CreateTrasformationMap()
        {
            if (ColorHistogram.Count < ColorsMax)
            {
                foreach (var kvp in ColorHistogram)
                {
                    ColorTransformationPalette.Add(kvp.Key);
                    ColorTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            var hPalette = new HashSet<int>();
            foreach (var kvp in ColorHistogram)
            { 
                hPalette.Add(kvp.Key);
            }
            hPalette.Remove(-1);
            int iColorsMax = Math.Min(256, Math.Max(2, ColorsMax));
            Partition(hPalette,  iColorsMax/2 );
        }

   
    }
}