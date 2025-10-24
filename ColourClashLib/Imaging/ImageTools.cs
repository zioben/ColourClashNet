using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Defaults;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using static System.Net.Mime.MediaTypeNames;

namespace ColourClashNet.Imaging
{
    public static partial class ImageTools
    {
        static string sClass = nameof(ImageTools);

       

        static int GetImageWidthAligned(int iW, ImageWidthAlignMode ePixelWidthAlign)
        {
            switch (ePixelWidthAlign)
            {
                case ImageWidthAlignMode.MultiplePixel16:
                    return (iW + 15) & (~0x0000000F);
                case ImageWidthAlignMode.MultiplePixel32:
                    return (iW + 31) & (~0x0000001F);
                case ImageWidthAlignMode.MultiplePixel64:
                    return (iW + 63) & (~0x0000003F);
                    break;
                default:
                    break;
            }
            return iW;
        }

        static int GetImageWidthAlign(Bitmap oImage, ImageWidthAlignMode ePixelWidthAlign)
        {
            return GetImageWidthAligned(oImage?.Width ?? 0, ePixelWidthAlign);
        }


        static public byte[,] CreateIndexedData(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            if (mDataSrc == null)
                return null;

            int R = mDataSrc.GetLength(0);
            int C = mDataSrc.GetLength(1);
            int CO = GetImageWidthAligned(C, ePixelWidthAlign);

            if (llPaletteSrc == null || llPaletteSrc.Count != R)
                return null;

            var oRet = new byte[R, CO];
            for (int y = 0; y < R; y++)
            {
                for (int x = 0; x < C; x++)
                {
                    var col = mDataSrc[y, x];
                    var idx = llPaletteSrc[y].IndexOf(col);
                    oRet[y, x] = (byte)(idx >= 0 && idx < 256 ? idx : ColorDefaults.DefaultInvalidColorInt);
                }
            }
            return oRet;
        }


        static public ImageWidthAlignMode GetImageWidthStdAlignMode()
        {
            return ImageWidthAlignMode.MultiplePixel16;
        }

        static public byte[,] CreateIndexedData(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            if (mDataSrc == null)
                return null;

            int R = mDataSrc.GetLength(0);

            List<List<int>> llPaletteSrc = new List<List<int>>(R);
            for (int r = 0; r < R; r++)
                llPaletteSrc.Add(lPaletteSrc);

            return CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
        }


        //unsafe static public Bitmap ToBitmapIndexed(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        //{
        //    if (mDataSrc == null)
        //        return null;
        //    if (lPaletteSrc == null || lPaletteSrc.Count == 0)
        //        return null;
        //    int HD = mDataSrc.GetLength(0);
        //    int WD = mDataSrc.GetLength(1);

        //    int WI = GetImageWidthAlign(WD, ePixelWidthAlign);
        //    var oImageDst = new Bitmap(WI, HD, PixelFormat.Format8bppIndexed);

        //    ColorPalette oPalette = oImageDst.Palette;
        //    for (int i = 0; i < oImageDst.Palette.Entries.Length; i++)
        //    {
        //        oPalette.Entries.SetValue(System.Drawing.Color.Black, i);
        //    }
        //    for (int i = 0; i < Math.Min(lPaletteSrc.Count, oImageDst.Palette.Entries.Length); i++)
        //    {
        //        oPalette.Entries.SetValue(ColorIntExt.ToDrawingColor(lPaletteSrc[i]), i);
        //    }
        //    oImageDst.Palette = oPalette;

        //    var oLockDst = oImageDst.LockBits(new Rectangle(0, 0, oImageDst.Width, oImageDst.Height), ImageLockMode.WriteOnly, oImageDst.PixelFormat);
        //    for (int y = 0; y < HD; y++)
        //    {
        //        byte* pucData = (byte*)(oLockDst.Scan0 + oLockDst.Stride * y);
        //        for (int x = 0; x < WD; x++)
        //        {
        //            var col = mDataSrc[y, x];
        //            var idx = lPaletteSrc.IndexOf(col);
        //            pucData[x] = (byte)(idx >= 0 && idx < 256 ? idx : 0);
        //        }
        //    }
        //    oImageDst.UnlockBits(oLockDst);
        //    return oImageDst;
        //}


        #region Image Export

        static string GetfileExt(ImageExportFormat eFormat)
        {
            switch (eFormat)
            {
                case ImageExportFormat.Bmp24:
                    return ".bmp";
                case ImageExportFormat.BmpIndex:
                    return ".bmp";
                case ImageExportFormat.Png24:
                    return ".png";
                case ImageExportFormat.PngIndex:
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

        public static bool Export(Bitmap oImage, string sFileName, ImageExportFormat eFormat)
        {
            if (oImage == null)
                return false;
            try
            {
                switch (eFormat)
                {
                    case ImageExportFormat.Bmp24:
                        {
                            using (var oImage2 = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                            using (Graphics g = Graphics.FromImage(oImage2))
                            {
                                g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                                var oBmpEncoder = ImageCodecInfo.GetImageEncoders();
                                oImage2.Save(sFileName + ".bmp", ImageFormat.Bmp);
                                return true;
                            }
                        }
                    case ImageExportFormat.Png24:
                        {
                            using (var oImage2 = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                            using (Graphics g = Graphics.FromImage(oImage2))
                            {
                                g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                                var oBmpEncoder = ImageCodecInfo.GetImageEncoders();
                                oImage2.Save(sFileName + ".png", ImageFormat.Png);
                                return true;
                            }
                        }
                    case ImageExportFormat.BmpIndex:
                        {
                            oImage.Save(sFileName + ".bmp", ImageFormat.Bmp);
                            return true;
                        }
                    case ImageExportFormat.PngIndex:
                        {
                            oImage.Save(sFileName + ".png", ImageFormat.Png);
                            return true;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion

        #region conversion BMP -> Matrix

        public unsafe static int[,] ToMatrix(System.Drawing.Image oImage)
        {
            string sMethod = nameof(ToMatrix);
            if (oImage is Bitmap oBmp)
            {
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
            else
            {
                LogMan.Error(sClass, sMethod, "Bitmap is null");
                return null;
            }
        }

        #endregion

        #region Matrix -> Bitmap    

        public unsafe static Bitmap ToBitmap(int[,] m)
        {
            string sMethod = nameof(ToBitmap);
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

        public static Bitmap ToBitmap(Bitmap oImage)
        {
            string sMethod = nameof(ToBitmap);
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

        unsafe static Bitmap ToBitmapIndexed(byte[,] mDataIndex)
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

        static public Bitmap ToBitmapIndexed(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
            var oBmp = ToBitmapIndexed(mDataIndex);
            SetBitmapPalette(oBmp, llPaletteSrc);
            return oBmp;
        }

        static public Bitmap ToBitmapIndexed(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, lPaletteSrc, ePixelWidthAlign);
            var oBmp = ToBitmapIndexed(mDataIndex);
            SetBitmapPalette(oBmp, lPaletteSrc);
            return oBmp;
        }

        #endregion

        #region Plaette -> ColorPalette

        public static System.Drawing.Imaging.ColorPalette ToColorPalette(List<int> lPalette)
        {
            string sMethod = nameof(ToColorPalette);
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
        public static System.Drawing.Imaging.ColorPalette ToColorPalette(ColourClashNet.Color.Palette oPalette)
        {
            string sMethod = nameof(ToColorPalette);
            if (oPalette == null | oPalette.Count <= 0)
            {
                LogMan.Error(sClass, sMethod, "Color Palette is null or empty");
                return null;
            }
            return ToColorPalette(oPalette?.ToList());
        }

        public static System.Drawing.Imaging.ColorPalette ToColorPalette(Dictionary<int, int> dictTrasf)
        {
            string sMethod = nameof(ToColorPalette);
            if (dictTrasf == null)
            {
                LogMan.Error(sClass, sMethod, "Palette Dictionary is null");
                return null;
            }
            List<int> lCol = dictTrasf.Where(X => X.Value >= 0).Select(X => X.Value).ToList().Distinct().ToList();
            return ToColorPalette(lCol);
        }

        #endregion

        

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

      
    }
}
