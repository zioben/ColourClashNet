using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color
{
    /// <summary>
    /// Color Histogram evaluation.<BR/>
    /// Represent Histogram RGB Color with its occurrences.
    /// </summary>
    public partial class ColorHistogram
    {
        static string sClass = nameof(ColorHistogram);
        public Dictionary<int, int> rgbHistogram { get; private init; } = new Dictionary<int, int>();
       // public int this[int index] => rgbHistogram[index];

        public void Reset()
        {
            rgbHistogram.Clear();
        }

        public ColorHistogram? Create(int[,] oDataSource)
        {
            Reset();
            return ColorHistogram.CreateColorHist(oDataSource, this);
        }

      
        public ColorHistogram? Create(ColorPalette oPalette)
        {
            Reset();
            if (oPalette == null)
                return null;
            foreach( var rgb in oPalette.rgbPalette) 
            {
                AddToHistogram(rgb, 0);
            }
            return this;
        }

        public ColorHistogram? Create(List<ColorPalette> lPalette)
        {
            Reset();
            if (lPalette == null || lPalette.Count<=0 )
                return null;
            lPalette.ForEach(pal =>
            {
                foreach (var rgb in pal.rgbPalette)
                {
                    AddToHistogram(rgb, 0);
                }
            });
            return this;
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

        public ColorHistogram? SortColorsDescending()
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
            return this;
        }
        public ColorHistogram? SortColorsAscending()
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
            return this;
        }
    }


}
