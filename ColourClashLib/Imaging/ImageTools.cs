using ColourClashLib;
using ColourClashNet.Color;
using ColourClashNet.Log;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

/// <summary>
/// High level image helpers: indexed conversion, resolution changes, load and save (SkiaSharp based).
/// </summary>
public static partial class ImageTools
{
    static string sC = nameof(ImageTools);

    #region Indexed Data Creation

    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image, using the
    /// given pixel width alignment mode.
    /// </summary>
    /// <param name="image">The image data to convert to indexed format. Must not be null.</param>
    /// <param name="pixelWidthAlignment">The pixel width alignment mode to apply when generating the indexed data.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image, aligned according to the
    /// specified mode.</returns>
    static public byte[,] CreateIndexedMatrix(ImageData image, WidthAlignMode pixelWidthAlignment)
    {
        ImageData.AssertValid(image);
        return MatrixTools.CreateIndexedMatrix(image.matrix, image.ColorPalette, pixelWidthAlignment);
    }
    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image.
    /// </summary>
    /// <param name="image">The image data containing the pixel buffer and palette to convert to indexed format. Cannot be null.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image.</returns>
    static public byte[,] CreateIndexedMatrix(ImageData image)
    {
        ImageData.AssertValid(image);
        return MatrixTools.CreateIndexedMatrix(image.matrix, image.ColorPalette);
    }

    /// <summary>
    /// Doubles the horizontal resolution by duplicating every column.
    /// </summary>
    /// <param name="image">Source image</param>
    /// <returns>A new image twice as wide.</returns>
    /// <exception cref="ArgumentNullException">The image is invalid.</exception>
    public static ImageData DoubleXResolution(ImageData image)
    {
        ImageData.AssertValid(image);
        var matrix = MatrixTools.DoubleMatrixColumns(image.matrix);
        return new ImageData().Create(matrix);
    }

    /// <summary>
    /// Doubles the horizontal resolution and crops the result to <paramref name="width"/> columns.
    /// <para/>
    /// Undoes <see cref="HalveXResolution"/> on odd widths, where halving rounds up and doubling would add a column.
    /// </summary>
    /// <param name="image">Half resolution image</param>
    /// <param name="width">Width of the original image</param>
    /// <returns>A new image <paramref name="width"/> columns wide (or twice as wide, if smaller).</returns>
    public static ImageData DoubleXResolution(ImageData image, int width)
    {
        ImageData.AssertValid(image);
        var doubled = MatrixTools.DoubleMatrixColumns(image.matrix);
        int rows = doubled.GetLength(0);
        int columns = Math.Min(width, doubled.GetLength(1));
        if (columns == doubled.GetLength(1))
            return new ImageData().Create(doubled);
        var cropped = new int[rows, columns];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                cropped[r, c] = doubled[r, c];
        return new ImageData().Create(cropped);
    }

    /// <summary>
    /// Halves the horizontal resolution by merging pairs of adjacent columns.
    /// </summary>
    /// <param name="image">Source image</param>
    /// <param name="halveMode">How each pair of pixels is merged</param>
    /// <returns>A new image half as wide.</returns>
    public static ImageData HalveXResolution(ImageData image, HalveResolutionMode halveMode)
    {
        ImageData.AssertValid(image);
        var matrix = MatrixTools.HalveMatrixColumns(image.matrix, halveMode);
        return new ImageData().Create(matrix);
    }


    /// <summary>
    /// Saves the image as an 8 bit indexed BMP, aligning the width as requested.
    /// </summary>
    /// <returns>True on success.</returns>
    public static bool SaveImageAsIndexedBitmap(ImageData image,  string fileName, WidthAlignMode pixelWidthAlignment)
        =>MatrixTools.SaveIndexedMatrixAsImage(image.matrix, fileName, image.ColorPalette, pixelWidthAlignment);

    /// <summary>
    /// Saves the image as an 8 bit indexed BMP.
    /// </summary>
    /// <returns>True on success.</returns>
    public static bool SaveImageAsIndexedBitmap(ImageData image, string fileName)
        => MatrixTools.SaveIndexedMatrixAsImage(image.matrix, fileName, image.ColorPalette);

    /// <summary>
    /// Replaces the extension of <paramref name="fileName"/> with the one of <paramref name="saveFormat"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Unsupported format.</exception>
    public static string SetFileNameExtension(string fileName, ImageSaveFormat saveFormat)
    {
        string ext = saveFormat switch
        {
            ImageSaveFormat.Png => ".png",
            ImageSaveFormat.Bmp => ".bmp",
            ImageSaveFormat.Jpg => ".jpg",
            _ => throw new ArgumentException($"{saveFormat} not supported")
        };
        string newFileName = Path.ChangeExtension(fileName, ext);
        return newFileName;
    }   

    /// <summary>
    /// Saves the image as PNG, BMP or JPG. The file extension is replaced to match the format.
    /// </summary>
    /// <param name="image">Image to save</param>
    /// <param name="fileName">Destination file name</param>
    /// <param name="saveFormat">File format</param>
    /// <param name="quality">Encoder quality [0..100] (JPG only)</param>
    /// <returns>True on success; errors are logged.</returns>
    public static bool SaveImage(ImageData image, string fileName, ImageSaveFormat saveFormat, int quality=80)
    {
        string sM = nameof(SaveImage);
        try
        {
            ImageData.AssertValid(image);

            int rows = image.Rows;
            int cols = image.Columns;

            SkiaSharp.SKBitmap skBitmap = new(cols, rows, SKColorType.Bgra8888, SKAlphaType.Opaque);

            Span<byte> pixels = skBitmap.GetPixelSpan();

            long expectedBytes = (long)rows * cols * sizeof(int);
            if (pixels.Length != expectedBytes)
                throw new InvalidOperationException(
                    $"Pixel buffer mismatch: bitmap has {pixels.Length} bytes, matrix needs {expectedBytes}.");

            // image.matrix is int[,]: AsSpan() is not available on multidimensional arrays.
            // Get a byte reference to the start of the buffer and build a Span<byte> of the
            // exact byte size (rows*cols*sizeof(int)). A packed 0x00RRGGBB int in little endian
            // is laid out as B,G,R,0, i.e. BGRA8888 with the alpha ignored (opaque bitmap).
            ref byte srcRef = ref MemoryMarshal.GetArrayDataReference(image.matrix);
            Span<byte> srcBytes = MemoryMarshal.CreateSpan(ref srcRef, (int)expectedBytes);

            srcBytes.CopyTo(pixels);

            SKEncodedImageFormat format =  SKEncodedImageFormat.Png;
            switch (saveFormat)
            {
                case ImageSaveFormat.Png:
                    format = SKEncodedImageFormat.Png;                    
                    break;
                case ImageSaveFormat.Bmp:
                    format = SKEncodedImageFormat.Bmp;
                    break;
                case ImageSaveFormat.Jpg:
                    format = SKEncodedImageFormat.Jpeg;
                    break;
                default: throw new ArgumentException($"{saveFormat} not supported");
            }
            var newFileName = SetFileNameExtension(fileName, saveFormat);
            using SKData encoded = skBitmap.Encode(format, quality);
            using FileStream stream = File.Create(newFileName);
            encoded.SaveTo(stream);
            LogMan.Debug(sC, sM, $"saved {cols}x{rows} image as {saveFormat} : {newFileName}");
            return true;
        }
        catch (Exception e)
        {
            LogMan.Exception(sC, sM, $"cannot save {fileName} as {saveFormat}", e);
            return false;
        }
    }

    /// <summary>
    /// Saves the image as PNG.
    /// </summary>
    public static bool SaveImage(ImageData image, string fileName)
        => SaveImage(image, fileName, ImageSaveFormat.Png);

    /// <summary>
    /// Loads an image file (any format supported by SkiaSharp) into an <see cref="ImageData"/>.
    /// <para/>
    /// The alpha channel is discarded and odd widths are padded with one black column,
    /// because several retro video modes work on pixel pairs.
    /// </summary>
    /// <param name="fileName">Image file path</param>
    /// <returns>The loaded image, or null on error (errors are logged).</returns>
    public static ImageData LoadImage(string fileName)
    {
        string sM = nameof(LoadImage);
        try
        {
            LogMan.Debug(sC, sM, $"loading {fileName}");
            using SKBitmap original = SKBitmap.Decode(fileName);
            if (original == null)
            {
                LogMan.Error(sC, sM, $"unable to decode {fileName} (missing file or unsupported format)");
                return null;
            }
            LogMan.Trace(sC, sM, $"decoded {original.Width}x{original.Height}, {original.ColorType}, {original.AlphaType}");

            // Force the BGRA8888 Unpremul format, whatever the source format
            // (e.g. JPEG decoded as RGB, PNG with alpha, etc.)
            SKImageInfo targetInfo = new(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            using SKBitmap skBitmap = original.ColorType == targetInfo.ColorType && original.AlphaType == targetInfo.AlphaType
                ? original.Copy()
                : original.Copy(SKColorType.Bgra8888); // handles the color type conversion

            // If Copy(ColorType) does not normalize the alpha (still Premul), explicitly
            // redraw the bitmap in the target format.
            SKBitmap normalized = skBitmap;
            if (skBitmap.AlphaType != SKAlphaType.Unpremul)
            {
                normalized = new SKBitmap(targetInfo);
                using (SKCanvas canvas = new(normalized))
                {
                    canvas.DrawBitmap(skBitmap, 0, 0);
                }
            }

            int rows = normalized.Height;
            int cols = normalized.Width;

            Span<byte> srcBytes = normalized.GetPixelSpan();

            long expectedBytes = (long)rows * cols * sizeof(int);
            if (srcBytes.Length != expectedBytes)
            {
                LogMan.Error(sC, sM, $"pixel buffer mismatch loading {fileName} : bitmap has {srcBytes.Length} bytes, expected {expectedBytes}");
                return null;
            }

            int[,] matrix = new int[rows, cols];

            // Same technique used in SaveImage, in the opposite direction:
            // copy the bitmap bytes (BGRA) directly into the int[,] matrix.
            ref byte dstRef = ref MemoryMarshal.GetArrayDataReference(matrix);
            Span<byte> dstBytes = MemoryMarshal.CreateSpan(ref dstRef, (int)expectedBytes);

            srcBytes.CopyTo(dstBytes);

            if (!ReferenceEquals(normalized, skBitmap))
                normalized.Dispose();

            // Drop the alpha byte: the upper byte of a packed color is reserved for ColorInfo flags
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    matrix[r, c] = matrix[r, c] & 0x00FFFFFF;
                }
            }

            if (cols % 2 == 0)
            {
                return new ImageData().Create(matrix);
            }
            else
            {
                // Pad odd widths to an even number of columns
                LogMan.Debug(sC, sM, $"odd width {cols} padded to {cols + 1} columns");
                var matrixPadded = new int[rows, cols + 1];
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        matrixPadded[r, c] = matrix[r, c];
                    }
                }
                return new ImageData().Create(matrixPadded);
            }
        }
        catch (Exception e)
        {
            LogMan.Exception(sC, sM, $"cannot load {fileName}", e);
            return null;
        }
    }

    #endregion
}