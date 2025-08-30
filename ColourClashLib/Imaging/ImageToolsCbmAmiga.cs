using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public static partial class ImageTools
    {
        static int BitplaneGetPlanes(int iPaletteEntries)
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

        static public byte[,] BitplaneCreateData(byte[,] mDataIndex, bool bInterleavedData, int MaxColors)
        {
            if (mDataIndex == null)
                return null;

            var R = mDataIndex.GetLength(0);
            var C = mDataIndex.GetLength(1);
            var Bytes = C / 8;
            var Planes = BitplaneGetPlanes(MaxColors);// llPaletteSrc.Max(X => X.Count));
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

        static public byte[,] BitplaneCreateData(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, llPaletteSrc, ePixelWidthAlign);
            return BitplaneCreateData(mDataIndex, bInterleavedData, llPaletteSrc.Max(X => X.Count));
          
        }

        static public byte[,] BitplaneCreateData(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = CreateIndexedData(mDataSrc, lPaletteSrc, ePixelWidthAlign);
            return BitplaneCreateData(mDataIndex, bInterleavedData, lPaletteSrc.Count);
        }

        static public Bitmap BitplaneCreateIndexedBitmap(int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDtaBpl = BitplaneCreateData(mDataSrc, llPaletteSrc, ePixelWidthAlign, bInterleavedData);
            var oBmp = CreateIndexedBitmap(mDtaBpl);
            BitmapSetPalette(oBmp, llPaletteSrc);
            return oBmp;
        }

        static public Bitmap BitplaneCreateIndexedBitmap(int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDtaBpl = BitplaneCreateData(mDataSrc, lPaletteSrc, ePixelWidthAlign, bInterleavedData);
            var oBmp = CreateIndexedBitmap(mDtaBpl);
            BitmapSetPalette(oBmp, lPaletteSrc);
            return oBmp;
        }


        public static void BitplaneWriteFileBpl(string sFile, byte[,] mDataIndex, int iMaxColors, bool bInterleavedData)
        {
            if (mDataIndex == null)
                return;
            var R = mDataIndex.GetLength(0);
            var C = mDataIndex.GetLength(1);
            var W = C / 2;
            var Planes = BitplaneGetPlanes(iMaxColors);
            var s = bInterleavedData ? "il" : "pr";
            var sFileName = $"{sFile}_r{R/4:D4}_w{W:D4}_p{Planes}_{s}.bpl";

            using (var oFileDat = File.OpenWrite(sFileName))
            {
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        oFileDat.WriteByte(mDataIndex[r, c]);
                    }
                }
            }
        }

        public static void BitplaneWriteFilePalette(string sFile, List<List<int>> llPaletteSrc)
        {
            if (llPaletteSrc == null || llPaletteSrc.Count == 0)
                return;
            var sFileName = $"{sFile}_{llPaletteSrc.Count}_{llPaletteSrc.Max(X=>X.Count)}";
            using (var oFilePal = File.OpenWrite(sFileName + ".rgb24"))
            {
                llPaletteSrc.ForEach(X=>
                    {
                        X.ForEach(Y => 
                        {
                            oFilePal.WriteByte((byte)Y.ToR());
                            oFilePal.WriteByte((byte)Y.ToG());
                            oFilePal.WriteByte((byte)Y.ToB());
                        });
                });
            }
            using (var oFilePal = File.OpenWrite(sFileName + ".rgb12"))
            {
                llPaletteSrc.ForEach(X =>
                {
                    X.ForEach(Y =>
                    {
                        var R = (byte)Y.ToR();
                        var G = (byte)Y.ToG();
                        var B = (byte)Y.ToB();
                        oFilePal.WriteByte((byte)(R>>4));
                        oFilePal.WriteByte((byte)((G & 0xF0) | (B>>4)));
                    });
                });
            }
        }

        public static void BitplaneWriteFile(string sFile, int[,] mDataSrc, List<List<int>> llPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = BitplaneCreateData( mDataSrc,llPaletteSrc,ePixelWidthAlign,bInterleavedData);
            BitplaneWriteFileBpl(sFile, mDataIndex, llPaletteSrc.Max(X => X.Count),bInterleavedData);
            BitplaneWriteFilePalette(sFile, llPaletteSrc);
        }
        public static void BitplaneWriteFile(string sFile, int[,] mDataSrc, List<int> lPaletteSrc, ImageWidthAlignMode ePixelWidthAlign, bool bInterleavedData)
        {
            var mDataIndex = BitplaneCreateData(mDataSrc, lPaletteSrc, ePixelWidthAlign, bInterleavedData);
            BitplaneWriteFileBpl(sFile, mDataIndex, lPaletteSrc.Count, bInterleavedData);
            BitplaneWriteFilePalette(sFile, new List<List<int>>() { lPaletteSrc });
        }

    }
}
