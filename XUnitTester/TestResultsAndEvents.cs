using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System.Collections.Concurrent;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests for the error results of the transformations and for the forwarding of the
    /// progress events of nested transformations.
    /// </summary>
    public class TestResultsAndEvents
    {
        /// <summary>
        /// Builds an image with a horizontal gradient: one distinct color per column.
        /// </summary>
        static ImageData CreateGradient(int width, int height)
        {
            var matrix = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    matrix[r, c] = ColorIntExt.FromRGB(c * 255 / Math.Max(1, width - 1), r * 255 / Math.Max(1, height - 1), 128);
            return new ImageData().Create(matrix);
        }

        [Fact]
        public void ErrorResultCarriesContextImagesAndException()
        {
            var input = CreateGradient(4, 4);
            var ex = new InvalidOperationException("boom");

            var result = ColorTransformResult.CreateErrorResult("MyClass", "MyMethod", input, new ImageData(), "step failed", ex);

            Assert.False(result.IsSuccess);
            Assert.Equal("MyClass.MyMethod : step failed", result.Message);
            Assert.Same(input, result.DataIn);
            Assert.Null(result.DataOut);    // empty images are not reported
            Assert.Same(ex, result.Exception);
        }

        [Fact]
        public void ErrorResultUsesExceptionMessageWhenMessageIsMissing()
        {
            var result = ColorTransformResult.CreateErrorResult("MyClass", "MyMethod", null, null, new InvalidOperationException("boom"));
            Assert.Equal("MyClass.MyMethod : Exception : boom", result.Message);
        }

        [Fact]
        public void ErrorResultFromChainsNestedResult()
        {
            var input = CreateGradient(4, 4);
            var partial = CreateGradient(2, 2);
            var inner = ColorTransformResult.CreateErrorResult("Inner", "Run", input, partial, "inner failed", new Exception("cause"));

            var result = ColorTransformResult.CreateErrorResultFrom("Outer", "Run", input, "outer step failed", inner);

            Assert.False(result.IsSuccess);
            Assert.Equal("Outer.Run : outer step failed : Inner.Run : inner failed", result.Message);
            Assert.Same(input, result.DataIn);
            Assert.Same(partial, result.DataOut);
            Assert.Same(inner.Exception, result.Exception);
        }

        [Fact]
        public void UnsupportedVideoModeReturnsContextualErrorResult()
        {
            var source = CreateGradient(16, 8);
            var transform = new ColorTransformCbm64 { Options = new C64Options { VideoMode = ColorTransformCbm64.C64VideoMode.CharsetMulticolor } };

            var result = transform.CreateAndProcessColors(source);

            Assert.False(result.IsSuccess);
            Assert.Contains("ColorTransformBase.ProcessColors", result.Message);
            // Caught by the options validation, before any processing
            Assert.Contains("invalid options", result.Message);
            Assert.Contains("CharsetMulticolor is not implemented", result.Message);
            Assert.NotNull(result.DataIn);
        }

        [Fact]
        public void NestedTransformationEventsReachTheOuterTransformation()
        {
            // VGA = quantization + clustering: the clustering loop events must be re-raised by the VGA transformation
            var source = CreateGradient(64, 8);
            var transform = new ColorTransformVGA { Options = new VgaOptions { Reduction = new ClusteringOptions { MaxColors = 4 } } };
            var events = new ConcurrentBag<ColorProcessingEventArgs>();
            transform.ProcessPartial += (s, e) => events.Add(e);

            var result = transform.CreateAndProcessColors(source);

            Assert.True(result.IsSuccess, result.Message);
            Assert.Contains(events, e => e.ProcessingResults?.Message?.StartsWith("Loop") == true);
            Assert.All(events, e => Assert.Same(transform, e.ColorTransformInterface));
        }

        [Fact]
        public void TileManagerEventsReachTheOuterTransformation()
        {
            var source = CreateGradient(16, 16);
            var transform = new ColorTransformCbm64 { Options = new C64Options { VideoMode = ColorTransformCbm64.C64VideoMode.HiRes } };
            var events = new ConcurrentBag<ColorProcessingEventArgs>();
            transform.ProcessPartial += (s, e) => events.Add(e);

            var result = transform.CreateAndProcessColors(source);

            Assert.True(result.IsSuccess, result.Message);
            // 16 rows / 8 = 2 tile rows, each one reported with its progress
            Assert.Equal(2, events.Count(e => e.ProcessingResults?.Message?.StartsWith("Tile row") == true));
            Assert.Contains(events, e => e.CompletedPercent == 100.0);
        }
    }
}
