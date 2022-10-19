using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.ImageTools
{
    public static partial class ImageTools
    {
        unsafe static Bitmap CreateIndexedBitmap(byte[,] mDataIndex)
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

        static void BitmapSetPalette(Bitmap oBmp, List<List<int>> llPaletteSrc)
        {
            if (oBmp == null)
                return;
            ColorPalette oPalette = oBmp.Palette;
            for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
            {
                oPalette.Entries[i] = Color.FromArgb(255, (i % 16) * 16 + 15, (i % 16) * 16 + 15, (i % 16) * 16 + 15);
            }
            oBmp.Palette = oPalette;
        }

        static void BitmapSetPalette(Bitmap oBmp, List<int> lPaletteSrc)
        {
            if (oBmp == null)
                return;
            ColorPalette oPalette = oBmp.Palette;
            for (int i = 0; i < oBmp.Palette.Entries.Length; i++)
            {
                oPalette.Entries[i] = Color.Black;
            }
            for (int i = 0; i < Math.Min(oBmp.Palette.Entries.Length, lPaletteSrc.Count); i++)
            {
                oPalette.Entries[i] = ColorIntExt.ToDrawingColor(lPaletteSrc[i]);
            }
            oBmp.Palette = oPalette;
        }

        unsafe static public Bitmap CreateIndexedBitmap(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
            var oBmp = CreateIndexedBitmap(mDataIndex);
            BitmapSetPalette(oBmp, llPaletteSrc);
            return oBmp;
        }

        unsafe static public Bitmap CreateIndexedBitmap(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, lPaletteSrc, ePixelWidthAlign);
            var oBmp = CreateIndexedBitmap(mDataIndex);
            BitmapSetPalette(oBmp, lPaletteSrc);
            return oBmp;
        }
    }
}
