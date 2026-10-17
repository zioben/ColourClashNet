using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashProcessing.Blob;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Xunit;

namespace XUnitTester
{
    public class TestBlob
    {
        [Fact]
        public void TestBlobDetection()
        {
            int colBkg = ColorIntExt.FromRGB(255, 0, 255);
            int colFgd = ColorIntExt.FromRGB(128, 128, 128);

            // Create an image with 4 blobs
            var mat1 = MatrixTools.CreateMatrix(320, 200, colBkg);
            var mat2 = MatrixTools.CreateMatrix(32, 24, colFgd);
            for( int i = 0; i < 10; i++)
            {
                MatrixTools.Blit(mat2, mat1, 0, 0, i * 10, i * 8, 32, 24);
            }
            MatrixTools.Blit(mat2, mat1, 0, 0, 200, 0, 32, 24);
            MatrixTools.Blit(mat2, mat1, 0, 0, 200, 50, 32, 24);
            MatrixTools.Blit(mat2, mat1, 0, 0, 200, 100, 32, 24);

            ImageData oImage = new ImageData().Create(mat1);

            Directory.CreateDirectory("C:\\Temp");
            ImageTools.SaveImage(oImage, "C:\\Temp\\TestBlobIn");

            ColourClashProcessing.Blob.Blob blob = new ColourClashProcessing.Blob.Blob()
                .WithThreshold(ColorIntExt.FromRGB(255, 0, 255), 0, false)
                .WithFiltering(10, 1000000, 10, 1000000, 10, 1000000)
                .WithProcessing(ColourClashProcessing.Blob.BlobConnectionModes.FourConnected);

            var result = blob.Process(oImage);

            var oImageOut = blob.GetShapeImage();
            ImageTools.SaveImage(oImageOut, "C:\\Temp\\TestBlobOut");

            var blobSegmetedOut = blob.GetBlobSegmentedImage();
            ImageTools.SaveImage(blobSegmetedOut, $"C:\\Temp\\TestBlobOut_Segmented");

        }

        public static IEnumerable<object[]> GetConnectionModes() =>
          Enum.GetValues<BlobConnectionModes>().Select(t => new object[] { t });


        [Theory]
        [MemberData(nameof(GetConnectionModes))]
        public void TestBlobDetectionModes(BlobConnectionModes connectionMode)
        {
            int colBkg = ColorIntExt.FromRGB(255, 0, 255);
            int colFgd = ColorIntExt.FromRGB(128, 128, 128);
            int W = 320;
            int H = 200;

            // Crate te most difficult blob to process
            var mat1 = MatrixTools.CreateMatrix(W, H, colBkg);
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        mat1[y, x] = colFgd;
                    }
                    else
                    {
                        mat1[y, x] = colBkg;
                    }
                }
            }
            ImageData oImage = new ImageData().Create(mat1);

            Directory.CreateDirectory("C:\\Temp");
            ImageTools.SaveImage(oImage, $"C:\\Temp\\{connectionMode}_TestBlobIn.png", ImageSaveFormat.Png);

            ColourClashProcessing.Blob.Blob blob = new ColourClashProcessing.Blob.Blob()
                .WithThreshold(ColorIntExt.FromRGB(255, 0, 255), 0, false)
                .WithFiltering(0,0,0,0,0,0)
                .WithProcessing(ColourClashProcessing.Blob.BlobConnectionModes.FourConnected);

            blob.WithProcessing(connectionMode);
            var result = blob.Process(oImage);

            var imageShapeOut = blob.GetShapeImage();
            ImageTools.SaveImage(imageShapeOut, $"C:\\Temp\\{connectionMode}_ShapeOut_{blob.BlobList.Count}");

            var imageBlobOut = blob.GetBlobImage();
            ImageTools.SaveImage(imageBlobOut, $"C:\\Temp\\{connectionMode}_BlobOut_{blob.BlobList.Count}");

            var blobSegmetedOut = blob.GetBlobSegmentedImage();
            ImageTools.SaveImage(blobSegmetedOut, $"C:\\Temp\\{connectionMode}_BlobSegment_{blob.BlobList.Count}");
        }

        [Fact]
        public void TestBlobRyu()
        {
            int colBkg = ColorIntExt.FromRGB(255, 0, 255);
            int colFgd = ColorIntExt.FromRGB(128, 128, 128);

            string fileName = "ChunLi";
            // Create an image with 4 blobs

            Directory.CreateDirectory($"C:\\Temp\\{fileName}\\Frames");
            ImageData oImage = ImageTools.LoadImage($"C:\\Temp\\{fileName}.png");
            ImageTools.SaveImage(oImage, $"C:\\Temp\\{fileName}\\Test{fileName}_BlobIn");

            ColourClashProcessing.Blob.Blob blob = new ColourClashProcessing.Blob.Blob()
                .WithThreshold(ColorIntExt.FromRGB(255, 0, 255), 0, false)
                .WithFiltering(10, 1000000, 10, 1000000, 10, 1000000)
                .WithProcessing(ColourClashProcessing.Blob.BlobConnectionModes.FourConnected);

            var result = blob.Process(oImage);

            var shapeOut = blob.GetShapeImage();
            ImageTools.SaveImage(shapeOut, $"C:\\Temp\\{fileName}\\Test{fileName}_ShapeOut");

            var blobOut = blob.GetBlobImage();
            ImageTools.SaveImage(blobOut, $"C:\\Temp\\{fileName}\\Test{fileName}_BlobOut");

            var blobSegmetedOut = blob.GetBlobSegmentedImage();
            ImageTools.SaveImage(blobSegmetedOut, $"C:\\Temp\\{fileName}\\Test{fileName}_SegmentedOut");


            BlobTools.ExportImages(blob, $"C:\\Temp\\{fileName}\\Frames", $"{fileName}");


        }
    }
}
