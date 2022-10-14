using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.ImageTools
{
    public enum ImageExportformat
    {
        Unknown = 0,
        //
        Bmp24,
        BmpIndex,
        Png24,
        PngIndex,
        //
        RawBitplane,
        RawBitplaneInterleaved,
        RawBitplaneCopperlist,
        RawBitplaneInterleavedCopperlist,
    }

    public enum WidthAlignMode
    {
        None  = 0,
        Int16 = 16,
        Int32 = 32,
        Int64 = 64
    }

    public static class ImageExport
    {
        static int GetNewWidth(Bitmap oImage, WidthAlignMode ePixelWidthAlign)
        {
            var W = oImage.Width;
            switch (ePixelWidthAlign)
            {
                case WidthAlignMode.Int16:
                    W += (W + 15) & 0x0F;
                    break;
                case WidthAlignMode.Int32:
                    W += (W + 31) & 0x1F;
                    break;
                case WidthAlignMode.Int64:
                    W += (W + 63) & 0x3F;
                    break;
                default:
                    break;
            }
            return W;
        }

        static string GetfileExt(ImageExportformat eFormat)
        {
            switch (eFormat)
            {
                case ImageExportformat.Bmp24:
                    return ".bmp";
                case ImageExportformat.BmpIndex:
                    return ".bmp";
                case ImageExportformat.Png24:
                    return ".png";
                case ImageExportformat.PngIndex:
                    return ".png";
                case ImageExportformat.RawBitplane:
                case ImageExportformat.RawBitplaneCopperlist:
                case ImageExportformat.RawBitplaneInterleaved:
                case ImageExportformat.RawBitplaneInterleavedCopperlist:
                    return ".bpl";
                default:
                    return ".bin";
            }
        }

        unsafe public static bool Export(Bitmap oImage, string sFileName, ImageExportformat eFormat, ColorPalette oPalette, WidthAlignMode ePixelWidthAlign )
        {
            if (oImage == null)
                return false;
            try
            {
                int W = GetNewWidth(oImage, ePixelWidthAlign);
                int H = oImage.Height;

                switch (eFormat)
                {
                    case ImageExportformat.Bmp24:
                        {
                            using (var oImage2 = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                            using (Graphics g = Graphics.FromImage(oImage2))
                            {
                                g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                                var oBmpEncoder = ImageCodecInfo.GetImageEncoders();
                                oImage2.Save(sFileName + ".bmp", ImageFormat.Bmp);
                                return true;
                            }
                        }
                    case ImageExportformat.BmpIndex:
                        {
                            using (var oImage2 = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Indexed))
                            using (Graphics g = Graphics.FromImage(oImage2))
                            {
                                oImage2.Palette = oPalette;
                                g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                                var oBmpEncoder = ImageCodecInfo.GetImageEncoders();
                                oImage2.Save(sFileName + ".bmp", ImageFormat.Bmp);
                                return true;
                            }
                        }
                    case ImageExportformat.Png24:
                        {
                            var oImage2 = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            using (Graphics g = Graphics.FromImage(oImage2))
                            {
                                g.DrawImage(oImage, 0, 0, oImage.Width, oImage.Height);
                                var oBmpEncoder = ImageCodecInfo.GetImageEncoders();
                                oImage2.Save(sFileName + ".png", ImageFormat.Png);
                                return true;
                            }
                        }
                    default:
                        return false;
                        break;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
