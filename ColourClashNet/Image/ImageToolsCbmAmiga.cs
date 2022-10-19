using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.ImageTools
{
    public static partial class ImageTools
    {
        static int GetPlanes(int iPaletteEntries)
        {
            if (iPaletteEntries > 256)
                return 0;
            if (iPaletteEntries > 128)
                return 8;
            if (iPaletteEntries > 64)
                return 7;
            if (iPaletteEntries > 32)
                return 6;
            if (iPaletteEntries > 16)
                return 5;
            if (iPaletteEntries > 8)
                return 4;
            if (iPaletteEntries > 4)
                return 3;
            if (iPaletteEntries > 2)
                return 2;
            if (iPaletteEntries > 1)
                return 1;
            return 0;
        }

        static public byte[,] CreateBitplaneData(byte[,] mDataIndex, bool bInterleavedData, int MaxColors)
        {
            if (mDataIndex == null)
                return null;

            var R = mDataIndex.GetLength(0);
            var C = mDataIndex.GetLength(1);
            var Bytes = C / 8;
            var Planes = GetPlanes(MaxColors);// llPaletteSrc.Max(X => X.Count));
            var oRet = new byte[R * Planes, Bytes];

            byte[] chunky = new byte[8];
            byte[] planar = new byte[8];
            //----------------------------
            // C2P conversion
            //----------------------------
            for (int r = 0; r < R; r++)
            {
                for (int c = 0, w = 0; c < C; c += 8, w++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        chunky[k] = mDataIndex[r, c + k];
                        //a7a6a5a4a3a2a1a0-b7b6b5b4b3b2b1b0-.....-g7g6g5g4g3g2g1g0
                        //a0b0..........g0-a1b1..........g1-.....-a7b7..........g7
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        int bpl = 0;
                        for (int j = 0; j < 8; j++)
                            bpl |= (chunky[j] & 0x01) << (8 - j);
                        planar[k] = (byte)bpl;
                        for (int j = 0; j < 8; j++)
                            chunky[j] >>= 1;
                    }
                    if (bInterleavedData)
                    {
                        for (int k = 0; k < Planes; k++)
                        {
                            oRet[r * Planes + k, w] = planar[k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < Planes; k++)
                        {
                            oRet[r + k * R, w] = planar[k];
                        }
                    }
                }
            }
            return oRet;
        }

        static public byte[,] CreateBitplaneData(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
            return CreateBitplaneData(mDataIndex, bInterleavedData, llPaletteSrc.Max(X => X.Count));
          
        }

        static public byte[,] CreateBitplaneData(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, lPaletteSrc, ePixelWidthAlign);
            return CreateBitplaneData(mDataIndex, bInterleavedData, lPaletteSrc.Count);

        }

        static public Bitmap CreateIndexedBitplaneBitmap(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDtaBpl = CreateBitplaneData(mDataSrc, llPaletteSrc, ePixelWidthAlign, bInterleavedData);
            var oBmp = CreateIndexedBitmap(mDtaBpl);
            BitmapSetPalette(oBmp, llPaletteSrc);
            return oBmp;
        }

        static public Bitmap CreateIndexedBitplaneBitmap(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDtaBpl = CreateBitplaneData(mDataSrc, lPaletteSrc, ePixelWidthAlign, bInterleavedData);
            var oBmp = CreateIndexedBitmap(mDtaBpl);
            BitmapSetPalette(oBmp, lPaletteSrc);
            return oBmp;
        }
    }
}
