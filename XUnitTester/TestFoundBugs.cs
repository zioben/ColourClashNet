using ColourClashLib.Color.Transformation.Evolution;
using ColourClashNet.Color;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Regression tests for the bugs found during the naming and documentation review.
    /// </summary>
    public class TestFoundBugs
    {
        // Expose the protected error diffusion kernel of the tested ditherers
        class BurkesProbe : DitherBurkes { public double[,] Kernel => errorDiffusionMatrix; }
        class StuckiProbe : DitherStucki { public double[,] Kernel => errorDiffusionMatrix; }

        [Fact]
        public void GrayValueIsWeightedMeanClampedToByteRange()
        {
            var gray = GRAY.CreateFromIntRGB(ColorIntExt.FromRGB(90, 120, 150));
            Assert.Equal(120, gray.Y);

            var white = GRAY.CreateFromIntRGB(ColorIntExt.FromRGB(255, 255, 255));
            Assert.InRange(white.Y, 254, 255);
        }

        [Fact]
        public void BurkesKernelHasTwoRowsAndSumsToOne()
        {
            var probe = new BurkesProbe();
            probe.Create();
            Assert.Equal(2, probe.Kernel.GetLength(0));
            Assert.Equal(8.0 / 32, probe.Kernel[0, 3], 6);
            Assert.Equal(1.0, probe.Kernel.Cast<double>().Sum(), 6);
        }

        [Fact]
        public void StuckiKernelUsesStuckiWeights()
        {
            var probe = new StuckiProbe();
            probe.Create();
            Assert.Equal(8.0 / 42, probe.Kernel[0, 3], 6);
            Assert.Equal(1.0 / 42, probe.Kernel[2, 0], 6);
            Assert.Equal(1.0, probe.Kernel.Cast<double>().Sum(), 6);
        }

        [Fact]
        public void EhbClusterExposesHalfBrightColorThroughInterface()
        {
            IColorEvolution cluster = new ColorEvolutionAmigaEHB();
            cluster.SetClusterColor(ColorIntExt.FromRGB(192, 192, 192));
            int halfBright = ColorIntExt.FromRGB(96, 96, 96);

            Assert.Equal(halfBright, cluster.GetClusterColor(halfBright, ColorDistanceEvaluationMode.RGB));
            Assert.Equal(0.0, cluster.Distance(halfBright, ColorDistanceEvaluationMode.RGB));
            Assert.Equal(ColorIntExt.FromRGB(192, 192, 192), cluster.ClusterRGB);
        }

        [Fact]
        public void BitplaneConversionMapsLeftmostPixelToMostSignificantBit()
        {
            // 8 pixels, only the first one uses index 1 -> plane 0 byte = 1000_0000
            var indexMatrix = new byte[1, 8] { { 1, 0, 0, 0, 0, 0, 0, 0 } };
            var planes = ImageTools.BitplaneCreateData(indexMatrix, false, 2);
            Assert.Equal(0x80, planes[0, 0]);

            indexMatrix = new byte[1, 8] { { 0, 0, 0, 0, 0, 0, 0, 1 } };
            planes = ImageTools.BitplaneCreateData(indexMatrix, false, 2);
            Assert.Equal(0x01, planes[0, 0]);
        }
    }
}
