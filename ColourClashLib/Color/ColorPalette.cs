using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color
{
    /// <summary>
    /// Class that enumerates distinct colors
    /// <para>
    /// The class in intended to operate only with "real" colors as defined in the <see cref="ColorIntInfo"/> Enum
    /// </para>
    /// </summary>
    public partial class ColorPalette
    {

        /// <summary>
        /// Color Palette. Only 'ColorIntInfo.IsColor' should be set.
        /// <para>
        /// Use Add() and Remove() methods. Don't add int values directly to avoid unexpected behaviors on reduction algorithms
        /// </para>
        /// </summary>
        public HashSet<int> rgbPalette { get; private init; } = new HashSet<int>();

        /// <summary>
        /// Distinct colors in the Palette
        /// </summary>
        public int Count => rgbPalette.Count;

        /// <summary>
        /// Add a RGB color in the palette. Only 'ColorIntInfo.IsColor' will be accepted
        /// </summary>
        /// <param name="iRGB"></param>
        public void Add(int iRGB )
        {
            if (iRGB.GetColorInfo() == ColorIntType.IsColor)
            {
                rgbPalette.Add(iRGB);
            }
        }

        /// <summary>
        /// Remove an RGB color to the Palette. If color is not present Palette not changes.
        /// </summary>
        /// <param name="iRGB"></param>
        public void Remove(int iRGB) => rgbPalette.Remove(iRGB);

        /// <summary>
        /// Convert to List of integer colors
        /// </summary>
        /// <returns></returns>
        public List<int> ToList() => rgbPalette.ToList();
        public void Reset()
        {
            rgbPalette.Clear(); 
        }

        public override string ToString()
        {
            return $"Palette Colors : {Count}";
        }
    }
}
