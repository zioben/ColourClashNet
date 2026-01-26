using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

public static class MatrixTools
{
    static string sClass = nameof(MatrixTools);

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
    static public int GetNewWidthAlign(int currentWidth, WidthAlignMode widthAlignMode)
    {
        if (currentWidth <= 0)
        {
            return 0;
        }
        switch (widthAlignMode)
        {
            case WidthAlignMode.Multiple001:
                return currentWidth;
            case WidthAlignMode.Multiple002:
                return (currentWidth + 1) & (~0x0000001);
            case WidthAlignMode.Multiple004:
                return (currentWidth + 3) & (~0x0000003);
            case WidthAlignMode.Multiple008:
                return (currentWidth + 7) & (~0x0000007);
            case WidthAlignMode.Multiple016:
                return (currentWidth + 15) & (~0x000000F);
            case WidthAlignMode.Multiple032:
                return (currentWidth + 31) & (~0x000001F);
            case WidthAlignMode.Multiple064:
                return (currentWidth + 63) & (~0x000003F);
            case WidthAlignMode.Multiple128:
                return (currentWidth + 127) & (~0x000007F);
            case WidthAlignMode.Multiple256:
                return (currentWidth + 255) & (~0x00000FF);
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
    static int GetNewWidthAlign<T>(T[,] matrix, WidthAlignMode widthAlignMode)
        => GetNewWidthAlign(matrix?.GetLength(1) ?? 0, widthAlignMode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="widthAlignMode"></param>
    /// <returns></returns>
    static public int GetNewWidthAlign(int[,] matrix, WidthAlignMode widthAlignMode)
        => GetNewWidthAlign<int>(matrix, widthAlignMode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="widthAlignMode"></param>
    /// <returns></returns>
    static public int GetNewWidthAlign(byte[,] matrix, WidthAlignMode widthAlignMode)
        => GetNewWidthAlign<byte>(matrix, widthAlignMode);

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
    public static byte[,]? CreateIndexedMatrix(int[,] rgbMatrix, List<int> palette, WidthAlignMode widthAlignMode)
    {
        string sMethod = nameof(CreateIndexedMatrix);
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
            int CO = GetNewWidthAlign(C, widthAlignMode);

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
    public static byte[,]? CreateIndexedMatrix(int[,] rgbMatrix, Palette palette, WidthAlignMode ePixelWidthAlign)
        => CreateIndexedMatrix(rgbMatrix, palette.ToList(), ePixelWidthAlign);

    /// <summary>
    /// Converts a two-dimensional array of color values to an indexed byte array using the specified palette.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing color values to be indexed.</param>
    /// <param name="palette">The palette used to map color values to palette indices. Cannot be null.</param>
    /// <returns>A two-dimensional byte array where each element is the index of the corresponding color in the palette.</returns>
    public static byte[,]? CreateIndexedMatrix(int[,] rgbMatrix, Palette palette)
        => CreateIndexedMatrix(rgbMatrix, palette?.ToList(), WidthAlignMode.Multiple001);


    /// <summary>
    /// Extracts a rectangular submatrix from the specified two-dimensional array.
    /// </summary>
    /// <remarks>If the specified crop area extends beyond the bounds of the source matrix, the method returns
    /// null. The returned array has the specified height and width, with elements copied from the corresponding region
    /// of the source matrix.</remarks>
    /// <param name="matrix">The source two-dimensional array from which to crop a submatrix. Cannot be null.</param>
    /// <param name="xs">The zero-based column index of the upper-left corner of the crop area within the source matrix. Must be within
    /// the bounds of the matrix.</param>
    /// <param name="ys">The zero-based row index of the upper-left corner of the crop area within the source matrix. Must be within the
    /// bounds of the matrix.</param>
    /// <param name="width">The width, in columns, of the submatrix to extract. Must be greater than zero and the crop area must not exceed
    /// the bounds of the source matrix.</param>
    /// <param name="height">The height, in rows, of the submatrix to extract. Must be greater than zero and the crop area must not exceed
    /// the bounds of the source matrix.</param>
    /// <returns>A new two-dimensional array containing the cropped submatrix, or null if the source matrix is null or the
    /// specified crop area is out of bounds.</returns>
    static public int[,]? Crop(int[,] matrix, int xs, int ys,  int width, int height)
    {
        string sMethod = nameof(Crop);
        try
        {
            if (matrix == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return null;
            }
            int R = matrix.GetLength(0);
            int C = matrix.GetLength(1);
            if (ys < 0 || ys >= R || xs < 0 || xs >= C || width <= 0 || height <= 0 || ys + height > R || xs + width > C)
            {
                LogMan.Error(sClass, sMethod, "Crop parameters are out of bounds");
                return null;
            }
            var oRet = new int[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    oRet[y, x] = matrix[ys + y, xs + x];
                }
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
    /// Clears all elements in the specified two-dimensional integer array by setting them to zero.
    /// </summary>
    /// <remarks>If the array is null or an exception occurs during the operation, the method returns false
    /// and does not modify the array.</remarks>
    /// <param name="matrix">The two-dimensional array of integers to clear. Cannot be null.</param>
    /// <returns>true if the array was successfully cleared; otherwise, false.</returns>
    static public bool Clear(int[,] matrix)
    {
        string sMethod = nameof(Clear);
        try
        {
            if (matrix == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return false;
            }
            Array.Clear(matrix, 0, matrix.Length);
            return true;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return false;
        }
    }

    static Rectangle<int>GetRectangle(int[,] matrix)
        => new(0, 0, matrix?.GetLength(1)??0, matrix?.GetLength(0)??0);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="xs"></param>
    /// <param name="ys"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fillRGB"></param>
    /// <returns></returns>
    static public bool Clear(int[,] matrix, int xs, int ys, int width, int height, int fillRGB = 0)
    {
        string sMethod = nameof(Clear);
        try
        {
            if (matrix == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return false;
            }
            Rectangle<int> rectClipped = Rectangle<int>.Intersect(new(xs, ys, width, height), GetRectangle(matrix));
            if (rectClipped.IsEmpty)
            {
                LogMan.Error(sClass, sMethod, "Clear rectangle is out of bounds");
                return false;
            }
            for (int r = 0; r < rectClipped.Height; r++)
            {
                for (int c = 0; c < rectClipped.Width; c++)
                {
                    matrix[rectClipped.X + r, rectClipped.Y + c] = fillRGB;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return false;
        }
    }

    /// <summary>
    /// Copies a rectangular region of elements from the source matrix to the destination matrix.
    /// </summary>
    /// <remarks>If the specified regions in the source or destination matrices do not overlap or are out of
    /// bounds, no elements are copied and the method returns false. Only the overlapping portion of the specified
    /// regions is copied.</remarks>
    /// <param name="matrixSrc">The two-dimensional source matrix from which elements are copied. Cannot be null.</param>
    /// <param name="matrixDst">The two-dimensional destination matrix to which elements are copied. Cannot be null.</param>
    /// <param name="xSrc">The starting column index in the source matrix for the region to copy.</param>
    /// <param name="ySrc">The starting row index in the source matrix for the region to copy.</param>
    /// <param name="xDst">The starting column index in the destination matrix where the region will be placed.</param>
    /// <param name="yDst">The starting row index in the destination matrix where the region will be placed.</param>
    /// <param name="columnLenght">The width, in elements, of the region to copy.</param>
    /// <param name="rowLenght">The height, in elements, of the region to copy.</param>
    /// <returns>true if the region was successfully copied; otherwise, false.</returns>
    static public bool Blit(int[,] matrixSrc, int[,] matrixDst, int xSrc, int ySrc, int xDst, int yDst,  int width, int height)
    {
        string sMethod = nameof(Crop);
        try
        {
            if (matrixSrc == null || matrixDst == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return false;
            }
            Rectangle<int> rectSrc = Rectangle<int>.Intersect(new(xSrc, ySrc, width,height),GetRectangle(matrixSrc));
            Rectangle<int> rectDst = Rectangle<int>.Intersect(new(xDst, yDst, width, height),GetRectangle(matrixDst));
            if ( rectSrc.IsEmpty)
            {
                LogMan.Warning(sClass, sMethod, "Source blit rectangle is out of bounds");
                return false;
            }
            if (rectDst.IsEmpty)
            {
                LogMan.Warning(sClass, sMethod, "Destination blit rectangle is out of bounds");
                return false;
            }
            int minHeight = Math.Min(rectSrc.Height, rectDst.Height);
            int minWidth = Math.Min(rectSrc.Width, rectDst.Width);
            //var rectSrcClip = new MatrixRectangle<int>(rectSrc.Row, rectSrc.Column, minHeight, minWidth);
            for (int y = 0; y <minHeight; y++)
            {
                for (int x = 0; x < minWidth; x++)
                {
                    matrixDst[rectDst.Y + y, rectDst.X + x] = matrixSrc[rectSrc.Y + y, rectSrc.X + x];
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return false;
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixSrc"></param>
    /// <param name="matrixDst"></param>
    /// <param name="rectangleSource"></param>
    /// <param name="rowDest"></param>
    /// <param name="columnDest"></param>
    /// <returns></returns>
    static public bool Blit(int[,] matrixSrc, int[,] matrixDst, Rectangle<int> rectangleSource, int xDst, int yDst)
       => Blit(matrixSrc, matrixDst, rectangleSource.X, rectangleSource.Y, xDst, yDst, rectangleSource.Width,rectangleSource.Height);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    static public int[,] HalveColumnResolution(int[,]? matrix)
    {
        if (matrix == null)
            return null;
        var R = matrix.GetLength(0);
        var C = matrix.GetLength(1);
        var CO = (C + 1) / 2;
        var oRet = new int[R, CO];
        Parallel.For(0, R, r =>
        {
            for (int c = 0, co = 0; c < C; c += 2, co++)
            {
                if (c < C - 1)
                {
                    var a = matrix[r, c];
                    var b = matrix[r, c + 1];
                    oRet[r, co] = ColorIntExt.GetColorMean(a, a);
                }
            }
        });
        return oRet;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static int[,] DoubleColumnResolution(int[,]? matrix)
    {
        if (matrix == null)
            return null;
        var R = matrix.GetLength(0);
        var C = matrix.GetLength(1);
        var oRet = new int[R, C * 2];

        Parallel.For(0, R, r =>
        {
            for (int c = 0, co = 0; c < C; c++)
            {
                var a = matrix[r, c];
                oRet[r, co++] = a;
                oRet[r, co++] = a;
            }
        });
        return oRet;
    }

}