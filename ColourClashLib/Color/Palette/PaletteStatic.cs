using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColourClashNet.Color;

/// <summary>
/// Static class for palette creation
/// </summary>
public partial class Palette
{
    static string sC = nameof(Palette);

    /// <summary>
    /// Creates a new palette by merging the colors from the specified collection of palettes.
    /// </summary>
    /// <param name="sourcePalettes">A collection of palettes whose colors will be combined. Null palettes within the collection are ignored. If the
    /// collection itself is null, an empty palette is returned.</param>
    /// <returns>A new Palette instance containing all colors from the non-null palettes in the collection. If no palettes are
    /// provided or all are null, the returned palette will be empty.</returns>
    public static Palette MergePalette(IEnumerable<Palette?> sourcePalettes)
    {
        var sM = nameof(MergePalette);
        try
        {
            var result = new Palette();
            if (sourcePalettes == null)
            {
                LogMan.Error(sC, sM, "invalis source palette list");
                return result;
            }

            foreach (var palette in sourcePalettes)
            {
                if (palette == null)
                    continue;

                foreach (var color in palette.rgbPalette)
                    result.Add(color);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, ex);
            return new Palette();
        }
    }

   /// <summary>
   /// Merges two palettes into a single palette containing the combined colors from both inputs.
   /// </summary>
   /// <remarks>If both palettes contain the same color, the resulting palette may handle duplicates according
   /// to the implementation of the underlying merge logic.</remarks>
   /// <param name="paletteA">The first palette to merge. Cannot be null.</param>
   /// <param name="paletteB">The second palette to merge. Cannot be null.</param>
   /// <returns>A new Palette instance containing the combined colors from both input palettes.</returns>
    public static Palette MergePalette(Palette paletteA, Palette paletteB)
    {
        return MergePalette(new[] { paletteA, paletteB });
    }

    /// <summary>
    /// Creates a new palette instance based on the specified source palette.
    /// </summary>
    /// <param name="sourcePalette">The palette to use as the basis for the new palette. Cannot be null.</param>
    /// <returns>A new Palette instance that is a copy of the specified source palette.</returns>
    public static Palette CreatePalette(Palette sourcePalette)
    {
        var sM = nameof(CreatePalette);
        return MergePalette(new[] { sourcePalette });
    }



   /// <summary>
   /// Creates a new Palette instance using the specified sequence of color values.
   /// </summary>
   /// <param name="sourceEnumerable">An enumerable collection of integers representing color values to include in the palette. Cannot be null.</param>
   /// <returns>A Palette object containing the colors specified in the source enumerable.</returns>
    public static Palette CreatePalette(IEnumerable<int> sourceEnumerable)
       => new Palette().Create(sourceEnumerable);

    /// <summary>
    /// Creates a color palette by extracting valid color values from the specified two-dimensional array.
    /// </summary>
    /// <param name="matrix">A two-dimensional array of integers representing color data. Each element is evaluated to determine if it
    /// represents a valid color.</param>
    /// <returns>A Palette containing all valid colors found in the input array. If no valid colors are found or if the input
    /// is null, the returned Palette will be empty.</returns>
    public static Palette CreatePalette(int[,] matrix)
       => new Palette().Create(matrix);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oImageData"></param>
    /// <returns></returns>
    public static Palette CreatePalette(ImageData oImageData)
       => new Palette().Create(oImageData);
}