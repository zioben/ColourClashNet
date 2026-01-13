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
using static System.Net.Mime.MediaTypeNames;

namespace ColourClashNet.Imaging
{
    public static partial class ImageTools
    {

        #region BMP Creation

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
        /// <returns>A Bitmap object in Format32bppArgb pixel format if the image is successfully loaded; otherwise, null if the
        /// image cannot be created from the stream.</returns>
        public static Bitmap? BitmapFromStream(Stream stream)
        {
            string sMethod = nameof(BitmapFromStream);
            try
            {
                var bitmap = new Bitmap(stream);
                switch (bitmap.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        return bitmap;
                    default:
                        var convertedBitmap = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        using (var g = Graphics.FromImage(convertedBitmap))
                        {
                            g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                        }
                        bitmap.Dispose();
                        return convertedBitmap;
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
        /// <returns>A Bitmap object that represents the image contained in the specified file, or null if the file cannot be
        /// opened or the image cannot be loaded.</returns>
        public static Bitmap? BitmapFromFile(string sFileName)
        {
            string sMethod = nameof(BitmapFromFile);
            try
            {
                var stream = File.OpenRead(sFileName);
                stream.Seek(0, SeekOrigin.Begin);
                return BitmapFromStream(stream);
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
                case ImageExportFormat.CbmAmigaRawBitplane:
                case ImageExportFormat.CbmAmigaRawBitplaneCopperlist:
                case ImageExportFormat.CbmAmigaRawBitplaneInterleaved:
                case ImageExportFormat.CbmAmigaRawBitplaneInterleavedCopperlist:
                    return ".bpl";
                default:
                    return ".bin";
            }
        }

        public static Stream? BitmapToStream(Bitmap oImage, ImageExportFormat eFormat, int iQuality = 100)
        {
            ArgumentNullException.ThrowIfNull(oImage);
            string sMethod = nameof(BitmapToStream);
            try
            {
                if (oImage == null)
                {
                    LogMan.Error(sClass, sMethod, "Bitmap is null");
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




        public static bool BitmapToFile(Bitmap oImage, string sFileName, ImageExportFormat eFormat)
        {
            string sMethod = nameof(BitmapToFile);  
            try
            {
                var ms = BitmapToStream(oImage, eFormat);   
                if (ms == null)
                {
                    LogMan.Error(sClass, sMethod, $"{nameof(BitmapToStream)} returned null");
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

        #region conversion BMP -> Matrix

        public unsafe static int[,] BitmapToMatrix(Bitmap oBmp)
        {
            string sMethod = nameof(BitmapToMatrix);
            try
            {
                if( oBmp == null)
                {
                    LogMan.Error(sClass, sMethod, "Bitmap is null");
                    return null;
                }

                var m = new int[oBmp.Height, oBmp.Width];
                var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                {
                    try
                    {
                        byte* ptr = (byte*)oLock.Scan0.ToPointer();
                        for (int y = 0; y < oBmp.Height; y++)
                        {
                            int yoff = oLock.Stride * y;
                            int* ptrRow = (int*)(oLock.Scan0 + yoff);
                            for (int x = 0; x < oBmp.Width; x++)
                            {
                                int rgb = ptrRow[x];
                                m[y, x] = (int)(ptrRow[x] & 0x00FFFFFF);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMan.Exception(sClass, sMethod, ex);
                    }
                    finally
                    {
                        oBmp.UnlockBits(oLock);
                    }
                }
                return m;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                return null;
            }
        }

        #endregion

        #region Matrix -> Bitmap    

        public unsafe static Bitmap MatrixToBitmap(int[,] m)
        {
            string sMethod = nameof(MatrixToBitmap);
            if (m == null)
            {
                LogMan.Error(sClass, sMethod, "Matrix is null");
                return null;
            }
            var R = m.GetLength(0);
            var C = m.GetLength(1);
            var oBmp = new Bitmap(C, R, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                try
                {
                    byte* ptr = (byte*)oLock.Scan0.ToPointer();
                    for (int y = 0; y < oBmp.Height; y++)
                    {
                        int yoff = oLock.Stride * y;
                        int* ptrRow = (int*)(oLock.Scan0 + yoff);
                        for (int x = 0; x < oBmp.Width; x++)
                        {
                            ptrRow[x] = m[y, x] >= 0 ? m[y, x] : ColorDefaults.DefaultBkgColorInt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sClass, sMethod, ex);
                }
                finally
                {
                    oBmp.UnlockBits(oLock);
                }
            }
            return oBmp;
        }

        public static Bitmap MatrixToBitmap(Bitmap oImage)
        {
            string sMethod = nameof(MatrixToBitmap);
            if (oImage == null)
            {
                LogMan.Error(sClass, sMethod, "Bitmap is null");
                return null;
            }
            var oImageDest = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                using (Graphics g = Graphics.FromImage(oImage))
                {
                    g.DrawImage(oImage, 0, 0, oImageDest.Width, oImageDest.Height);
                }
            }
            return oImageDest;
        }

        unsafe static Bitmap MatrixToBitmapIndexed(byte[,] mDataIndex)
        {
            if (mDataIndex == null)
                return null;

            int HD = mDataIndex.GetLength(0);
            int WD = mDataIndex.GetLength(1);

            var oBmp = new Bitmap(WD, HD, PixelFormat.Format8bppIndexed);

            var oLockDst = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), ImageLockMode.WriteOnly, oBmp.PixelFormat);
            for (int y = 0; y < HD; y++)
            {
                byte* pucData = (byte*)(oLockDst.Scan0 + oLockDst.Stride * y);
                for (int x = 0; x < WD; x++)
                {
                    pucData[x] = mDataIndex[y, x];
                }
            }
            oBmp.UnlockBits(oLockDst);
            return oBmp;
        }

        //static public Bitmap ToBitmapIndexed(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        //{
        //    var mDataIndex = CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
        //    var oBmp = ToBitmapIndexed(mDataIndex);
        //    SetBitmapPalette(oBmp, llPaletteSrc);
        //    return oBmp;
        //}

        static public Bitmap MatrixToBitmapIndexed(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, lPaletteSrc, ePixelWidthAlign);
            var oBmp = MatrixToBitmapIndexed(mDataIndex);
            SetBitmapPalette(oBmp, lPaletteSrc);
            return oBmp;
        }

        #endregion


        #region Palette Helpers
        static void SetBitmapPalette(Bitmap oBmp, List<List<int>> llPaletteSrc)
        {
            if (oBmp == null)
                return;
            ColorPalette oPalette = oBmp.Palette;
            for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
            {
                oPalette.Entries[i] = System.Drawing.Color.FromArgb(255, (i % 16) * 16 + 15, (i % 16) * 16 + 15, (i % 16) * 16 + 15);
            }
            oBmp.Palette = oPalette;
        }

        static void SetBitmapPalette(Bitmap oBmp, List<int> lPaletteSrc)
        {
            if (oBmp == null)
                return;
            ColorPalette oPalette = oBmp.Palette;
            for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
            {
                oPalette.Entries[i] = System.Drawing.Color.Black;
            }
            for (int i = 0; i < Math.Min(oBmp.Palette.Entries.Length, lPaletteSrc.Count); i++)
            {
                oPalette.Entries[i] = ColorIntExt.ToDrawingColor(lPaletteSrc[i]);
            }
            oBmp.Palette = oPalette;
        }
        #endregion
    }
}