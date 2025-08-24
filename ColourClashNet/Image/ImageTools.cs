using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace ColourClashNet.ImageTools
{
    public enum ImageExportFormat
    {
        Unknown = 0,
        //
        Bmp24,
        BmpIndex,
        Png24,
        PngIndex,
        //
        CbmAmigaRawBitplane,
        CbmAmigaRawBitplaneInterleaved,
        CbmAmigaRawBitplaneCopperlist,
        CbmAmigaRawBitplaneInterleavedCopperlist,
    }

    public enum ImageWidthAlignMode
    {
        None  = 0,
        MultiplePixel16 = 16,
        MultiplePixel32 = 32,
        MultiplePixel64 = 64
    }

    public static partial class ImageTools
    {


        static int GetImageWidthAlign(int iW, ImageWidthAlignMode ePixelWidthAlign)
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
            return GetImageWidthAlign(oImage?.Width ?? 0, ePixelWidthAlign);
        }

       
        static public byte[,] CreateIndexedData(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            if (mDataSrc == null)
                return null;

            int R = mDataSrc.GetLength(0);
            int C = mDataSrc.GetLength(1);
            int CO = GetImageWidthAlign(C, ePixelWidthAlign);

            if (llPaletteSrc == null || llPaletteSrc.Count != R )
                return null;

            var oRet = new byte[R, CO];
            for (int y = 0; y < R; y++)
            {
                for (int x = 0; x < C; x++)
                {
                    var col = mDataSrc[y, x];
                    var idx = llPaletteSrc[y].IndexOf(col);
                    oRet[y, x] = (byte)(idx >= 0 && idx < 256 ? idx : 0);
                }
            }
            return oRet;
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


        //unsafe static public Bitmap CreateIndexedBitmap(int[,] mDataSrc, List<int>lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        //{
        //    if (mDataSrc == null)
        //        return null;
        //    if (lPaletteSrc == null || lPaletteSrc.Count == 0  )
        //        return null;
        //    int HD = mDataSrc.GetLength(0);
        //    int WD = mDataSrc.GetLength(1);

        //    int WI = GetImageWidthAlign( WD, ePixelWidthAlign);
        //    var oImageDst = new Bitmap(WI, HD, PixelFormat.Format8bppIndexed);

        //    ColorPalette oPalette = oImageDst.Palette;
        //    for (int i = 0; i < oImageDst.Palette.Entries.Length; i++)
        //    {
        //        oPalette.Entries.SetValue(Color.Black, i);
        //    }
        //    for (int i = 0; i < Math.Min(lPaletteSrc.Count, oImageDst.Palette.Entries.Length); i++)
        //    {
        //        oPalette.Entries.SetValue(ColorIntExt.ToDrawingColor(lPaletteSrc[i]), i);
        //    }
        //    oImageDst.Palette = oPalette;

        //    var oLockDst = oImageDst.LockBits(new Rectangle(0, 0, oImageDst.Width, oImageDst.Height), ImageLockMode.WriteOnly, oImageDst.PixelFormat);
        //    for (int y = 0; y < HD; y++)
        //    {
        //        byte* pucData = (byte*)(oLockDst.Scan0+oLockDst.Stride*y);
        //        for (int x = 0; x < WD; x++)
        //        {
        //            var col = mDataSrc[y, x];
        //            var idx = lPaletteSrc.IndexOf(col);
        //            pucData[x] = (byte)(idx >= 0 && idx <256 ? idx : 0);
        //        }
        //    }
        //    oImageDst.UnlockBits(oLockDst);
        //    return oImageDst;
        //}

      

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

        public static bool Export(Bitmap oImage, string sFileName, ImageExportFormat eFormat )
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
    }
}
