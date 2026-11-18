using ColourClashLib;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Imaging;

/// <summary>
/// Conversions between GDI+ images (<see cref="System.Drawing.Image"/>) and <see cref="ImageData"/> / color matrices.
/// </summary>
public static partial class ImageToolsGDI
{
    static readonly string sClass = nameof(ImageToolsGDI);

    #region Bitmap Creation

    /// <summary>
    /// Calculates the width of the specified image adjusted to the given pixel alignment mode.
    /// </summary>
    /// <param name="image">The image whose width is to be aligned. If null, a width of 0 is used.</param>
    /// <param name="pixelWidthAlign">The alignment mode that determines how the image width should be adjusted.</param>
    /// <returns>The width of the image, adjusted according to the specified alignment mode.</returns>
    static public int GetImageNewWidthAlign(Image image, WidthAlignMode pixelWidthAlign)
    {
        return MatrixTools.GetNewWidthAlign(image?.Width ?? 0, pixelWidthAlign);
    }

    /// <summary>
    /// Creates a new Bitmap from the specified data stream, ensuring the pixel format is 32 bits per pixel with
    /// alpha channel (Format32bppArgb).
    /// </summary>
    /// <remarks>The caller is responsible for disposing the returned Bitmap when it is no longer
    /// needed. If the input image is not already in Format32bppArgb, it will be converted to this format. The
    /// method returns null if the stream does not contain a valid image or if an error occurs during
    /// loading.</remarks>
    /// <param name="stream">The input stream containing image data. The stream must be readable and positioned at the beginning of the
    /// image data.</param>
    /// <returns>An Image object in Format32bppArgb pixel format if the image is successfully loaded; otherwise, null if the
    /// image cannot be created from the stream.</returns>
    public static System.Drawing.Image? GdiImageFromStream(Stream stream)
    {
        string sMethod = nameof(GdiImageFromStream);
        try
        {
            if( stream == null)
            {
                LogMan.Error(sClass, sMethod, "Stream is null");
                return null;
            }
            using (Bitmap bitmapSource = new Bitmap(stream))
            {
                return GdiImageToGdiImage32bpp(bitmapSource);
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates a new Bitmap from the specified file.
    /// </summary>
    /// <remarks>If the file cannot be opened or does not contain a valid image, this method returns
    /// null instead of throwing an exception. The caller should check the return value before using the
    /// Bitmap.</remarks>
    /// <param name="fileName">The path to the image file to open. The file must exist and be accessible for reading.</param>
    /// <returns>An Image object that represents the image contained in the specified file, or null if the file cannot be
    /// opened or the image cannot be loaded.</returns>
    public static System.Drawing.Image? GdiImageFromFile(string fileName)
    {
        string sMethod = nameof(GdiImageFromFile);
        try
        {
            if( string.IsNullOrWhiteSpace(fileName))
            {
                LogMan.Error(sClass, sMethod, "File name is null or empty");
                return null;
            }
            if( !File.Exists(fileName))
            {
                LogMan.Error(sClass, sMethod, $"File does not exist: {fileName}");
                return null;
            }
            using (var stream = File.OpenRead(fileName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return GdiImageFromStream(stream);
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    #endregion

    #region Bitmap Export

    //static string GetfileExt(ImageExportFormat format)
    //{
    //    switch (format)
    //    {
    //        case ImageExportFormat.BmpIndexed:
    //        case ImageExportFormat.Bmp:
    //            return ".bmp";
    //        case ImageExportFormat.PngIndexed:
    //        case ImageExportFormat.Png:
    //            return ".png";
    //        case ImageExportFormat.Jpg:
    //            return ".jpg";  
    //        case ImageExportFormat.CbmAmigaRawBitplane:
    //            return ".amibpl";
    //        case ImageExportFormat.CbmAmigaRawBitplaneCopperlist:
    //            return ".amicoplist";
    //        case ImageExportFormat.CbmAmigaRawBitplaneInterleaved:
    //            return ".amibpli";
    //        case ImageExportFormat.CbmAmigaRawBitplaneInterleavedCopperlist:
    //            return ".amicoplisti";
    //        default:
    //            return ".bin";
    //    }
    //}

    
    //public static Stream? GdiImageToStream(System.Drawing.Image image, ImageExportFormat format, int quality = 100)
    //{
    //    string sMethod = nameof(GdiImageToStream);
    //    try
    //    {
    //        if (image == null)
    //        {
    //            LogMan.Error(sClass, sMethod, "image is null");
    //            return null;
    //        }
    //        switch (format)
    //        {
    //            case ImageExportFormat.Bmp:
    //            case ImageExportFormat.Png:
    //            case ImageExportFormat.Jpg:
    //                {
    //                    using (var image32 = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
    //                    using (Graphics g = Graphics.FromImage(image32))
    //                    {
    //                        g.DrawImage(image, 0, 0, image.Width, image.Height);
    //                        MemoryStream stream = new MemoryStream();
    //                        switch (format)
    //                        {
    //                            case ImageExportFormat.Bmp:
    //                                image32.Save(stream, ImageFormat.Bmp);
    //                                return stream;
    //                            case ImageExportFormat.Png:
    //                                image32.Save(stream, ImageFormat.Png);
    //                                return stream;
    //                            case ImageExportFormat.Jpg:
    //                                image32.Save(stream, ImageFormat.Jpeg);
    //                                return stream;
    //                            default:
    //                                return null;
    //                        }
    //                    }
    //                }
    //            // TODO - Future support for indexed formats
    //            //case ImageExportFormat.PngIndexed:
    //            //case ImageExportFormat.BmpIndexed:
    //            //    {
    //            //        if (image.PixelFormat != PixelFormat.Format8bppIndexed)
    //            //        { 
    //            //        }
    //            //        return null;
    //            //    }
    //            default:
    //                LogMan.Error(sClass, sMethod, $"{format} not yet supported!");
    //                return null;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        LogMan.Exception(sClass, sMethod, ex);  
    //        return null;
    //    }
    //}

    //public static bool GdiImageToFile(System.Drawing.Image image, string fileName, ImageExportFormat format)
    //{
    //    string sMethod = nameof(GdiImageToFile);  
    //    try
    //    {
    //        var ms = GdiImageToStream(image, format);   
    //        if (ms == null)
    //        {
    //            LogMan.Error(sClass, sMethod, $"{nameof(GdiImageToStream)} returned null");
    //            return false;
    //        }
    //        using (var fs = File.Create(fileName + GetfileExt(format)))
    //        {
    //            ms.Seek(0, SeekOrigin.Begin);
    //            ms.CopyTo(fs);
    //            return true;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        LogMan.Exception(sClass, sMethod, ex);
    //        return false;
    //    }
    //}

    #endregion

    #region conversion Bitmap -> Matrix
    /// <summary>
    /// Reads an 8 bit indexed bitmap into a color matrix through its palette.
    /// </summary>
    /// <returns>The color matrix, or null on error.</returns>
    unsafe static int[,] ConvertFromBmp8BppIndex(System.Drawing.Image image)
    {
        string sM = nameof(ConvertFromBmp8BppIndex);
        try
        {
            if (!(image is Bitmap bmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            List<int> rgbList = new();
            foreach( var rgb in bmp.Palette.Entries)
            {
                rgbList.Add(ColorIntExtGdi.FromDrawingColor(rgb));
            }

            var matrix = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int rowOffset = bitmapData.Stride * y;
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        byte data = ptr[x + rowOffset];
                        var rgb = rgbList[data];
                        matrix[y, x] = rgb;                        
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sM, ex);
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }
            return matrix;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

    /// <summary>
    /// Reads a 24 bit (BGR) bitmap into a color matrix.
    /// </summary>
    /// <returns>The color matrix, or null on error.</returns>
    unsafe static int[,] ConvertFromBmp24Bpp(System.Drawing.Image image)
    {
        string sM = nameof(ConvertFromBmp24Bpp);
        try
        {
            if (!(image is Bitmap bmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            var matrix = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int rowOffset = bitmapData.Stride * y;
                    for (int x = 0,byteOffset=0; x < bmp.Width; x++,byteOffset+=3)
                    {
                        byte b = ptr[byteOffset + rowOffset + 0];
                        byte g = ptr[byteOffset + rowOffset + 1];
                        byte r = ptr[byteOffset + rowOffset + 2];
                        matrix[y, x] = ColorIntExt.FromRGB(r,g,b);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sM, ex);
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }
            return matrix;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }
    /// <summary>
    /// Reads a 32 bit (BGRA) bitmap into a color matrix; alpha is ignored.
    /// </summary>
    /// <returns>The color matrix, or null on error.</returns>
    unsafe static int[,] ConvertFromBmp32Bpp(System.Drawing.Image image)
    {
        string sM = nameof(ConvertFromBmp32Bpp);
        try
        {
            if (!(image is Bitmap bmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            var matrix = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int rowOffset = bitmapData.Stride * y;
                    for (int x = 0, byteOffset = 0; x < bmp.Width; x++,byteOffset+=4)
                    {
                        byte b = ptr[byteOffset + rowOffset + 0];
                        byte g = ptr[byteOffset + rowOffset + 1];
                        byte r = ptr[byteOffset + rowOffset + 2];
                        matrix[y, x] = ColorIntExt.FromRGB(r, g, b);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sM, ex);
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }
            return matrix;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

    /// <summary>
    /// Converts a GDI+ image (8 bpp indexed, 24 bpp or 32 bpp) into a color matrix.
    /// </summary>
    /// <returns>The color matrix, or null for unsupported formats or errors.</returns>
    static int[,] GdiImageToMatrixUnsafe(System.Drawing.Image image)
    {
        string sMethod = nameof(GdiImageToMatrixUnsafe);
        try
        {
            if( image == null)
            {
                LogMan.Error(sClass, sMethod, "image is null");
                return null;
            }

            bool canConvert =
              image.PixelFormat == PixelFormat.Format8bppIndexed ||
              image.PixelFormat == PixelFormat.Format24bppRgb ||
              image.PixelFormat == PixelFormat.Format32bppRgb ||
              image.PixelFormat == PixelFormat.Format32bppArgb;

            if (!canConvert)
            {
                LogMan.Error(sClass, sMethod, $"Pixel format {image.PixelFormat} not supported in {sMethod}");
                return null;
            }

            switch ( image.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return ConvertFromBmp8BppIndex(image);
                case PixelFormat.Format24bppRgb:
                    return ConvertFromBmp24Bpp(image);
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    return ConvertFromBmp32Bpp(image);
                default:
                    return null;
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    /// <summary>
    /// Converts a GDI+ image (8 bpp indexed, 24 bpp or 32 bpp) into a color matrix.
    /// </summary>
    /// <returns>The color matrix, or null on error.</returns>
    public static int[,] GdiImageToMatrix(System.Drawing.Image image)
        => GdiImageToMatrixUnsafe(image);

    /// <summary>
    /// Converts a GDI+ image into an <see cref="ImageData"/>.
    /// </summary>
    /// <returns>The image; empty when <paramref name="image"/> is null, null on error.</returns>
    public static ImageData? GdiImageToImageData(System.Drawing.Image image)
    {
        string sMethod = nameof(GdiImageToImageData);
        try
        {
            var imageData = new ImageData();
            if (image == null)
            {
                LogMan.Error(sClass, sMethod, "image is null");
                return imageData;
            }
            var data = GdiImageToMatrixUnsafe(image);
            return imageData.Create(data);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    #endregion

    #region Matrix -> Bitmap    

    /// <summary>
    /// Creates a 32 bpp bitmap from a color matrix; non-color values are drawn with the default background color.
    /// </summary>
    /// <returns>The bitmap (to be disposed by the caller), or null on error.</returns>
    unsafe static System.Drawing.Image? MatrixToGdiImageUnsafe(int[,] matrix)
    {
        string sMethod = nameof(MatrixToGdiImageUnsafe);
        if (matrix == null)
        {
            LogMan.Error(sClass, sMethod, "Matrix is null");
            return null;
        }
        try
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);
            var bmp = new Bitmap(columns, rows, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int rowOffset = bitmapData.Stride * y;
                    int* ptrRow = (int*)(bitmapData.Scan0 + rowOffset);
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        // Force opaque alpha; non-color values are drawn with the background color
                        ptrRow[x] = matrix[y, x].IsColor() ? (int)(0xFF_00_00_00 | matrix[y, x]) : ColorDefaults.DefaultBkgColorInt;
                    }
                }
                bmp.UnlockBits(bitmapData);
                return bmp;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                bmp.UnlockBits(bitmapData);
                bmp.Dispose();
                return null;
            }
        }
        catch (Exception exOuter)
        {
            LogMan.Exception(sClass, sMethod, exOuter);            
            return null;
        }
    }

    /// <summary>
    /// Creates a 32 bpp bitmap from a color matrix.
    /// </summary>
    /// <returns>The bitmap (to be disposed by the caller), or null on error.</returns>
    public static System.Drawing.Image? MatrixToGdiImage(int[,] matrix)
     => MatrixToGdiImageUnsafe(matrix);

    /// <summary>
    /// Creates a 32 bpp bitmap from an <see cref="ImageData"/>.
    /// </summary>
    /// <returns>The bitmap (to be disposed by the caller), or null on error.</returns>
    public static System.Drawing.Image? ImageDataToGdiImage(ImageData image)
     => MatrixToGdiImage(image?.GetMatrix());


    /// <summary>
    /// Redraws any GDI+ image as a 32 bpp RGB bitmap.
    /// </summary>
    /// <returns>The new bitmap, or null on error.</returns>
    public static System.Drawing.Image? GdiImageToGdiImage32bpp(System.Drawing.Image image)
    {
        string sMethod = nameof(GdiImageToGdiImage32bpp);
        try
        {
            if (image == null)
            {
                LogMan.Error(sClass, sMethod, "image is null");
                return null;
            }
            var imageDest = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                using (Graphics g = Graphics.FromImage(imageDest))
                {
                    g.DrawImage(image, 0, 0);
                }
            }
            return imageDest;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates an 8 bpp indexed bitmap from a color matrix, aligning the width as requested.
    /// <para/>
    /// Pixels are indexed on <paramref name="paletteSrc"/>, which also becomes the bitmap palette.
    /// </summary>
    /// <param name="indexMatrix">Color matrix to index</param>
    /// <param name="paletteSrc">Palette used for the indexing</param>
    /// <param name="alignMode">Width alignment</param>
    /// <returns>The bitmap, or null on error.</returns>
    unsafe static Bitmap? MatrixToGdiImageIndexedUnsafe(int[,] indexMatrix, List<int> paletteSrc, WidthAlignMode alignMode)
    {
        string sMethod = nameof(MatrixToGdiImageIndexedUnsafe);
        try
        {
            if (indexMatrix == null)
            {
                LogMan.Error(sClass, sMethod, "Matrix is null");
                return null;
            }
            if( paletteSrc == null)
            {
                LogMan.Error(sClass, sMethod, "Palette is null");
                return null;
            }
            // Index on the given palette, so that pixel indexes and bitmap palette entries match
            var indexData = MatrixTools.CreateIndexedMatrix(indexMatrix, paletteSrc, alignMode);
            int height = indexData.GetLength(0);
            int width = indexData.GetLength(1);

            var bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            var lockedData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    byte* rowPtr = (byte*)(lockedData.Scan0 + lockedData.Stride * y);
                    for (int x = 0; x < width; x++)
                    {
                        rowPtr[x] = indexData[y, x];
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                bmp.UnlockBits(lockedData);
                bmp.Dispose();
                return null;
            }
            bmp.UnlockBits(lockedData);
            SetGdiImageColorPalette(bmp, paletteSrc);
            return bmp;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates an 8 bpp indexed bitmap from a color matrix.
    /// </summary>
    public static Bitmap? MatrixToGdiImageIndexed(int[,] indexMatrix, List<int> paletteSrc, WidthAlignMode alignMode)
       => MatrixToGdiImageIndexedUnsafe(indexMatrix, paletteSrc, alignMode);

    /// <summary>
    /// Creates an 8 bpp indexed bitmap from a color matrix.
    /// </summary>
    public static Bitmap? MatrixToGdiImageIndexed(int[,] indexMatrix, Palette paletteSrc, WidthAlignMode alignMode)
        => MatrixToGdiImageIndexed(indexMatrix, paletteSrc?.ToList(), alignMode);

    /// <summary>
    /// Creates an 8 bpp indexed bitmap from an <see cref="ImageData"/>.
    /// </summary>
    public static Bitmap? ImageDataToGdiImageIndexed(ImageData image, WidthAlignMode alignMode)
        => MatrixToGdiImageIndexed(image?.GetMatrix(), image?.ColorPalette, alignMode);



    #endregion

    #region GDI Palette Helpers


    /// <summary>
    /// Copies <paramref name="paletteSrc"/> into the palette of an indexed bitmap; unused entries become black.
    /// </summary>
    /// <returns>False when the image is not a bitmap or the palette is null.</returns>
    static bool SetGdiImageColorPalette(System.Drawing.Image image, List<int> paletteSrc)
    {
        string sMethod = nameof(SetGdiImageColorPalette);
        if (!(image is Bitmap bmp))
        {
            LogMan.Error(sClass, sMethod, "Bitmap is null");
            return false;
        }
        if (paletteSrc == null )
        {
            LogMan.Error(sClass, sMethod, "paletteSrc is null");
            return false;
        }
        ColorPalette palette = bmp.Palette;
        for (int i = 0; i < bmp.Palette.Entries.Length; i++)
        {
            palette.Entries[i] = System.Drawing.Color.Black;
        }
        if (paletteSrc.Count > bmp.Palette.Entries.Length)
        {
            LogMan.Warning(sClass, sMethod, $"Source palette has more entries than target bitmap palette can hold; truncating to {bmp.Palette.Entries.Length} elements.");
        }
        for (int i = 0; i < Math.Min(bmp.Palette.Entries.Length, paletteSrc.Count); i++)
        {
            palette.Entries[i] = ColorIntExtGdi.ToDrawingColor(paletteSrc[i]);
        }
        bmp.Palette = palette;
        return true;
    }
  
  
    /// <summary>
    /// Creates a 256 entry GDI+ palette; unused entries get the default background color.
    /// </summary>
    /// <returns>The palette, or null when <paramref name="palette"/> is null or empty.</returns>
    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(List<int> palette)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (palette == null | palette.Count <= 0)
        {
            LogMan.Error(sClass, sMethod, "Palette list is null or empty");
            return null;
        }
        using (var bmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
        {
            var loop = Math.Min(palette.Count, bmp.Palette.Entries.Length);
            for (int i = 0; i < loop; i++)
            {
                bmp.Palette.Entries[i] = palette[i].ToDrawingColor();
            }
            for (int i = loop; i < bmp.Palette.Entries.Length; i++)
            {
                bmp.Palette.Entries[i] = ColorDefaults.DefaultBkgColorInt.ToDrawingColor();
            }
            return bmp.Palette;
        }
    }

    /// <summary>
    /// Creates a 256 level gray scale GDI+ palette.
    /// </summary>
    public static System.Drawing.Imaging.ColorPalette ToGdiGrayScalePalette()
    {
        string sMethod = nameof(ToGdiColorPalette);
        using (var bmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
        {
            for (int i = 0; i < bmp.Palette.Entries.Length; i++)
            {
                bmp.Palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            return bmp.Palette;
        }
    }

    /// <summary>
    /// Creates a 256 entry GDI+ palette from a <see cref="Palette"/>.
    /// </summary>
    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(ColourClashNet.Color.Palette palette)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (palette == null | palette.Count <= 0)
        {
            LogMan.Error(sClass, sMethod, "Color Palette is null or empty");
            return null;
        }
        return ToGdiColorPalette(palette?.ToList());
    }

    /// <summary>
    /// Creates a GDI+ palette with the distinct destination colors of a transformation map.
    /// </summary>
    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(Dictionary<int, int> transformationMap)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (transformationMap == null)
        {
            LogMan.Error(sClass, sMethod, "Palette Dictionary is null");
            return null;
        }
        List<int> colors = transformationMap.Where(x => x.Value >= 0).Select(x => x.Value).ToList().Distinct().ToList();
        return ToGdiColorPalette(colors);
    }


    #endregion
}