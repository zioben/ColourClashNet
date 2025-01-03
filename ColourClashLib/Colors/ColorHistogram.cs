using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color
{
    public partial class ColorHistogram
    {
        static string sClass = nameof(ColorHistogram);
        public Dictionary<int, int> rgbHistogram { get; private init; } = new Dictionary<int, int>();
       // public int this[int index] => rgbHistogram[index];

        public void Reset()
        {
            rgbHistogram.Clear();
        }

        public bool Create(int[,] oDataSource)
        {
            Reset();
            return ColorHistogram.CreateColorHist(oDataSource, this);
        }

      
        public bool Create(ColorPalette oPalette)
        {
            Reset();
            if (oPalette == null)
                return false;
            foreach( var rgb in oPalette.rgbPalette) 
            {
                AddToHistogram(rgb, 0);
            }
            return true;
        }

        public bool Create(List<ColorPalette> lPalette)
        {
            Reset();
            if (lPalette == null || lPalette.Count<=0 )
                return false;
            lPalette.ForEach(pal =>
            {
                foreach (var rgb in pal.rgbPalette)
                {
                    AddToHistogram(rgb, 0);
                }
            });
            return true;
        }

        public int Count => rgbHistogram.Count;

        public void AddToHistogram(int rgb, int histAdder)
        {
            if (rgbHistogram.ContainsKey(rgb))
            {
                rgbHistogram[rgb] += histAdder;
            }
            else
            {
                rgbHistogram.Add(rgb, histAdder);
            }
        }

        public ColorPalette ToColorPalette()
        {
            var oCP = new ColorPalette();
            foreach (var kvp in rgbHistogram)
            {
                if (kvp.Key < 0)
                    continue;
                oCP.Add(kvp.Key);
            }
            return oCP; 
        }

        //public List<int> HistR()
        //{
        //    var oRet = new int[256];
        //    foreach (var rgb in rgbHistogram)
        //    {
        //        if (rgb.Key > 0)
        //            oRet[rgb.Key.ToR()] += rgb.Value;
        //    }
        //    return oRet.ToList().Where(X => X > 0).ToList();
        //}

        //public List<int> HistG()
        //{
        //    var oRet = new int[256];
        //    foreach (var rgb in rgbHistogram)
        //    {
        //        if (rgb.Key > 0)
        //            oRet[rgb.Key.ToG()] += rgb.Value;
        //    }
        //    return oRet.ToList();
        //}

        //public List<int> HistB()
        //{
        //    var oRet = new int[256];
        //    foreach (var rgb in rgbHistogram)
        //    {
        //        if (rgb.Key > 0)
        //            oRet[rgb.Key.ToB()] += rgb.Value;
        //    }
        //    return oRet.ToList();
        //}

        //static public int Otsu(List<int> lHistogram)
        //{
        //    if (lHistogram.Count == 0)
        //        return 0;

        //    double HistogramAccu = 0;
        //    for (int i = 0; i < lHistogram.Count; i++)
        //    {
        //        HistogramAccu += lHistogram[i];
        //    }

        //    if (HistogramAccu > 0)
        //    {
        //        double sum = 0;
        //        for (int i = 0; i < lHistogram.Count; i++)
        //        {
        //            sum += i * lHistogram[i];
        //        }
        //        double sumB = 0;
        //        double wB = 0;
        //        double wF = 0;
        //        double mB;
        //        double mF;
        //        double max = 0;
        //        double between = 0;
        //        int iTH1 = 0;
        //        int iTH2 = 0;
        //        for (int i = 0; i < lHistogram.Count; i++)
        //        {
        //            wB += lHistogram[i];
        //            if (wB == 0)
        //                continue;
        //            wF = HistogramAccu - wB;
        //            if (wF == 0)
        //                break;
        //            sumB += i * lHistogram[i];
        //            mB = sumB / wB;
        //            mF = (sum - sumB) / wF;
        //            between = wB * wF * (mB - mF) * (mB - mF);
        //            if (between >= max)
        //            {
        //                iTH1 = i;
        //                if (between > max)
        //                {
        //                    iTH2 = i;
        //                }
        //                max = between;
        //            }
        //        }
        //        int iTH = (iTH1 + iTH2) / 2;
        //        return iTH;
        //    }
        //    return 0;
        //}

        //public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        //{
        //    public int Compare(TKey x, TKey y)
        //    {
        //        int result = x.CompareTo(y);

        //        if (result == 0)
        //            return 1; // Handle equality as being greater. Note: this will break Remove(key) or
        //        else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
        //            return -result;
        //    }
        //}

        public void SortColorsDescending()
        {
            List<Tuple<int, int>> oListHist = new List<Tuple<int, int>>(rgbHistogram.Count);
            foreach (var kvp in rgbHistogram)
            {
                oListHist.Add(Tuple.Create(kvp.Value, kvp.Key));
            }
            oListHist = oListHist.OrderByDescending(X => X.Item1).ToList();
            rgbHistogram.Clear();
            foreach (var kvp in oListHist)
            {
                rgbHistogram.Add(kvp.Item2, kvp.Item1);
            }
        }
        public void SortColorsAscending()
        {
            List<Tuple<int, int>> oListHist = new List<Tuple<int, int>>(rgbHistogram.Count);
            foreach (var kvp in rgbHistogram)
            {
                oListHist.Add(Tuple.Create(kvp.Value, kvp.Key));
            }
            oListHist = oListHist.OrderBy(X => X.Item1).ToList();
            rgbHistogram.Clear();
            foreach (var kvp in oListHist)
            {
                rgbHistogram.Add(kvp.Item2, kvp.Item1);
            }
        }
    }


}
