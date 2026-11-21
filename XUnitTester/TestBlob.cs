using ColourClashProcessing.Threshold;
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

            ImageData image = new ImageData().Create(mat1);

            Directory.CreateDirectory("C:\\Temp");
            ImageTools.SaveImage(image, "C:\\Temp\\TestBlobIn");

            ColourClashProcessing.Blob.Blob blob = new ColourClashProcessing.Blob.Blob
            {
                Options = new BlobOptions
                {
                    Threshold = new ThresholdOptions { BackgroundColors = new[] { ColorIntExt.FromRGB(255, 0, 255) } },
                    MinArea = 10, MaxArea = 1000000,
                    MinWidth = 10, MaxWidth = 1000000,
                    MinHeight = 10, MaxHeight = 1000000,
                    ConnectionMode = BlobConnectionModes.FourConnected,
                },
            };

            var result = blob.Process(image);

            var imageOut = blob.GetShapeImage();
            ImageTools.SaveImage(imageOut, "C:\\Temp\\TestBlobOut");

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
            int width = 320;
            int height = 200;

            // Crate te most difficult blob to process
            var mat1 = MatrixTools.CreateMatrix(width, height, colBkg);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
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
            ImageData image = new ImageData().Create(mat1);

            Directory.CreateDirectory("C:\\Temp");
            ImageTools.SaveImage(image, $"C:\\Temp\\{connectionMode}_TestBlobIn.png", ImageSaveFormat.Png);

            ColourClashProcessing.Blob.Blob blob = new ColourClashProcessing.Blob.Blob
            {
                Options = new BlobOptions
                {
                    Threshold = new ThresholdOptions { BackgroundColors = new[] { ColorIntExt.FromRGB(255, 0, 255) } },
                    ConnectionMode = connectionMode,
                },
            };
            var result = blob.Process(image);

            var imageShapeOut = blob.GetShapeImage();
            ImageTools.SaveImage(imageShapeOut, $"C:\\Temp\\{connectionMode}_ShapeOut_{blob.BlobList.Count}");

            var imageBlobOut = blob.GetBlobImage();
            ImageTools.SaveImage(imageBlobOut, $"C:\\Temp\\{connectionMode}_BlobOut_{blob.BlobList.Count}");

            var blobSegmetedOut = blob.GetBlobSegmentedImage();
            ImageTools.SaveImage(blobSegmetedOut, $"C:\\Temp\\{connectionMode}_BlobSegment_{blob.BlobList.Count}");
        }
    }
}
