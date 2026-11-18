using ColourClashNet.Color;
using System;
using System.Collections.Generic;
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
        static int BitplaneGetPlanes(int paletteEntries)
        {
            if (paletteEntries > 256)
                return 0;
            if (paletteEntries > 128)
                return 8;
            if (paletteEntries > 64)
                return 7;
            if (paletteEntries > 32)
                return 6;
            if (paletteEntries > 16)
                return 5;
            if (paletteEntries > 8)
                return 4;
            if (paletteEntries > 4)
                return 3;
            if (paletteEntries > 2)
                return 2;
            if (paletteEntries > 1)
                return 1;
            return 0;
        }

        static public byte[,] BitplaneCreateData(byte[,] indexMatrix, bool interleavedData, int maxColors)
        {
            if (indexMatrix == null)
                return null;

            var rows = indexMatrix.GetLength(0);
            var columns = indexMatrix.GetLength(1);
            var bytes = columns / 8;
            var planes = BitplaneGetPlanes(maxColors);// linePalettes.Max(x => x.Count));
            var ret = new byte[rows * planes, bytes];

            byte[] chunky = new byte[8];
            byte[] planar = new byte[8];
            //----------------------------
            // C2P conversion
            //----------------------------
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0, w = 0; c < columns; c += 8, w++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        chunky[k] = indexMatrix[r, c + k];
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
                    if (interleavedData)
                    {
                        for (int k = 0; k < planes; k++)
                        {
                            ret[r * planes + k, w] = planar[k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < planes; k++)
                        {
                            ret[r + k * rows, w] = planar[k];
                        }
                    }
                }
            }
            return ret;
        }

        //static public byte[,] BitplaneCreateData(int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var indexMatrix = CreateIndexedData(sourceMatrix, linePalettes, pixelWidthAlign);
        //    return BitplaneCreateData(indexMatrix, interleavedData, linePalettes.Max(x => x.Count));
          
        //}

        static public byte[,] BitplaneCreateData(int[,] sourceMatrix, List<int> paletteSrc, WidthAlignMode pixelWidthAlign, bool interleavedData)
        {
            var indexMatrix = MatrixTools.CreateIndexedMatrix(sourceMatrix, paletteSrc, pixelWidthAlign);
            return BitplaneCreateData(indexMatrix, interleavedData, paletteSrc.Count);
        }

        //static public Bitmap BitplaneCreateIndexedBitmap(int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var mDtaBpl = BitplaneCreateData(sourceMatrix, linePalettes, pixelWidthAlign, interleavedData);
        //    var oBmp = CreateIndexedBitmap(mDtaBpl);
        //    BitmapSetPalette(oBmp, linePalettes);
        //    return oBmp;
        //}

        //static public Bitmap BitplaneCreateIndexedBitmap(int[,] sourceMatrix, List<int> paletteSrc, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var mDtaBpl = BitplaneCreateData(sourceMatrix, paletteSrc, pixelWidthAlign, interleavedData);
        //    var oBmp = CreateIndexedBitmap(mDtaBpl);
        //    BitmapSetPalette(oBmp, paletteSrc);
        //    return oBmp;
        //}


        public static void BitplaneWriteFileBpl(string file, byte[,] indexMatrix, int maxColors, bool interleavedData)
        {
            if (indexMatrix == null)
                return;
            var rows = indexMatrix.GetLength(0);
            var columns = indexMatrix.GetLength(1);
            var halfColumns = columns / 2;
            var planes = BitplaneGetPlanes(maxColors);
            var s = interleavedData ? "il" : "pr";
            var fileName = $"{file}_r{rows/4:D4}_w{halfColumns:D4}_p{planes}_{s}.bpl";

            using (var fileDat = File.OpenWrite(fileName))
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        fileDat.WriteByte(indexMatrix[r, c]);
                    }
                }
            }
        }

        public static void BitplaneWriteFilePalette(string file, List<List<int>> linePalettes)
        {
            if (linePalettes == null || linePalettes.Count == 0)
                return;
            var fileName = $"{file}_{linePalettes.Count}_{linePalettes.Max(x=>x.Count)}";
            using (var filePal = File.OpenWrite(fileName + ".rgb24"))
            {
                linePalettes.ForEach(linePalette=>
                    {
                        linePalette.ForEach(color =>
                        {
                            filePal.WriteByte((byte)color.ToR());
                            filePal.WriteByte((byte)color.ToG());
                            filePal.WriteByte((byte)color.ToB());
                        });
                });
            }
            using (var paletteFile = File.OpenWrite(fileName + ".rgb12"))
            {
                linePalettes.ForEach(linePalette =>
                {
                    linePalette.ForEach(color =>
                    {
                        var r = (byte)color.ToR();
                        var g = (byte)color.ToG();
                        var b = (byte)color.ToB();
                        paletteFile.WriteByte((byte)(r>>4));
                        paletteFile.WriteByte((byte)((g & 0xF0) | (b>>4)));
                    });
                });
            }
        }

        //public static void BitplaneWriteFile(string file, int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var indexMatrix = BitplaneCreateData( sourceMatrix,linePalettes,pixelWidthAlign,interleavedData);
        //    BitplaneWriteFileBpl(file, indexMatrix, linePalettes.Max(x => x.Count),interleavedData);
        //    BitplaneWriteFilePalette(file, linePalettes);
        //}
        public static void BitplaneWriteFile(string file, int[,] sourceMatrix, List<int> paletteSrc, WidthAlignMode pixelWidthAlign, bool interleavedData)
        {
            var indexMatrix = BitplaneCreateData(sourceMatrix, paletteSrc, pixelWidthAlign, interleavedData);
            BitplaneWriteFileBpl(file, indexMatrix, paletteSrc.Count, interleavedData);
            BitplaneWriteFilePalette(file, new List<List<int>>() { paletteSrc });
        }

    }
}
