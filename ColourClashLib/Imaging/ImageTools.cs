using ColourClashLib;
using ColourClashNet.Color;
using ColourClashNet.Log;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

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
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ImageData DoubleXResolution(ImageData image)
    {
        ImageData.AssertValid(image);
        var matrix = MatrixTools.DoubleColumnResolution(image.matrix);
        return new ImageData().Create(matrix);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static ImageData HalveXResolution(ImageData image)
    {
        ImageData.AssertValid(image);
        var matrix = MatrixTools.HalveColumnResolution(image.matrix);
        return new ImageData().Create(matrix);
    }

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

            // image.matrix è int[,]: niente AsSpan() diretto sui multidim array.
            // Otteniamo un ref byte all'inizio del buffer e costruiamo uno Span<byte>
            // della dimensione esatta in byte (rows*cols*sizeof(int)).
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
//            using SKImage skImage = SKImage.FromBitmap(skBitmap);
            using SKData encoded = skBitmap.Encode(format, quality);
            using FileStream stream = File.Create(fileName);
            encoded.SaveTo(stream);
            return true;
        }
        catch (Exception e)
        {
            LogMan.Exception(sC, sM, e);
            return false;
        }
    }

    public static ImageData LoadImage(string fileName)
    {
        string sM = nameof(LoadImage);
        try
        {
            using SKBitmap original = SKBitmap.Decode(fileName);
            if (original == null)
            {
                LogMan.Exception(sC, sM, new FileLoadException($"Impossibile decodificare il file: {fileName}"));
                return null;
            }

            // Forziamo il formato BGRA8888 Unpremul, indipendentemente dal formato
            // sorgente (es. JPEG decodificato in RGB, PNG con alpha, ecc.)
            SKImageInfo targetInfo = new(original.Width, original.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            using SKBitmap skBitmap = original.ColorType == targetInfo.ColorType && original.AlphaType == targetInfo.AlphaType
                ? original.Copy()
                : original.Copy(SKColorType.Bgra8888); // gestisce conversione colore

            // Se Copy(ColorType) non normalizza l'alpha (rimane Premul), ricostruiamo
            // esplicitamente il bitmap nel formato desiderato.
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
                LogMan.Exception(sC, sM, new InvalidOperationException(
                    $"Pixel buffer mismatch: bitmap ha {srcBytes.Length} byte, attesi {expectedBytes}."));
                return null;
            }

            int[,] matrix = new int[rows, cols];

            // Stessa tecnica usata in SaveImage, ma in direzione opposta:
            // copiamo i byte del bitmap (BGRA) direttamente nella matrice int[,].
            ref byte dstRef = ref MemoryMarshal.GetArrayDataReference(matrix);
            Span<byte> dstBytes = MemoryMarshal.CreateSpan(ref dstRef, (int)expectedBytes);

            srcBytes.CopyTo(dstBytes);

            if (!ReferenceEquals(normalized, skBitmap))
                normalized.Dispose();

            return new ImageData().Create(matrix);
        }
        catch (Exception e)
        {
            LogMan.Exception(sC, sM, e);
            return null;
        }
    }

    #endregion
}