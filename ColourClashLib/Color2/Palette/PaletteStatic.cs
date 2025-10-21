using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColourClashNet.Color
{
    public partial class Palette
    {
        /// <summary>
        /// Merges multiple color palettes into a single palette.
        /// Returns an empty palette if the input list is null or empty.
        /// </summary>
        public static Palette MergeColorPalette(IEnumerable<Palette>? sourcePalettes)
        {
            var result = new Palette();
            if (sourcePalettes == null)
                return result;

            foreach (var palette in sourcePalettes)
            {
                if (palette == null)
                    continue;

                foreach (var color in palette.rgbPalette)
                    result.Add(color);
            }

            return result;
        }

        /// <summary>
        /// Merges two color palettes into a single palette.
        /// </summary>
        public static Palette MergeColorPalette(Palette a, Palette b)
        {
            return MergeColorPalette(new[] { a, b });
        }

        /// <summary>
        /// Creates a new palette from a single source palette.
        /// </summary>
        public static Palette CreateColorPalette(Palette sourcePalette)
        {
            return MergeColorPalette(new[] { sourcePalette });
        }

        /// <summary>
        /// Creates a palette from a list of RGB integers.
        /// </summary>
        public static Palette CreateColorPalette(List<int>? sourceList)
        {
            var result = new Palette();
            if (sourceList == null || sourceList.Count == 0)
                return result;

            foreach (var color in sourceList)
                result.Add(color);

            return result;
        }

        /// <summary>
        /// Creates a palette from an enumerable of RGB integers.
        /// </summary>
        public static Palette CreateColorPalette(IEnumerable<int>? sourceEnumerable)
        {
            return CreateColorPalette(sourceEnumerable?.ToList());
        }
    }
}