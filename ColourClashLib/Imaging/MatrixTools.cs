using ColourClashNet.Color;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

public static class MatrixTools
{
    static string sC = nameof(MatrixTools);

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
    static int GetNewWidthAlign<T>(T[,] matrixSrc, WidthAlignMode widthAlignMode)
    {
        string sM = nameof(GetNewWidthAlign);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        return GetNewWidthAlign(matrixSrc.GetLength(1), widthAlignMode);
    }

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


    #region Indexed Data Manipulation

    /// <summary>
    /// Creates a two-dimensional byte array representing indexed image data, mapping each Color value in the source
    /// matrix to its corresponding palette index.
    /// </summary>
    /// <remarks>If a Color value in the source matrix does not exist in the palette or the palette
    /// contains fewer than 256 Colors, the corresponding output value is set to 255; otherwise, it is set to 0. The
    /// width of each row in the output array may be increased to satisfy the specified alignment mode.</remarks>
    /// <param name="rgbMatrix">A two-dimensional array containing the source Color values to be indexed. Each element represents a Color
    /// value to be mapped to the palette.</param>
    /// <param name="palette">A list of integer Color values representing the palette. Each unique Color in the palette is assigned an
    /// index used in the output array.</param>
    /// <param name="widthAlignMode">Specifies the alignment mode to use for the width of the output image data. Determines how the width of each
    /// row in the output array is aligned.</param>
    /// <returns>A two-dimensional byte array where each element contains the palette index corresponding to the Color value
    /// in the source matrix. Returns null if the input data or palette is null, or if an error occurs.</returns>
    public static byte[,] CreateIndexedMatrix(int[,] rgbMatrix, List<int> paletteList, WidthAlignMode widthAlignMode)
    {
        string sM = nameof(CreateIndexedMatrix);

        if (rgbMatrix == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(rgbMatrix)} is null");

        if (paletteList == null)
        {
            LogMan.Warning(sC, sM, "Performace issue, no palette provided, rebuild from matrix");
            var palette = new Palette().Create(rgbMatrix);
            return CreateIndexedMatrix(rgbMatrix, palette.ToList(), widthAlignMode);
        }

        int R = rgbMatrix.GetLength(0);
        int C = rgbMatrix.GetLength(1);
        int CO = GetNewWidthAlign(C, widthAlignMode);

        Dictionary<int, byte> converter = new Dictionary<int, byte>();
        for (int i = 0; i < Math.Min(paletteList.Count, 256); i++)
        {
            converter[paletteList[i]] = (byte)i;
        }

        // FIX: era "< 255" — con palette da 255 colori sbagliava ramo
        byte invalidColorIndex = (byte)(paletteList.Count < 256 ? 255 : 0);

        var oRet = new byte[R, CO];
        for (int y = 0; y < R; y++)
        {
            for (int x = 0; x < C; x++)
            {
                var col = rgbMatrix[y, x];
                if (converter.TryGetValue(col, out var paletteIndex))
                    oRet[y, x] = paletteIndex;
                else
                    oRet[y, x] = invalidColorIndex;
            }

            for (int x = C; x < CO; x++)
            {
                oRet[y, x] = invalidColorIndex;
            }
        }
        return oRet;
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
    public static byte[,] CreateIndexedMatrix(int[,] rgbMatrix, Palette palette, WidthAlignMode ePixelWidthAlign)
        => CreateIndexedMatrix(rgbMatrix, palette?.ToList(), ePixelWidthAlign);

    /// <summary>
    /// Converts a two-dimensional array of Color values to an indexed byte array using the specified palette.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing Color values to be indexed.</param>
    /// <param name="palette">The palette used to map Color values to palette indices. Cannot be null.</param>
    /// <returns>A two-dimensional byte array where each element is the index of the corresponding Color in the palette.</returns>
    public static byte[,] CreateIndexedMatrix(int[,] rgbMatrix, Palette palette)
        => CreateIndexedMatrix(rgbMatrix, palette?.ToList(), WidthAlignMode.Multiple001);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="rgbMatrix"></param>
    /// <param name="fileName"></param>
    /// <param name="palette"></param>
    /// <param name="ePixelWidthAlign"></param>
    /// <returns></returns>
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, List<int> paletteList, WidthAlignMode ePixelWidthAlign)
    {
        string sM = nameof(SaveIndexedMatrixAsImage);

        if (rgbMatrix is null)
            throw new ArgumentNullException($"{sC}.{sM} : null {nameof(rgbMatrix)}");

        if (paletteList is null)
            throw new ArgumentNullException($"{sC}.{sM} : null {nameof(paletteList)}");

        if (paletteList.Count > 256)
            throw new ArgumentException($"{sC}.{sM} : too many colors for indexing ({paletteList.Count})");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException($"{sC}.{sM} : invalid filename ({paletteList.Count})");

        if (!fileName.ToLower().EndsWith(".bmp"))
            fileName += ".bmp";

        var idxMatrix = CreateIndexedMatrix(rgbMatrix, paletteList, ePixelWidthAlign);
        var width = idxMatrix.GetLength(1);
        var height = idxMatrix.GetLength(0);
        using (BinaryWriter bw = new BinaryWriter(File.Create(fileName)))
        {
            // Ogni riga deve essere allineata a 4 byte
            int rowSize = ((width + 3) / 4) * 4;
            int pixelDataSize = rowSize * height;

            int paletteSize = 256 * 4;

            int fileSize =
                14 +      // BITMAPFILEHEADER
                40 +      // BITMAPINFOHEADER
                paletteSize +
                pixelDataSize;

            //----------------------------------
            // BITMAPFILEHEADER (14 byte)
            //----------------------------------
            bw.Write((ushort)0x4D42);          // Signature "BM"
            bw.Write(fileSize);                // FileSize
            bw.Write((uint)0);                 // FIX: Reserved1+Reserved2 = 4 byte, non 8
            bw.Write(14 + 40 + paletteSize);   // FIX: offset basato sulla palette REALMENTE scritta (256*4), non su paletteList.Count

            //----------------------------------
            // BITMAPINFOHEADER (40 byte)
            //----------------------------------

            bw.Write(40);             // Header size
            bw.Write(width);          // Width
            bw.Write(height);         // Height
            bw.Write((ushort)1);      // Planes
            bw.Write((ushort)8);      // 8-bit indexed
            bw.Write(0);              // BI_RGB no compression
            bw.Write(pixelDataSize);  // Raw data size
            bw.Write(2835);           // X ppm
            bw.Write(2835);           // Y ppm
            bw.Write(256);            // Colors used
            bw.Write(0);              // Important colors

            //----------------------------------
            // Palette
            //----------------------------------

            for (int i = 0; i < paletteList.Count; i++)
            {
                bw.Write((byte)ColorIntExt.ToB(paletteList[i])); // Blue
                bw.Write((byte)ColorIntExt.ToG(paletteList[i])); // Green
                bw.Write((byte)ColorIntExt.ToR(paletteList[i])); // Red
                bw.Write((byte)255); // Reserved
            }
            for (int i = paletteList.Count; i < 256; i++)
            {
                bw.Write((byte)0); // Blue
                bw.Write((byte)0); // Green
                bw.Write((byte)0); // Red
                bw.Write((byte)0); // Reserved
            }
            //----------------------------------
            // Pixel data
            //----------------------------------

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    bw.Write((byte)idxMatrix[y, x]);
                }
                for (int x = width; x < rowSize; x++)
                {
                    bw.Write((byte)0);
                }
            }

            return true;
        }
    }
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, List<int>paletteList)
    => SaveIndexedMatrixAsImage(rgbMatrix, fileName, paletteList, WidthAlignMode.Multiple004);

    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, Palette palette, WidthAlignMode pixelWidthAlignment)
        => SaveIndexedMatrixAsImage(rgbMatrix, fileName, palette.ToList(), pixelWidthAlignment);

    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, Palette palette)
    => SaveIndexedMatrixAsImage(rgbMatrix, fileName, palette.ToList(),  WidthAlignMode.Multiple004);

    #endregion


    #region Matrix Manipulation

    /// <summary>
    /// Extracts a rectangular submatrix from the specified two-dimensional array.
    /// </summary>
    /// <remarks>If the specified crop area extends beyond the bounds of the source matrix, the method returns
    /// null. The returned array has the specified height and width, with elements copied from the corresponding region
    /// of the source matrix.</remarks>
    /// <param name="matrixSrc">The source two-dimensional array from which to crop a submatrix. Cannot be null.</param>
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
    static public int[,] Crop(int[,] matrixSrc, int xs, int ys, int width, int height)
    {
        string sM = nameof(Crop);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");

        int R = matrixSrc.GetLength(0);
        int C = matrixSrc.GetLength(1);
        if (ys < 0 || ys >= R || xs < 0 || xs >= C || width <= 0 || height <= 0 || ys + height > R || xs + width > C)
            throw new ArgumentOutOfRangeException($"{sC}.{sM} : Crop rectangle out of bounds");

        var oRet = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                oRet[y, x] = matrixSrc[ys + y, xs + x];
            }
        }
        return oRet;
    }

    /// <summary>
    /// Clears all elements in the specified two-dimensional integer array by setting them to zero.
    /// </summary>
    /// <remarks>If the array is null or an exception occurs during the operation, the method returns false
    /// and does not modify the array.</remarks>
    /// <param name="matrixDst">The two-dimensional array of integers to clear. Cannot be null.</param>
    /// <returns>true if the array was successfully cleared; otherwise, false.</returns>
    static public bool Clear(int[,] matrixDst)
    {
        string sM = nameof(Clear);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        Array.Clear(matrixDst, 0, matrixDst.Length);
        return true;
    }

    static Rectangle<int> GetRectangle(int[,] matrixSrc)
    {
        string sM = nameof(GetRectangle);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        return new(0, 0, matrixSrc.GetLength(1), matrixSrc.GetLength(0));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixDst"></param>
    /// <param name="xs"></param>
    /// <param name="ys"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fillRGB"></param>
    /// <returns></returns>
    static public bool Clear(int[,] matrixDst, int xs, int ys, int width, int height, int fillRGB = 0)
    {
        string sM = nameof(Clear);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        Rectangle<int> rectClipped = Rectangle<int>.Intersect(new(xs, ys, width, height), GetRectangle(matrixDst));
        if (rectClipped.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Clear rectangle is out of bounds");
            return false;
        }
        for (int r = 0; r < rectClipped.Height; r++)
        {
            for (int c = 0; c < rectClipped.Width; c++)
            {
                matrixDst[rectClipped.X + r, rectClipped.Y + c] = fillRGB;
            }
        }
        return true;
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
    static public bool Blit(int[,] matrixSrc, int[,] matrixDst, int xSrc, int ySrc, int xDst, int yDst, int width, int height)
    {
        string sM = nameof(Blit);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");

        Rectangle<int> rectSrc = Rectangle<int>.Intersect(new(xSrc, ySrc, width, height), GetRectangle(matrixSrc));
        Rectangle<int> rectDst = Rectangle<int>.Intersect(new(xDst, yDst, width, height), GetRectangle(matrixDst));
        if (rectSrc.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Source blit rectangle is out of bounds");
            return false;
        }
        if (rectDst.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Destination blit rectangle is out of bounds");
            return false;
        }
        int minHeight = Math.Min(rectSrc.Height, rectDst.Height);
        int minWidth = Math.Min(rectSrc.Width, rectDst.Width);
        //var rectSrcClip = new MatrixRectangle<int>(rectSrc.Row, rectSrc.Column, minHeight, minWidth);
        for (int y = 0; y < minHeight; y++)
        {
            for (int x = 0; x < minWidth; x++)
            {
                matrixDst[rectDst.Y + y, rectDst.X + x] = matrixSrc[rectSrc.Y + y, rectSrc.X + x];
            }
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixSrc"></param>
    /// <param name="matrixDst"></param>
    /// <param name="rectangleSource"></param>
    /// <param name="rowDest"></param>
    /// <param name="columnDest"></param>
    /// <returns></returns>
    static public void Blit(int[,] matrixSrc, int[,] matrixDst, Rectangle<int> rectangleSource, int xDst, int yDst)
       => Blit(matrixSrc, matrixDst, rectangleSource.X, rectangleSource.Y, xDst, yDst, rectangleSource.Width, rectangleSource.Height);



    /// <summary>
    /// 
    /// </summary>
    /// <param name="srcMatrix"></param>
    /// <param name="bkgRGB"></param>
    /// <param name="invalidRGB"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    static public int[,] CreateSafeMatrix(int[,]? srcMatrix, int bkgRGB, int invalidRGB)
    {
        string sM = nameof(DoubleMatrixColumns);
        if (srcMatrix == null)
            throw new ArgumentNullException($"{sC}.{sM} : Null matrix source");
        var R = srcMatrix.GetLength(0);
        var C = srcMatrix.GetLength(1);
        var dstMatrix = new int[R, C];
        for (int r = 0; r < R; r++)
        {
            for (int c = 0; c < C; c++)
            {
                int rgb = srcMatrix[r, c];
                switch (ColorIntExt.GetColorInfo(rgb))
                {
                    case ColorInfo.IsColor:
                        dstMatrix[r, c] = rgb;
                        break;
                    case ColorInfo.IsTransparent:
                    case ColorInfo.IsMask:
                    case ColorInfo.IsAplha:
                        dstMatrix[r, c] = bkgRGB;
                        break;
                    case ColorInfo.Invalid:
                        dstMatrix[r, c] = invalidRGB;
                        break;
                    default:
                        dstMatrix[r, c] = bkgRGB;
                        break;
                }
            }
        }
        return dstMatrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="srcMatrix"></param>
    /// <returns></returns>
    static public int[,] CreateSafeMatrix(int[,]? srcMatrix)
        => CreateSafeMatrix(srcMatrix, ColorDefaults.DefaultInvalidColorInt, ColorDefaults.DefaultBkgColorInt);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixSrc"></param>
    /// <param name="keepEvenColumns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    static public int[,] HalveMatrixColumns(int[,]? matrixSrc, HalveResolutionMode halveResolutionMode)
    {
        string sM = nameof(HalveMatrixColumns);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        var R = matrixSrc.GetLength(0);
        var C = matrixSrc.GetLength(1);
        var CO = (C + 1) / 2;
        var oRet = new int[R, CO];
        switch (halveResolutionMode)
        {
            case HalveResolutionMode.OddPixel:
                for (int r = 0; r < R; r++)
                    for (int c = 0, co = 0; c < C; c += 2, co++)
                    {
                        oRet[r, co] = matrixSrc[r, c];
                    }
                break;
            case HalveResolutionMode.EvenPixel:
                for (int r = 0; r < R; r++)
                    for (int c = 1, co = 0; c < C; c += 2, co++)
                    {
                        oRet[r, co] = matrixSrc[r, c];
                    }
                break;
            case HalveResolutionMode.MeanColor:
                for (int r = 0; r < R; r++)
                    for (int c = 1, co = 0; c < C; c += 2, co++)
                    {
                        oRet[r, co] = matrixSrc[r, c];
                        var a = matrixSrc[r, c];
                        var b = a;
                        if (c < C - 1)
                        {
                            b = matrixSrc[r, c + 1];
                        }
                        oRet[r, co] = ColorIntExt.GetColorMean(a, b);
                    }
                break;
            default: throw new ArgumentException($"{sC}.{sM} : unsupported halve mode : '{halveResolutionMode}'", nameof(HalveResolutionMode));
        }
        return oRet;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrixSrc"></param>
    /// <returns></returns>
    public static int[,] DoubleMatrixColumns(int[,] matrixSrc)
    {
        string sM = nameof(DoubleMatrixColumns);

        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");

        var R = matrixSrc.GetLength(0);
        var C = matrixSrc.GetLength(1);
        var oRet = new int[R, C * 2];

        //Parallel.For(0, R, r =>
        for (int r = 0; r < R; r++)
        {
            for (int c = 0, co = 0; c < C; c++)
            {
                var a = matrixSrc[r, c];
                oRet[r, co++] = a;
                oRet[r, co++] = a;
            }
        }//);
        return oRet;
    }

    #endregion
}