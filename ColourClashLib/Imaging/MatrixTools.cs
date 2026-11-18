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

/// <summary>
/// Low level helpers on color matrices (<c>int[rows, columns]</c>): width alignment, palette indexing,
/// indexed BMP export, cropping, blitting, cookie-cut masking and horizontal resolution changes.
/// </summary>
public static class MatrixTools
{
    static string sC = nameof(MatrixTools);

    #region Width Alignment

    /// <summary>
    /// Rounds <paramref name="currentWidth"/> up to the next multiple required by <paramref name="widthAlignMode"/>.
    /// </summary>
    /// <param name="currentWidth">Original width in pixels</param>
    /// <param name="widthAlignMode">Required alignment</param>
    /// <returns>The aligned width; 0 if <paramref name="currentWidth"/> is not positive.</returns>
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
    /// Returns the aligned width of a matrix.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="matrixSrc"/> is null.</exception>
    static int GetNewWidthAlign<T>(T[,] matrixSrc, WidthAlignMode widthAlignMode)
    {
        string sM = nameof(GetNewWidthAlign);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        return GetNewWidthAlign(matrixSrc.GetLength(1), widthAlignMode);
    }

    /// <summary>
    /// Returns the aligned width of a color matrix.
    /// </summary>
    static public int GetNewWidthAlign(int[,] matrix, WidthAlignMode widthAlignMode)
        => GetNewWidthAlign<int>(matrix, widthAlignMode);

    /// <summary>
    /// Returns the aligned width of an index matrix.
    /// </summary>
    static public int GetNewWidthAlign(byte[,] matrix, WidthAlignMode widthAlignMode)
        => GetNewWidthAlign<byte>(matrix, widthAlignMode);

    #endregion

    #region base

    /// <summary>
    /// Creates a <paramref name="width"/> x <paramref name="height"/> matrix filled with <paramref name="fillRGB"/>.
    /// </summary>
    static public int[,] CreateMatrix(int width, int height, int fillRGB)
    {
        var matrix = new int[height, width];
        for(int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                matrix[r, c] = fillRGB;
            }
        }
        return matrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>

    /// <summary>
    /// Creates a <paramref name="width"/> x <paramref name="height"/> matrix filled with black.
    /// </summary>
    static public int[,] CreateMatrix(int width, int height) => CreateMatrix(width, height, 0);

    #endregion


        #region Indexed Data Manipulation

    /// <summary>
    /// Maps every color of <paramref name="rgbMatrix"/> to its index in <paramref name="paletteList"/>.
    /// <para/>
    /// Colors missing from the palette, and the padding columns added by the alignment, get index 255
    /// (or 0 when the palette already uses all 256 entries). Only the first 256 palette colors are indexed.
    /// </summary>
    /// <param name="rgbMatrix">Source color matrix</param>
    /// <param name="paletteList">Palette; when null it is rebuilt from the matrix (slow)</param>
    /// <param name="widthAlignMode">Alignment of the output width</param>
    /// <returns>Index matrix [rows, aligned columns].</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rgbMatrix"/> is null.</exception>
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

        int rows = rgbMatrix.GetLength(0);
        int columns = rgbMatrix.GetLength(1);
        int alignedColumns = GetNewWidthAlign(columns, widthAlignMode);

        Dictionary<int, byte> converter = new Dictionary<int, byte>();
        for (int i = 0; i < Math.Min(paletteList.Count, 256); i++)
        {
            converter[paletteList[i]] = (byte)i;
        }

        // Use 255 for unmapped colors unless the palette needs that index itself
        byte invalidColorIndex = (byte)(paletteList.Count < 256 ? 255 : 0);

        var indexMatrix = new byte[rows, alignedColumns];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var color = rgbMatrix[y, x];
                if (converter.TryGetValue(color, out var paletteIndex))
                    indexMatrix[y, x] = paletteIndex;
                else
                    indexMatrix[y, x] = invalidColorIndex;
            }

            for (int x = columns; x < alignedColumns; x++)
            {
                indexMatrix[y, x] = invalidColorIndex;
            }
        }
        return indexMatrix;
    }

    /// <summary>
    /// Creates a two-dimensional byte array representing indexed image data from the specified source matrix and
    /// palette, using the given pixel width alignment mode.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing the source pixel data to be converted to indexed format.</param>
    /// <param name="palette">The palette to use for mapping source pixel values to palette indices. Cannot be null.</param>
    /// <param name="pixelWidthAlign">Specifies how the width of each image row is aligned in the resulting indexed data.</param>
    /// <returns>A two-dimensional byte array containing the indexed image data, where each value corresponds to a palette
    /// index.</returns>
    public static byte[,] CreateIndexedMatrix(int[,] rgbMatrix, Palette palette, WidthAlignMode pixelWidthAlign)
        => CreateIndexedMatrix(rgbMatrix, palette?.ToList(), pixelWidthAlign);

    /// <summary>
    /// Converts a two-dimensional array of Color values to an indexed byte array using the specified palette.
    /// </summary>
    /// <param name="rgbMatrix">A two-dimensional array of integers representing Color values to be indexed.</param>
    /// <param name="palette">The palette used to map Color values to palette indices. Cannot be null.</param>
    /// <returns>A two-dimensional byte array where each element is the index of the corresponding Color in the palette.</returns>
    public static byte[,] CreateIndexedMatrix(int[,] rgbMatrix, Palette palette)
        => CreateIndexedMatrix(rgbMatrix, palette?.ToList(), WidthAlignMode.Multiple001);


    /// <summary>
    /// Writes a color matrix as an uncompressed 8 bit indexed BMP file.
    /// <para/>
    /// The ".bmp" extension is appended when missing. A full 256 entry palette is always written;
    /// rows are stored bottom-up and padded to 4 bytes, as required by the BMP format.
    /// </summary>
    /// <param name="rgbMatrix">Source color matrix</param>
    /// <param name="fileName">Destination file</param>
    /// <param name="paletteList">Palette (max 256 colors)</param>
    /// <param name="pixelWidthAlign">Alignment of the image width</param>
    /// <returns>True when the file has been written.</returns>
    /// <exception cref="ArgumentNullException">Null matrix or palette.</exception>
    /// <exception cref="ArgumentException">Too many colors or invalid file name.</exception>
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, List<int> paletteList, WidthAlignMode pixelWidthAlign)
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

        var indexMatrix = CreateIndexedMatrix(rgbMatrix, paletteList, pixelWidthAlign);
        var width = indexMatrix.GetLength(1);
        var height = indexMatrix.GetLength(0);
        using (BinaryWriter writer = new BinaryWriter(File.Create(fileName)))
        {
            // Each pixel row must be aligned to 4 bytes
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
            writer.Write((ushort)0x4D42);          // Signature "BM"
            writer.Write(fileSize);                // FileSize
            writer.Write((uint)0);                 // Reserved1 + Reserved2 (4 bytes in total)
            writer.Write(14 + 40 + paletteSize);   // Pixel data offset: based on the palette actually written (256*4), not on paletteList.Count

            //----------------------------------
            // BITMAPINFOHEADER (40 byte)
            //----------------------------------

            writer.Write(40);             // Header size
            writer.Write(width);          // Width
            writer.Write(height);         // Height
            writer.Write((ushort)1);      // Planes
            writer.Write((ushort)8);      // 8-bit indexed
            writer.Write(0);              // BI_RGB no compression
            writer.Write(pixelDataSize);  // Raw data size
            writer.Write(2835);           // X ppm
            writer.Write(2835);           // Y ppm
            writer.Write(256);            // Colors used
            writer.Write(0);              // Important colors

            //----------------------------------
            // Palette
            //----------------------------------

            for (int i = 0; i < paletteList.Count; i++)
            {
                writer.Write((byte)ColorIntExt.ToB(paletteList[i])); // Blue
                writer.Write((byte)ColorIntExt.ToG(paletteList[i])); // Green
                writer.Write((byte)ColorIntExt.ToR(paletteList[i])); // Red
                writer.Write((byte)255); // Reserved
            }
            for (int i = paletteList.Count; i < 256; i++)
            {
                writer.Write((byte)0); // Blue
                writer.Write((byte)0); // Green
                writer.Write((byte)0); // Red
                writer.Write((byte)0); // Reserved
            }
            //----------------------------------
            // Pixel data (bottom-up rows)
            //----------------------------------

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write((byte)indexMatrix[y, x]);
                }
                for (int x = width; x < rowSize; x++)
                {
                    writer.Write((byte)0);
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Writes an 8 bit indexed BMP with the width aligned to 4 pixels.
    /// </summary>
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, List<int>paletteList)
    => SaveIndexedMatrixAsImage(rgbMatrix, fileName, paletteList, WidthAlignMode.Multiple004);

    /// <summary>
    /// Writes an 8 bit indexed BMP using a <see cref="Palette"/>.
    /// </summary>
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, Palette palette, WidthAlignMode pixelWidthAlignment)
        => SaveIndexedMatrixAsImage(rgbMatrix, fileName, palette.ToList(), pixelWidthAlignment);

    /// <summary>
    /// Writes an 8 bit indexed BMP using a <see cref="Palette"/>, with the width aligned to 4 pixels.
    /// </summary>
    public static bool SaveIndexedMatrixAsImage(int[,] rgbMatrix, string fileName, Palette palette)
    => SaveIndexedMatrixAsImage(rgbMatrix, fileName, palette.ToList(),  WidthAlignMode.Multiple004);

    #endregion


    #region Matrix Manipulation

    /// <summary>
    /// Copies a rectangular area of <paramref name="matrixSrc"/> into a new matrix.
    /// </summary>
    /// <param name="matrixSrc">Source matrix</param>
    /// <param name="xs">Left coordinate</param>
    /// <param name="ys">Top coordinate</param>
    /// <param name="width">Area width</param>
    /// <param name="height">Area height</param>
    /// <returns>A new <paramref name="height"/> x <paramref name="width"/> matrix.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="matrixSrc"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The area is not fully inside the source matrix.</exception>
    static public int[,] Crop(int[,] matrixSrc, int xs, int ys, int width, int height)
    {
        string sM = nameof(Crop);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");

        int rows = matrixSrc.GetLength(0);
        int columns = matrixSrc.GetLength(1);
        if (ys < 0 || ys >= rows || xs < 0 || xs >= columns || width <= 0 || height <= 0 || ys + height > rows || xs + width > columns)
            throw new ArgumentOutOfRangeException($"{sC}.{sM} : Crop rectangle out of bounds");

        var result = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = matrixSrc[ys + y, xs + x];
            }
        }
        return result;
    }

    /// <summary>
    /// Sets every element of <paramref name="matrixDst"/> to zero (black).
    /// </summary>
    /// <returns>Always true.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="matrixDst"/> is null.</exception>
    static public bool Clear(int[,] matrixDst)
    {
        string sM = nameof(Clear);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        Array.Clear(matrixDst, 0, matrixDst.Length);
        return true;
    }

    /// <summary>
    /// Returns the bounds of a matrix as a rectangle at (0,0).
    /// </summary>
    static Rectangle<int> GetRectangle(int[,] matrixSrc)
    {
        string sM = nameof(GetRectangle);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        return new(0, 0, matrixSrc.GetLength(1), matrixSrc.GetLength(0));
    }

    /// <summary>
    /// Fills a rectangular area with <paramref name="fillRGB"/>; the area is clipped to the matrix bounds.
    /// </summary>
    /// <returns>False when the area is completely outside the matrix.</returns>
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
                matrixDst[rectClipped.Y + r, rectClipped.X + c] = fillRGB;
            }
        }
        return true;
    }

    /// <summary>
    /// Copies a rectangular area from <paramref name="matrixSrc"/> to <paramref name="matrixDst"/>.
    /// <para/>
    /// Both areas are clipped to their matrix bounds; only the common part is copied.
    /// </summary>
    /// <param name="matrixSrc">Source matrix</param>
    /// <param name="matrixDst">Destination matrix</param>
    /// <param name="xSrc">Source left coordinate</param>
    /// <param name="ySrc">Source top coordinate</param>
    /// <param name="xDst">Destination left coordinate</param>
    /// <param name="yDst">Destination top coordinate</param>
    /// <param name="width">Area width</param>
    /// <param name="height">Area height</param>
    /// <returns>False when the source or destination area is completely out of bounds.</returns>
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
    /// Copies a rectangular area of an image into <paramref name="matrixDst"/>.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="matrixDst"/> is the image's own matrix.</exception>
    static public bool Blit(ImageData imageSrc, int[,] matrixDst, int xSrc, int ySrc, int xDst, int yDst, int width, int height)
    {
        string sM = nameof(Blit);
        ImageData.AssertValid(imageSrc);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        if(imageSrc.matrix == matrixDst)
            throw new InvalidDataException( $"{sC}.{sM} : cannot blit inside the same ImageData");
        return Blit(imageSrc.matrix, matrixDst, xSrc, ySrc, xDst, yDst, width, height);
    }

    /// <summary>
    /// Copies <paramref name="rectangleSource"/> from <paramref name="matrixSrc"/> to (<paramref name="xDst"/>, <paramref name="yDst"/>).
    /// </summary>
    static public void Blit(int[,] matrixSrc, int[,] matrixDst, Rectangle<int> rectangleSource, int xDst, int yDst)
       => Blit(matrixSrc, matrixDst, rectangleSource.X, rectangleSource.Y, xDst, yDst, rectangleSource.Width, rectangleSource.Height);

    /// <summary>
    /// Copies <paramref name="rectangleSource"/> of an image to (<paramref name="xDst"/>, <paramref name="yDst"/>).
    /// </summary>
    static public void Blit(ImageData imageSrc, int[,] matrixDst, Rectangle<int> rectangleSource, int xDst, int yDst)
       => Blit(imageSrc, matrixDst, rectangleSource.X, rectangleSource.Y, xDst, yDst, rectangleSource.Width, rectangleSource.Height);


    /// <summary>
    /// Returns a copy of the matrix that contains only displayable colors.
    /// <para/>
    /// Transparent, mask and alpha values become <paramref name="bkgRGB"/>, invalid values become <paramref name="invalidRGB"/>.
    /// </summary>
    /// <param name="srcMatrix">Source matrix</param>
    /// <param name="bkgRGB">Replacement for transparent and tagged values</param>
    /// <param name="invalidRGB">Replacement for invalid values</param>
    /// <exception cref="ArgumentNullException"><paramref name="srcMatrix"/> is null.</exception>
    static public int[,] CreateSafeMatrix(int[,]? srcMatrix, int bkgRGB, int invalidRGB)
    {
        string sM = nameof(CreateSafeMatrix);
        if (srcMatrix == null)
            throw new ArgumentNullException($"{sC}.{sM} : Null matrix source");
        var rows = srcMatrix.GetLength(0);
        var columns = srcMatrix.GetLength(1);
        var dstMatrix = new int[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
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
    /// Copies the pixels of <paramref name="matrixSrc"/> into <paramref name="matrixDst"/> only where the mask is set.
    /// <para/>
    /// A mask pixel is set when it is a real color different from <see cref="ColorDefaults.DefaultShapeBkgColorInt"/>.
    /// All three areas are clipped to their matrix bounds.
    /// </summary>
    /// <param name="matrixSrc">Source matrix</param>
    /// <param name="matrixMsk">Mask matrix</param>
    /// <param name="matrixDst">Destination matrix</param>
    /// <param name="xSrc">Source left coordinate</param>
    /// <param name="ySrc">Source top coordinate</param>
    /// <param name="xMsk">Mask left coordinate</param>
    /// <param name="yMsk">Mask top coordinate</param>
    /// <param name="xDst">Destination left coordinate</param>
    /// <param name="yDst">Destination top coordinate</param>
    /// <param name="width">Area width</param>
    /// <param name="height">Area height</param>
    /// <returns>False when any area is completely out of bounds.</returns>
    static public bool CoockieCut(int[,] matrixSrc, int[,] matrixMsk, int[,] matrixDst, int xSrc, int ySrc, int xMsk, int yMsk, int xDst, int yDst, int width, int height)
    {
        string sM = nameof(CoockieCut);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        if (matrixMsk == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixMsk)} is null");
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");

        Rectangle<int> rectSrc = Rectangle<int>.Intersect(new(xSrc, ySrc, width, height), GetRectangle(matrixSrc));
        Rectangle<int> rectMsk = Rectangle<int>.Intersect(new(xMsk, yMsk, width, height), GetRectangle(matrixMsk));
        Rectangle<int> rectDst = Rectangle<int>.Intersect(new(xDst, yDst, width, height), GetRectangle(matrixDst));

        if (rectSrc.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Source blit rectangle is out of bounds");
            return false;
        }
        if (rectMsk.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Mask blit rectangle is out of bounds");
            return false;
        }
        if (rectDst.IsEmpty)
        {
            LogMan.Warning(sC, sM, "Destination blit rectangle is out of bounds");
            return false;
        }
        int minHeight = Math.Min(rectMsk.Height,Math.Min(rectSrc.Height, rectDst.Height));
        int minWidth = Math.Min(rectMsk.Width,Math.Min(rectSrc.Width, rectDst.Width));
        //var rectSrcClip = new MatrixRectangle<int>(rectSrc.Row, rectSrc.Column, minHeight, minWidth);
        for (int y = 0; y < minHeight; y++)
        {
            for (int x = 0; x < minWidth; x++)
            {
                var maskColor = matrixMsk[rectMsk.Y + y, rectMsk.X + x];
                if (ColorIntExt.IsColor(maskColor) && maskColor != ColorDefaults.DefaultShapeBkgColorInt)
                {
                    matrixDst[rectDst.Y + y, rectDst.X + x] = matrixSrc[rectSrc.Y + y, rectSrc.X + x];
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Cookie-cut copy from an image using a mask matrix.
    /// </summary>
    static public bool CoockieCut(ImageData imageSrc, int[,] matrixMsk, int[,] matrixDst, int xSrc, int ySrc, int xMsk, int yMsk, int xDst, int yDst, int width, int height)
    {
        string sM = nameof(CoockieCut);
        ImageData.AssertValid(imageSrc);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        if (imageSrc.matrix == matrixDst)
            throw new InvalidDataException($"{sC}.{sM} : cannot blit inside the same ImageData");
        return CoockieCut(imageSrc.matrix, matrixMsk, matrixDst, xSrc, ySrc, xMsk,yMsk, xDst, yDst, width, height);
    }

    /// <summary>
    /// Cookie-cut copy from an image using a mask image.
    /// </summary>
    static public bool CoockieCut(ImageData imageSrc, ImageData imageMsk, int[,] matrixDst, int xSrc, int ySrc, int xMsk, int yMsk, int xDst, int yDst, int width, int height)
    {
        string sM = nameof(CoockieCut);
        ImageData.AssertValid(imageSrc);
        ImageData.AssertValid(imageMsk);
        if (matrixDst == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixDst)} is null");
        if (imageSrc.matrix == matrixDst)
            throw new InvalidDataException($"{sC}.{sM} : cannot blit inside the same ImageData");
        return CoockieCut(imageSrc.matrix, imageMsk.matrix, matrixDst, xSrc, ySrc, xMsk, yMsk, xDst, yDst, width, height);
    }



    /// <summary>
    /// Returns a displayable copy of the matrix using the default background and invalid colors.
    /// </summary>
    static public int[,] CreateSafeMatrix(int[,]? srcMatrix)
        => CreateSafeMatrix(srcMatrix, ColorDefaults.DefaultBkgColorInt, ColorDefaults.DefaultInvalidColorInt);

    /// <summary>
    /// Halves the number of columns, merging each pair of adjacent pixels according to <paramref name="halveResolutionMode"/>.
    /// </summary>
    /// <param name="matrixSrc">Source matrix</param>
    /// <param name="halveResolutionMode">Pixel merge strategy</param>
    /// <returns>A matrix with (columns + 1) / 2 columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="matrixSrc"/> is null.</exception>
    static public int[,] HalveMatrixColumns(int[,]? matrixSrc, HalveResolutionMode halveResolutionMode)
    {
        string sM = nameof(HalveMatrixColumns);
        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");
        var rows = matrixSrc.GetLength(0);
        var columns = matrixSrc.GetLength(1);
        var packedColumns = (columns + 1) / 2;
        var result = new int[rows, packedColumns];
        switch (halveResolutionMode)
        {
            case HalveResolutionMode.OddPixel:
                for (int r = 0; r < rows; r++)
                    for (int c = 0, columnOut = 0; c < columns; c += 2, columnOut++)
                    {
                        result[r, columnOut] = matrixSrc[r, c];
                    }
                break;
            case HalveResolutionMode.EvenPixel:
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 1, columnOut = 0; c < columns; c += 2, columnOut++)
                    {
                        result[r, columnOut] = matrixSrc[r, c];
                    }
                    // Odd width: the last output column has no even source pixel, use the last one
                    if (columns % 2 == 1)
                        result[r, packedColumns - 1] = matrixSrc[r, columns - 1];
                }
                break;
            case HalveResolutionMode.MeanColor:
                for (int r = 0; r < rows; r++)
                    // Average pixel pairs (0,1), (2,3), ...
                    for (int c = 0, columnOut = 0; c < columns; c += 2, columnOut++)
                    {
                        var a = matrixSrc[r, c];
                        var b = a;
                        if (c < columns - 1)
                        {
                            b = matrixSrc[r, c + 1];
                        }
                        result[r, columnOut] = ColorIntExt.GetColorMean(a, b);
                    }
                break;
            default: throw new ArgumentException($"{sC}.{sM} : unsupported halve mode : '{halveResolutionMode}'", nameof(HalveResolutionMode));
        }
        return result;
    }

    /// <summary>
    /// Doubles the number of columns by duplicating every pixel horizontally.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="matrixSrc"/> is null.</exception>
    public static int[,] DoubleMatrixColumns(int[,] matrixSrc)
    {
        string sM = nameof(DoubleMatrixColumns);

        if (matrixSrc == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrixSrc)} is null");

        var rows = matrixSrc.GetLength(0);
        var columns = matrixSrc.GetLength(1);
        var result = new int[rows, columns * 2];

        //Parallel.For(0, rows, r =>
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0, columnOut = 0; c < columns; c++)
            {
                var a = matrixSrc[r, c];
                result[r, columnOut++] = a;
                result[r, columnOut++] = a;
            }
        }//);
        return result;
    }


    #endregion
}   