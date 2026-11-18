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

    //static string GetfileExt(ImageExportFormat eFormat)
    //{
    //    switch (eFormat)
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

    
    //public static Stream? GdiImageToStream(System.Drawing.Image image, ImageExportFormat eFormat, int iQuality = 100)
    //{
    //    string sMethod = nameof(GdiImageToStream);
    //    try
    //    {
    //        if (image == null)
    //        {
    //            LogMan.Error(sClass, sMethod, "image is null");
    //            return null;
    //        }
    //        switch (eFormat)
    //        {
    //            case ImageExportFormat.Bmp:
    //            case ImageExportFormat.Png:
    //            case ImageExportFormat.Jpg:
    //                {
    //                    using (var oImage2 = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
    //                    using (Graphics g = Graphics.FromImage(oImage2))
    //                    {
    //                        g.DrawImage(image, 0, 0, image.Width, image.Height);
    //                        MemoryStream stream = new MemoryStream();
    //                        switch (eFormat)
    //                        {
    //                            case ImageExportFormat.Bmp:
    //                                oImage2.Save(stream, ImageFormat.Bmp);
    //                                return stream;
    //                            case ImageExportFormat.Png:
    //                                oImage2.Save(stream, ImageFormat.Png);
    //                                return stream;
    //                            case ImageExportFormat.Jpg:
    //                                oImage2.Save(stream, ImageFormat.Jpeg);
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
    //                LogMan.Error(sClass, sMethod, $"{eFormat} not yet supported!");
    //                return null;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        LogMan.Exception(sClass, sMethod, ex);  
    //        return null;
    //    }
    //}

    //public static bool GdiImageToFile(System.Drawing.Image image, string fileName, ImageExportFormat eFormat)
    //{
    //    string sMethod = nameof(GdiImageToFile);  
    //    try
    //    {
    //        var ms = GdiImageToStream(image, eFormat);   
    //        if (ms == null)
    //        {
    //            LogMan.Error(sClass, sMethod, $"{nameof(GdiImageToStream)} returned null");
    //            return false;
    //        }
    //        using (var fs = File.Create(fileName + GetfileExt(eFormat)))
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

            var m = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int yoff = bitmapData.Stride * y;
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        byte data = ptr[x + yoff];
                        var rgb = rgbList[data];
                        m[y, x] = rgb;                        
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
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

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

            var m = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int yoff = bitmapData.Stride * y;
                    for (int x = 0,xx=0; x < bmp.Width; x++,xx+=3)
                    {
                        byte b = ptr[xx + yoff + 0];
                        byte g = ptr[xx + yoff + 1];
                        byte r = ptr[xx + yoff + 2];
                        m[y, x] = ColorIntExt.FromRGB(r,g,b);
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
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }
    unsafe static int[,] ConvertFromBmp32Bpp(System.Drawing.Image image)
    {
        string sM = nameof(ConvertFromBmp24Bpp);
        try
        {
            if (!(image is Bitmap bmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            var m = new int[bmp.Height, bmp.Width];

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int yoff = bitmapData.Stride * y;
                    for (int x = 0, xx = 0; x < bmp.Width; x++,xx+=4)
                    {
                        byte b = ptr[xx + yoff + 0];
                        byte g = ptr[xx + yoff + 1];
                        byte r = ptr[xx + yoff + 2];
                        m[y, x] = ColorIntExt.FromRGB(r, g, b);
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
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

    static int[,] GdiImageToMatrixUnsafe(System.Drawing.Image image)
    {
        string sMethod = nameof(GdiImageToMatrixUnsafe);
        try
        {
            if( image == null)
            {
                LogMan.Error(sClass, sMethod, "oImage is null");
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

            var m = new int[image.Height, image.Width];

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

    public static int[,] GdiImageToMatrix(System.Drawing.Image image)
        => GdiImageToMatrixUnsafe(image);

    public static ImageData? GdiImageToImageData(System.Drawing.Image image)
    {
        string sMethod = nameof(GdiImageToImageData);
        try
        {
            var img = new ImageData();
            if (image == null)
            {
                LogMan.Error(sClass, sMethod, "oImage is null");
                return img;
            }
            var data = GdiImageToMatrixUnsafe(image);
            return img.Create(data);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    #endregion

    #region Matrix -> Bitmap    

    unsafe static System.Drawing.Image? MatrixToGdiImageUnsafe(int[,] m)
    {
        string sMethod = nameof(MatrixToGdiImageUnsafe);
        if (m == null)
        {
            LogMan.Error(sClass, sMethod, "Matrix is null");
            return null;
        }
        try
        {
            var rows = m.GetLength(0);
            var columns = m.GetLength(1);
            var bmp = new Bitmap(columns, rows, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                //byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                for (int y = 0; y < bmp.Height; y++)
                {
                    int yoff = bitmapData.Stride * y;
                    int* ptrRow = (int*)(bitmapData.Scan0 + yoff);
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        ptrRow[x] = m[y, x].IsColor() ? (int)(0xFF_00_00_00 | m[y, x]) : ColorDefaults.DefaultBkgColorInt;
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
        catch (Exception exx)
        {
            LogMan.Exception(sClass, sMethod, exx);            
            return null;
        }
    }

    public static System.Drawing.Image? MatrixToGdiImage(int[,] m)
     => MatrixToGdiImageUnsafe(m);

    public static System.Drawing.Image? ImageDataToGdiImage(ImageData image)
     => MatrixToGdiImage(image?.GetMatrix());


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
            //Test OK
            //imageDest.Save("columns:\\TestX\\Temp.bmp");
            return imageDest;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    unsafe static Bitmap? MatrixToGdiImageIndexedUnsafe(int[,] indexMatrix, List<int> paletteSrc, WidthAlignMode alignMode)
    {
        string sMethod = nameof(MatrixToGdiImageIndexed);
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
            ImageData img = new ImageData().Create(indexMatrix);
            var mDataIndex = ImageTools.CreateIndexedMatrix(img, alignMode);
            int height = mDataIndex.GetLength(0);
            int width = mDataIndex.GetLength(1);

            var bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            var lockDst = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    byte* pucData = (byte*)(lockDst.Scan0 + lockDst.Stride * y);
                    for (int x = 0; x < width; x++)
                    {
                        pucData[x] = mDataIndex[y, x];
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                bmp.UnlockBits(lockDst);
                bmp.Dispose();
                return null;
            }
            bmp.UnlockBits(lockDst);
            return bmp;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    public static Bitmap? MatrixToGdiImageIndexed(int[,] indexMatrix, List<int> paletteSrc, WidthAlignMode alignMode)
       => MatrixToGdiImageIndexedUnsafe(indexMatrix, paletteSrc, alignMode);

    public static Bitmap? MatrixToGdiImageIndexed(int[,] indexMatrix, Palette paletteSrc, WidthAlignMode alignMode)
        => MatrixToGdiImageIndexed(indexMatrix, paletteSrc?.ToList(), alignMode);

    public static Bitmap? ImageDataToGdiImageIndexed(ImageData image, WidthAlignMode alignMode)
        => MatrixToGdiImageIndexed(image?.GetMatrix(), image?.ColorPalette, alignMode);



    #endregion

    #region GDI Palette Helpers


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
            LogMan.Error(sClass, sMethod, "lPaletteSrc is null");
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

    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(Dictionary<int, int> transformationMap)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (transformationMap == null)
        {
            LogMan.Error(sClass, sMethod, "Palette Dictionary is null");
            return null;
        }
        List<int> col = transformationMap.Where(x => x.Value >= 0).Select(x => x.Value).ToList().Distinct().ToList();
        return ToGdiColorPalette(col);
    }


    #endregion
}