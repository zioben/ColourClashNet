using ColourClashNet.Colors;
using ColourClashSupport.Log;
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

        /// <summary>
        /// Dictionary that contains RGB color as key and its occurrences as value.
        /// </summary>
        public Dictionary<int, int> rgbHistogram { get; private init; } = new Dictionary<int, int>();

        /// <summary>
        /// Reset the histogram
        /// </summary>
        public void Reset()
        {
            rgbHistogram.Clear();
        }

        /// <summary>
        /// Create Histogram from a 2D int array of RGB values
        /// </summary>
        /// <param name="oDataSource">Image Data</param>
        /// <returns>this object</returns>
        public ColorHistogram? Create(int[,] oDataSource)
        {
            Reset();
            return ColorHistogram.CreateColorHist(oDataSource, this);
        }


        /// <summary>
        /// Create Histogram from a Color Palette.<BR/>
        /// Each color will be added with 0 occurrences.
        /// </summary>
        /// <param name="oPalette"></param>
        /// <returns>this object or null on error</returns>
        public ColorHistogram? Create(ColorPalette oPalette)
        {
            string sMethod = nameof(Create);    
            Reset();
            if (oPalette == null)
            {
                LogMan.Error(sClass, sMethod, "null ColorPalette");
                return null;
            }
            foreach( var rgb in oPalette.rgbPalette) 
            {
                AddToHistogram(rgb, 0);
            }
            return this;
        }

        /// <summary>
        /// Create Histogram from a list of Color Palettes.<BR/>
        /// Each color will be added with 0 occurrences.
        /// </summary>
        /// <param name="lPalette"></param>
        /// <returns>this object or null on error</returns>
        public ColorHistogram? Create(List<ColorPalette> lPalette)
        {
            string sMethod = nameof(Create);
            Reset();
            if (lPalette == null || lPalette.Count <= 0)
            {
                LogMan.Error(sClass, sMethod, "null List");
                return null;
            }
            lPalette.ForEach(pal =>
            {
                foreach (var rgb in pal.rgbPalette)
                {
                    AddToHistogram(rgb, 0);
                }
            });
            return this;
        }

        /// <summary>
        /// Gets the number of elements in the RGB histogram.
        /// </summary>
        public int Count => rgbHistogram.Count;

        /// <summary>
        /// Add occurrences to a RGB color in the histogram.<BR/>
        /// only 'ColorIntInfo.IsColor' will be accepted.
        /// </summary>
        /// <param name="rgb">color data</param>
        /// <param name="histAdder">number of occourrences ot color data</param>
        public void AddToHistogram(int rgb, int histAdder)
        {
            if (rgb.GetColorInfo() == ColorIntType.IsColor)
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
        }

        /// <summary>
        /// Extract colors in the histogram as a Color Palette
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Sort colors in the histogram by occurrences in descending order 
        /// </summary>
        /// <returns>this object</returns>
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

        /// <summary>
        /// Sort colors in the histogram by occurrences in ascending order
        /// </summary>
        /// <returns>this object</returns>
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
