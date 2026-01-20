using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

public static partial class ImageTools
{
    static string sClass = nameof(ImageTools);

    #region Width Alignment

    /// <summary>
    /// Calculates the adjusted image width based on the specified pixel alignment mode.
    /// </summary>
    /// <remarks>Use this method to ensure that image widths conform to hardware or format
    /// requirements that mandate alignment to specific pixel multiples, such as 16, 32, or 64 pixels. If an
    /// unrecognized alignment mode is specified, the original width is returned without adjustment.</remarks>
    /// <param name="currentWidth">The original width of the image, in pixels. Must be greater than 0.</param>
    /// <param name="widthAlignMode">The alignment mode that determines how the width should be adjusted. Specifies the pixel boundary to which
    /// the width will be aligned.</param>
    /// <returns>The image width aligned to the specified pixel boundary. Returns 0 if the original width is less than or
    /// equal to 0.</returns>
    static public int GetImageNewWidthAlign(int currentWidth, ImageWidthAlignMode widthAlignMode)
    {
        if (currentWidth <= 0)
        {
            return 0;
        }
        switch (widthAlignMode)
        {
            case ImageWidthAlignMode.MultiplePixel16:
                return (currentWidth + 15) & (~0x0000000F);
            case ImageWidthAlignMode.MultiplePixel32:
                return (currentWidth + 31) & (~0x0000001F);
            case ImageWidthAlignMode.MultiplePixel64:
                return (currentWidth + 63) & (~0x0000003F);
            default:
                break;
        }
        return currentWidth;
    }

    
    /// <summary>
    /// Calculates the new width of an image, adjusted according to the specified pixel width alignment mode.
    /// </summary>
    /// <param name="oImage">The image data for which to calculate the aligned width. If null, a width of 0 is used.</param>
    /// <param name="widthAlignMode">The alignment mode that determines how the image width should be adjusted.</param>
    /// <returns>The width of the image, aligned according to the specified alignment mode.</returns>
    static public int GetImageNewWidthAlign(ImageData image, ImageWidthAlignMode widthAlignMode)
    {
        return GetImageNewWidthAlign(image?.Width ?? 0, widthAlignMode);
    }

    /// <summary>
    /// Returns the standard image width alignment mode used by the system.
    /// </summary>
    /// <returns>The default <see cref="ImageWidthAlignMode"/> value that specifies the standard alignment for image widths.</returns>
    static public ImageWidthAlignMode GetImageStdWidthAlign() => ImageWidthAlignMode.MultiplePixel16;

    #endregion

    #region Indexed Data Creation

    /// <summary>
    /// Creates a two-dimensional byte array representing indexed image data, mapping each color value in the source
    /// matrix to its corresponding palette index.
    /// </summary>
    /// <remarks>If a color value in the source matrix does not exist in the palette or the palette
    /// contains fewer than 256 colors, the corresponding output value is set to 255; otherwise, it is set to 0. The
    /// width of each row in the output array may be increased to satisfy the specified alignment mode.</remarks>
    /// <param name="rgbMatrix">A two-dimensional array containing the source color values to be indexed. Each element represents a color
    /// value to be mapped to the palette.</param>
    /// <param name="palette">A list of integer color values representing the palette. Each unique color in the palette is assigned an
    /// index used in the output array.</param>
    /// <param name="widthAlignMode">Specifies the alignment mode to use for the width of the output image data. Determines how the width of each
    /// row in the output array is aligned.</param>
    /// <returns>A two-dimensional byte array where each element contains the palette index corresponding to the color value
    /// in the source matrix. Returns null if the input data or palette is null, or if an error occurs.</returns>
    static byte[,]? CreateIndexedData(int[,] rgbMatrix, List<int> palette, ImageWidthAlignMode widthAlignMode)
    {
        string sMethod = nameof(CreateIndexedData);
        try
        {
            if (rgbMatrix == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return null;
            }
            if (palette == null)
            {
                LogMan.Error(sClass, sMethod, "Source palette list is null");
                return null;
            }
            int invalidColorIndex = palette.Count < 255 ? 255 : 0;

            int R = rgbMatrix.GetLength(0);
            int C = rgbMatrix.GetLength(1);
            int CO = GetImageNewWidthAlign(C, widthAlignMode);            

            var oRet = new byte[R, CO];
            for (int y = 0; y < R; y++)
            {
                for (int x = 0; x < C; x++)
                {
                    var col = rgbMatrix[y, x];
                    var idx = palette.IndexOf(col);
                    oRet[y, x] = (byte)(idx >= 0 && idx < 256 ? idx : invalidColorIndex);
                }
                // for (int x = C; x < CO; x++)
                // {
                //     oRet[y, x] = (Byte)invalidColorIndex;
                // }
            }
            return oRet;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates a two-dimensional byte array representing indexed image data from the specified source matrix and
    /// palette, using the given pixel width alignment mode.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing the source pixel data to be converted to indexed format.</param>
    /// <param name="palette">The palette to use for mapping source pixel values to palette indices. Cannot be null.</param>
    /// <param name="ePixelWidthAlign">Specifies how the width of each image row is aligned in the resulting indexed data.</param>
    /// <returns>A two-dimensional byte array containing the indexed image data, where each value corresponds to a palette
    /// index.</returns>
    static byte[,]? CreateIndexedData(int[,] rgbMatrix, Palette palette, ImageWidthAlignMode ePixelWidthAlign)
        => CreateIndexedData(rgbMatrix, palette.ToList(), ePixelWidthAlign);

    /// <summary>
    /// Converts a two-dimensional array of color values to an indexed byte array using the specified palette.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing color values to be indexed.</param>
    /// <param name="palette">The palette used to map color values to palette indices. Cannot be null.</param>
    /// <returns>A two-dimensional byte array where each element is the index of the corresponding color in the palette.</returns>
    static byte[,]? CreateIndexedData(int[,] rgbMatrix, Palette palette)
        => CreateIndexedData(rgbMatrix, palette?.ToList(), GetImageStdWidthAlign());

    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image, using the
    /// given pixel width alignment mode.
    /// </summary>
    /// <param name="oImage">The image data to convert to indexed format. Must not be null.</param>
    /// <param name="ePixelWidthAlign">The pixel width alignment mode to apply when generating the indexed data.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image, aligned according to the
    /// specified mode.</returns>
    static public byte[,]? CreateIndexedData(ImageData oImage, ImageWidthAlignMode ePixelWidthAlign)
        => CreateIndexedData(oImage?.DataX, oImage?.ColorPalette, ePixelWidthAlign);

    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image.
    /// </summary>
    /// <param name="oImage">The image data containing the pixel buffer and palette to convert to indexed format. Cannot be null.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image.</returns>
    static public byte[,]? CreateIndexedData(ImageData oImage)
        => CreateIndexedData(oImage?.DataX, oImage?.ColorPalette, GetImageStdWidthAlign());

    #endregion

 

}
