using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public partial class Palette
    {
        /// <summary>
        /// Merges multiple color palettes into a single color palette.
        /// </summary>
        /// <remarks>This method iterates through each <see cref="Palette"/> in the provided list and
        /// adds all colors from each palette to the resulting palette.  If a palette in the list is <see
        /// langword="null"/>, it is skipped. If no colors are added to the resulting palette, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="lSourcePalette">A list of <see cref="Palette"/> objects to merge. Each palette in the list may contain a collection of
        /// colors.</param>
        /// <returns>A new <see cref="Palette"/> containing all unique colors from the input palettes, or <see
        /// langword="null"/> if the input list is <see langword="null"/>  or contains no valid palettes with colors.</returns>
        public static Palette? MergeColorPalette(List<Palette> lSourcePalette)
        {
            var oRet = new Palette();
            if (lSourcePalette != null)
            {
                foreach (var oPal in lSourcePalette)
                {
                    if (oPal == null)
                        continue;
                    foreach (var iRGB in oPal.rgbPalette)
                    {
                        oRet.Add(iRGB);
                    }
                }
            }
            return oRet;
        }

        /// <summary>
        /// Merges two color palettes into a single color palette.  
        /// </summary>
        /// <remarks>This method combines the colors from the two provided palettes into a single palette.
        /// Duplicate colors may be handled based on the implementation of the underlying merge logic.</remarks>
        /// <param name="oSourcePaletteA">The first color palette to merge.</param>
        /// <param name="oSourcePaletteB">The second color palette to merge.</param>
        /// <returns>A new <see cref="Palette"/> that contains the combined colors from both input palettes,  or <see
        /// langword="null"/> if the merge operation fails.</returns>
        public static Palette? MergeColorPalette(Palette oSourcePaletteA, Palette oSourcePaletteB)
        {
            return MergeColorPalette(new List<Palette> { oSourcePaletteA, oSourcePaletteB });
        }

        /// <summary>
        /// Creates a new color palette based on the provided source palette.
        /// </summary>
        /// <param name="oSourcePalette">Source color palette</param>
        /// <returns>ColorPalette or null on error</returns>
        public static Palette? CreateColorPalette(Palette oSourcePalette)
        {
            return MergeColorPalette(new List<Palette> { oSourcePalette } );
        }

        /// <summary>
        /// Creates a ColorPalette from a list of integer RGB values.
        /// </summary>
        /// <param name="oSourceList">List of RGB values</param>
        /// <returns>ColorPalette or null on error</returns>
        public static Palette? CreateColorPalette(List<int>? oSourceList)
        {
            var oRet = new Palette();
            if (oSourceList == null || oSourceList.Count == 0)
                return oRet;
            oSourceList.ForEach(X => oRet.Add(X));
            return oRet;    
        }

        public static Palette? CreateColorPalette(IEnumerable<int>? oSourceEnumerable) => CreateColorPalette(oSourceEnumerable?.ToList());
    }
}
