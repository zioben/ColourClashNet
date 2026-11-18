using System;
using System.Collections.Generic;
using System.Linq;

namespace ColourClashNet.Color;

/// <summary>
/// Static palette helpers.
/// </summary>
public partial class Palette
{
    static string sC = nameof(Palette);

    /// <summary>
    /// Creates a new palette by merging the colors from the specified collection of palettes.
    /// </summary>
    /// <param name="sourcePalettes">Palettes to combine; null items are ignored.</param>
    /// <returns>A new palette with the union of all colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sourcePalettes"/> is null.</exception>
    public static Palette MergePalette(IEnumerable<Palette?> sourcePalettes)
    {

        var result = new Palette();
        if (sourcePalettes == null)
           throw new ArgumentNullException(nameof(sourcePalettes));

        foreach (var palette in sourcePalettes)
        {
            if (palette == null)
                continue;

            foreach (var color in palette.RgbPalette)
                result.Add(color);
        }

        return result;
    }
    

    /// <summary>
    /// Merges two palettes (set union, no duplicates).
    /// </summary>
    /// <param name="paletteA">First palette; may be null.</param>
    /// <param name="paletteB">Second palette; may be null.</param>
    /// <returns>A new palette with the colors of both inputs.</returns>
    public static Palette MergePalette(Palette paletteA, Palette paletteB)
    {
        return MergePalette(new[] { paletteA, paletteB });
    }
}