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
    /// Merges multiple color palettes into a single palette containing all unique colors from the provided
    /// palettes.
    /// </summary>
    /// <param name="sourcePalettes">A collection of palettes to merge. Palettes in the collection may be null and will be ignored. If null, an
    /// empty palette is returned.</param>
    /// <returns>A new Palette instance containing all colors from the non-null palettes in the collection. If no palettes
    /// are provided or all are null, the returned palette will be empty.</returns>
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
   /// Merges two color palettes into a single palette.
   /// </summary>
   /// <param name="a">The first palette to merge. Can be null.</param>
   /// <param name="b">The second palette to merge. Can be null.</param>
   /// <returns>A new Palette instance containing the merged colors from both input palettes. If both parameters are null,
   /// returns an empty palette.</returns>
    public static Palette MergePalette(Palette a, Palette b)
    {
        return MergePalette(new[] { a, b });
    }

    /// <summary>
    /// Creates a new color palette based on the specified source palette.
    /// </summary>
    /// <param name="sourcePalette">The palette to use as the basis for the new color palette. Can be null.</param>
    /// <returns>A new Palette instance containing the colors from the source palette.</returns>
    public static Palette CreatePalette(Palette sourcePalette)
    {
        var sM = nameof(CreatePalette);
        return MergePalette(new[] { sourcePalette });
    }



    /// <summary>
    /// Creates a color palette from the specified sequence of integer color values.
    /// </summary>
    /// <param name="sourceEnumerable">An enumerable collection of integers representing color values to include in the palette. Can be null to
    /// create an empty palette.</param>
    /// <returns>A Palette instance containing the colors specified in the source enumerable. Returns an empty palette if
    /// sourceEnumerable is null or contains no elements.</returns>
    public static Palette CreatePalette(IEnumerable<int> sourceEnumerable)
       => new Palette().Create(sourceEnumerable);

    /// <summary>
    /// Creates a color palette by extracting valid color values from the specified two-dimensional array.
    /// </summary>
    /// <param name="mData">A two-dimensional array of integers representing color data. Each element is evaluated to determine if it
    /// represents a valid color.</param>
    /// <returns>A Palette containing all valid colors found in the input array. If no valid colors are found or if the input
    /// is null, the returned Palette will be empty.</returns>
    public static Palette CreatePalette(int[,] mData)
       => new Palette().Create(mData);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oImageData"></param>
    /// <returns></returns>
    public static Palette CreatePalette(ImageData oImageData)
       => new Palette().Create(oImageData);
}