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
    /// <param name="r">The zero-based row index of the upper-left corner of the crop area within the source matrix. Must be within the
    /// bounds of the matrix.</param>
    /// <param name="c">The zero-based column index of the upper-left corner of the crop area within the source matrix. Must be within
    /// the bounds of the matrix.</param>
    /// <param name="width">The width, in columns, of the submatrix to extract. Must be greater than zero and the crop area must not exceed
    /// the bounds of the source matrix.</param>
    /// <param name="height">The height, in rows, of the submatrix to extract. Must be greater than zero and the crop area must not exceed
    /// the bounds of the source matrix.</param>
    /// <returns>A new two-dimensional array containing the cropped submatrix, or null if the source matrix is null or the
    /// specified crop area is out of bounds.</returns>
    static public int[,]? Crop(int[,] matrix, int r, int c, int width, int height)
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
            if (r < 0 || r >= R || c < 0 || c >= C || width <= 0 || height <= 0 || r + height > R || c + width > C)
            {
                LogMan.Error(sClass, sMethod, "Crop parameters are out of bounds");
                return null;
            }
            var oRet = new int[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    oRet[y, x] = matrix[r + y, c + x];
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="rowStart"></param>
    /// <param name="columnStart"></param>
    /// <param name="rowLenght"></param>
    /// <param name="columnLenght"></param>
    /// <param name="fillRGB"></param>
    /// <returns></returns>
    static public bool Clear(int[,] matrix, int rowStart, int columnStart, int rowLenght, int columnLenght, int fillRGB = 0)
    {
        string sMethod = nameof(Clear);
        try
        {
            if (matrix == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return false;
            }
            MatrixRectangle<int> rectClipped = MatrixRectangle<int>.Intestect(new(rowStart, columnStart, rowLenght, columnLenght), new(0, 0, matrix.GetLength(0), matrix.GetLength(1)));
            if (rectClipped.IsEmpty)
            {
                LogMan.Error(sClass, sMethod, "Clear rectangle is out of bounds");
                return false;
            }
            for (int r = 0; r < rectClipped.RowsLenght; r++)
            {
                for (int c = 0; c < rectClipped.ColumnsLenght; c++)
                {
                    matrix[rectClipped.RS + r, rectClipped.CS + c] = fillRGB;
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
    /// <param name="rs">The starting row index in the source matrix for the region to copy.</param>
    /// <param name="cs">The starting column index in the source matrix for the region to copy.</param>
    /// <param name="re">The starting row index in the destination matrix where the region will be placed.</param>
    /// <param name="ce">The starting column index in the destination matrix where the region will be placed.</param>
    /// <param name="columnLenght">The width, in elements, of the region to copy.</param>
    /// <param name="rowLenght">The height, in elements, of the region to copy.</param>
    /// <returns>true if the region was successfully copied; otherwise, false.</returns>
    static public bool Blit(int[,] matrixSrc, int[,] matrixDst, int rs, int cs, int re, int ce, int rowLenght, int columnLenght)
    {
        string sMethod = nameof(Crop);
        try
        {
            if (matrixSrc == null || matrixDst == null)
            {
                LogMan.Error(sClass, sMethod, "Source data matrix is null");
                return false;
            }
            ImageRectangle<int> rectSrc = ImageRectangle<int>.Intestect(new(0, 0, matrixSrc.GetLength(1), matrixSrc.GetLength(0)), new(cs, rs, columnLenght, rowLenght));
            ImageRectangle<int> rectDst = ImageRectangle<int>.Intestect(new(0, 0, matrixDst.GetLength(1), matrixDst.GetLength(0)), new(ce, re, columnLenght, rowLenght));
            ImageRectangle<int> rectSrcClipped = ImageRectangle<int>.Intestect(rectSrc, rectDst);
            if (rectSrcClipped.IsEmpty)
            {
                LogMan.Error(sClass, sMethod, "Blit rectangles do not overlap");
                return false;
            }
            for (int y = 0; y < rectSrcClipped.Height; y++)
            {
                for (int x = 0; x < rectSrcClipped.Width; x++)
                {
                    matrixDst[rectDst.YS + y, rectDst.XS + x] = matrixSrc[rectSrc.YS + y, rectSrc.XS + x];
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
    static public bool Blit(int[,] matrixSrc, int[,] matrixDst, MatrixRectangle<int> rectangleSource, int rowDest, int columnDest)
       => Blit(matrixSrc, matrixDst, rectangleSource.Row, rectangleSource.Column, rowDest, columnDest, rectangleSource.RowsLenght, rectangleSource.ColumnsLenght);

}