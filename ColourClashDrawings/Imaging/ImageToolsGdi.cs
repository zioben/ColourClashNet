using ColourClashNet.Color;
using ColourClashNet.Defaults;
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
    /// <param name="oImage">The image whose width is to be aligned. If null, a width of 0 is used.</param>
    /// <param name="ePixelWidthAlign">The alignment mode that determines how the image width should be adjusted.</param>
    /// <returns>The width of the image, adjusted according to the specified alignment mode.</returns>
    static public int GetImageNewWidthAlign(Image oImage, WidthAlignMode ePixelWidthAlign)
    {
        return MatrixTools.GetNewWidthAlign(oImage?.Width ?? 0, ePixelWidthAlign);
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
    /// <param name="sFileName">The path to the image file to open. The file must exist and be accessible for reading.</param>
    /// <returns>An Image object that represents the image contained in the specified file, or null if the file cannot be
    /// opened or the image cannot be loaded.</returns>
    public static System.Drawing.Image? GdiImageFromFile(string sFileName)
    {
        string sMethod = nameof(GdiImageFromFile);
        try
        {
            if( string.IsNullOrWhiteSpace(sFileName))
            {
                LogMan.Error(sClass, sMethod, "File name is null or empty");
                return null;
            }
            if( !File.Exists(sFileName))
            {
                LogMan.Error(sClass, sMethod, $"File does not exist: {sFileName}");
                return null;
            }
            using (var stream = File.OpenRead(sFileName))
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

    static string GetfileExt(ImageExportFormat eFormat)
    {
        switch (eFormat)
        {
            case ImageExportFormat.BmpIndexed:
            case ImageExportFormat.Bmp:
                return ".bmp";
            case ImageExportFormat.PngIndexed:
            case ImageExportFormat.Png:
                return ".png";
            case ImageExportFormat.Jpg:
                return ".jpg";  
            case ImageExportFormat.CbmAmigaRawBitplane:
                return ".amibpl";
            case ImageExportFormat.CbmAmigaRawBitplaneCopperlist:
                return ".amicoplist";
            case ImageExportFormat.CbmAmigaRawBitplaneInterleaved:
                return ".amibpli";
            case ImageExportFormat.CbmAmigaRawBitplaneInterleavedCopperlist:
                return ".amicoplisti";
            default:
                return ".bin";
        }
    }

    
    public static Stream? GdiImageToStream(System.Drawing.Image oImage, ImageExportFormat eFormat, int iQuality = 100)
    {
        string sMethod = nameof(GdiImageToStream);
        try
        {
            if (oImage == null)
            {
                LogMan.Error(sClass, sMethod, "oImage is null");
                return null;
            }
            switch (eFormat)
            {
                case ImageExportFormat.Bmp:
                case ImageExportFormat.Png:
                case ImageExportFormat.Jpg:
                    {
                        using (var oImage2 = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                        using (Graphics g = Graphics.FromImage(oImage2))
                        {
                            g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                            MemoryStream stream = new MemoryStream();
                            switch (eFormat)
                            {
                                case ImageExportFormat.Bmp:
                                    oImage2.Save(stream, ImageFormat.Bmp);
                                    return stream;
                                case ImageExportFormat.Png:
                                    oImage2.Save(stream, ImageFormat.Png);
                                    return stream;
                                case ImageExportFormat.Jpg:
                                    oImage2.Save(stream, ImageFormat.Jpeg);
                                    return stream;
                                default:
                                    return null;
                            }
                        }
                    }
                // TODO - Future support for indexed formats
                //case ImageExportFormat.PngIndexed:
                //case ImageExportFormat.BmpIndexed:
                //    {
                //        if (oImage.PixelFormat != PixelFormat.Format8bppIndexed)
                //        { 
                //        }
                //        return null;
                //    }
                default:
                    LogMan.Error(sClass, sMethod, $"{eFormat} not yet supported!");
                    return null;
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);  
            return null;
        }
    }

    public static bool GdiImageToFile(System.Drawing.Image oImage, string sFileName, ImageExportFormat eFormat)
    {
        string sMethod = nameof(GdiImageToFile);  
        try
        {
            var ms = GdiImageToStream(oImage, eFormat);   
            if (ms == null)
            {
                LogMan.Error(sClass, sMethod, $"{nameof(GdiImageToStream)} returned null");
                return false;
            }
            using (var fs = File.Create(sFileName + GetfileExt(eFormat)))
            {
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
                return true;
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return false;
        }
    }

    #endregion

    #region conversion Bitmap -> Matrix
    unsafe static int[,] ConvertFromBmp8BppIndex(System.Drawing.Image oImage)
    {
        string sM = nameof(ConvertFromBmp8BppIndex);
        try
        {
            if (!(oImage is Bitmap oBmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            List<int> rgbList = new();
            foreach( var rgb in oBmp.Palette.Entries)
            {
                rgbList.Add(ColorIntExt.FromDrawingColor(rgb));
            }

            var m = new int[oBmp.Height, oBmp.Width];

            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, oBmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    for (int x = 0; x < oBmp.Width; x++)
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
                oBmp.UnlockBits(oLock);
            }
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

    unsafe static int[,] ConvertFromBmp24Bpp(System.Drawing.Image oImage)
    {
        string sM = nameof(ConvertFromBmp24Bpp);
        try
        {
            if (!(oImage is Bitmap oBmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            var m = new int[oBmp.Height, oBmp.Width];

            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, oBmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    for (int x = 0,xx=0; x < oBmp.Width; x++,xx+=3)
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
                oBmp.UnlockBits(oLock);
            }
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }
    unsafe static int[,] ConvertFromBmp32Bpp(System.Drawing.Image oImage)
    {
        string sM = nameof(ConvertFromBmp24Bpp);
        try
        {
            if (!(oImage is Bitmap oBmp))
            {
                LogMan.Error(sClass, sM, "Bitmap is null");
                return null;
            }

            var m = new int[oBmp.Height, oBmp.Width];

            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, oBmp.PixelFormat);
            try
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    for (int x = 0, xx = 0; x < oBmp.Width; x++,xx+=4)
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
                oBmp.UnlockBits(oLock);
            }
            return m;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sM, ex);
            return null;
        }
    }

    static int[,] GdiImageToMatrixUnsafe(System.Drawing.Image oImage)
    {
        string sMethod = nameof(GdiImageToMatrixUnsafe);
        try
        {
            if( oImage == null)
            {
                LogMan.Error(sClass, sMethod, "oImage is null");
                return null;
            }

            bool bCanConvert =
              oImage.PixelFormat == PixelFormat.Format8bppIndexed ||
              oImage.PixelFormat == PixelFormat.Format24bppRgb ||
              oImage.PixelFormat == PixelFormat.Format32bppArgb;

            if (!bCanConvert)
            {
                LogMan.Error(sClass, sMethod, $"Pixel format {oImage.PixelFormat} not supported in {sMethod}");
                return null;
            }

            var m = new int[oImage.Height, oImage.Width];

            switch ( oImage.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return ConvertFromBmp8BppIndex(oImage);
                case PixelFormat.Format24bppRgb:
                    return ConvertFromBmp24Bpp(oImage);
                case PixelFormat.Format32bppArgb:
                    return ConvertFromBmp32Bpp(oImage);
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

    public static int[,] GdiImageToMatrix(System.Drawing.Image oImage)
        => GdiImageToMatrixUnsafe(oImage);

    public static ImageData? GdiImageToImageData(System.Drawing.Image oImage)
    {
        string sMethod = nameof(GdiImageToImageData);
        try
        {
            var oImg = new ImageData();
            if (oImage == null)
            {
                LogMan.Error(sClass, sMethod, "oImage is null");
                return oImg;
            }
            var data = GdiImageToMatrixUnsafe(oImage);
            return oImg.Create(data);
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
            var R = m.GetLength(0);
            var C = m.GetLength(1);
            var oBmp = new Bitmap(C, R, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                //byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    int* ptrRow = (int*)(oLock.Scan0 + yoff);
                    for (int x = 0; x < oBmp.Width; x++)
                    {
                        ptrRow[x] = m[y, x].IsColor() ? (int)(0xFF_00_00_00 | m[y, x]) : ColorDefaults.DefaultBkgColorInt;
                    }
                }
                oBmp.UnlockBits(oLock);
                return oBmp;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                oBmp.UnlockBits(oLock);
                oBmp.Dispose();
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

    public static System.Drawing.Image? ImageDataToGdiImage(ImageData oImage)
     => MatrixToGdiImage(oImage?.GetMatrix());


    public static System.Drawing.Image? GdiImageToGdiImage32bpp(System.Drawing.Image oImage)
    {
        string sMethod = nameof(GdiImageToGdiImage32bpp);
        try
        {
            if (oImage == null)
            {
                LogMan.Error(sClass, sMethod, "image is null");
                return null;
            }
            var oImageDest = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                using (Graphics g = Graphics.FromImage(oImageDest))
                {
                    g.DrawImage(oImage, 0, 0);
                }
            }
            //Test OK
            //oImageDest.Save("C:\\TestX\\Temp.bmp");
            return oImageDest;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    unsafe static Bitmap? MatrixToGdiImageIndexedUnsafe(int[,] mData, List<int> lPaletteSrc, WidthAlignMode eAlignMode)
    {
        string sMethod = nameof(MatrixToGdiImageIndexed);
        try
        {
            if (mData == null)
            {
                LogMan.Error(sClass, sMethod, "Matrix is null");
                return null;
            }
            if( lPaletteSrc == null)
            {
                LogMan.Error(sClass, sMethod, "Palette is null");
                return null;
            }
            ImageData oImg = new ImageData().Create(mData);
            var mDataIndex = ImageTools.CreateIndexedMatrix(oImg, eAlignMode);
            int HD = mDataIndex.GetLength(0);
            int WD = mDataIndex.GetLength(1);

            var oBmp = new Bitmap(WD, HD, PixelFormat.Format8bppIndexed);

            var oLockDst = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), ImageLockMode.WriteOnly, oBmp.PixelFormat);
            try
            {
                for (int y = 0; y < HD; y++)
                {
                    byte* pucData = (byte*)(oLockDst.Scan0 + oLockDst.Stride * y);
                    for (int x = 0; x < WD; x++)
                    {
                        pucData[x] = mDataIndex[y, x];
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                oBmp.UnlockBits(oLockDst);
                oBmp.Dispose();
                return null;
            }
            oBmp.UnlockBits(oLockDst);
            return oBmp;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sClass, sMethod, ex);
            return null;
        }
    }

    public static Bitmap? MatrixToGdiImageIndexed(int[,] mData, List<int> lPaletteSrc, WidthAlignMode eAlignMode)
       => MatrixToGdiImageIndexedUnsafe(mData, lPaletteSrc, eAlignMode);

    public static Bitmap? MatrixToGdiImageIndexed(int[,] mData, Palette oPaletteSrc, WidthAlignMode eAlignMode)
        => MatrixToGdiImageIndexed(mData, oPaletteSrc?.ToList(), eAlignMode);

    public static Bitmap? ImageDataToGdiImageIndexed(ImageData oImage, WidthAlignMode eAlignMode)
        => MatrixToGdiImageIndexed(oImage?.GetMatrix(), oImage?.ColorPalette, eAlignMode);



    #endregion

    #region GDI Palette Helpers


    static bool SetGdiImageColorPalette(System.Drawing.Image oImage, List<int> lPaletteSrc)
    {
        string sMethod = nameof(SetGdiImageColorPalette);
        if (!(oImage is Bitmap oBmp))
        {
            LogMan.Error(sClass, sMethod, "Bitmap is null");
            return false;
        }
        if (lPaletteSrc == null )
        {
            LogMan.Error(sClass, sMethod, "lPaletteSrc is null");
            return false;
        }
        ColorPalette oPalette = oBmp.Palette;
        for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
        {
            oPalette.Entries[i] = System.Drawing.Color.Black;
        }
        if (lPaletteSrc.Count > oBmp.Palette.Entries.Length)
        {
            LogMan.Warning(sClass, sMethod, $"Source palette has more entries than target bitmap palette can hold; truncating to {oBmp.Palette.Entries.Length} elements.");
        }
        for (int i = 0; i < Math.Min(oBmp.Palette.Entries.Length, lPaletteSrc.Count); i++)
        {
            oPalette.Entries[i] = ColorIntExt.ToDrawingColor(lPaletteSrc[i]);
        }
        oBmp.Palette = oPalette;
        return true;
    }
  
  
    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(List<int> lPalette)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (lPalette == null | lPalette.Count <= 0)
        {
            LogMan.Error(sClass, sMethod, "Palette list is null or empty");
            return null;
        }
        using (var oBmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
        {
            var loop = Math.Min(lPalette.Count, oBmp.Palette.Entries.Length);
            for (int i = 0; i < loop; i++)
            {
                oBmp.Palette.Entries[i] = lPalette[i].ToDrawingColor();
            }
            for (int i = loop; i < oBmp.Palette.Entries.Length; i++)
            {
                oBmp.Palette.Entries[i] = ColorDefaults.DefaultBkgColor;
            }
            return oBmp.Palette;
        }
    }

    public static System.Drawing.Imaging.ColorPalette ToGdiGrayScalePalette()
    {
        string sMethod = nameof(ToGdiColorPalette);
        using (var oBmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
        {
            for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
            {
                oBmp.Palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            return oBmp.Palette;
        }
    }

    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(ColourClashNet.Color.Palette oPalette)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (oPalette == null | oPalette.Count <= 0)
        {
            LogMan.Error(sClass, sMethod, "Color Palette is null or empty");
            return null;
        }
        return ToGdiColorPalette(oPalette?.ToList());
    }

    public static System.Drawing.Imaging.ColorPalette ToGdiColorPalette(Dictionary<int, int> dictTrasf)
    {
        string sMethod = nameof(ToGdiColorPalette);
        if (dictTrasf == null)
        {
            LogMan.Error(sClass, sMethod, "Palette Dictionary is null");
            return null;
        }
        List<int> lCol = dictTrasf.Where(X => X.Value >= 0).Select(X => X.Value).ToList().Distinct().ToList();
        return ToGdiColorPalette(lCol);
    }


    #endregion
}