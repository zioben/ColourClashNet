using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests of the processing pipeline of <see cref="ColorTransformBase"/>: single entry point, events and metrics.
    /// </summary>
    public class TestColorTransformBase
    {
        static ImageData CreateImage(int width, int height, int rgb)
            => new ImageData().Create(MatrixTools.CreateMatrix(width, height, rgb));

        /// <summary>
        /// Transformation whose ExecuteTransform always fails.
        /// </summary>
        class FailingTransform : ColorTransformBase<IdentityOptions>
        {
            public const string ErrorMessage = "ExecuteTransform test failure";
            protected override ColorTransformResult ExecuteTransform(CancellationToken token)
                => ColorTransformResult.CreateErrorResult(ErrorMessage, new InvalidOperationException(ErrorMessage));
        }

        /// <summary>
        /// Transformation that bypasses the final dithering on request and records the bypass state it starts with.
        /// </summary>
        class BypassTransform : ColorTransformBase<FastOptions>
        {
            public bool Bypass { get; set; }
            public bool BypassAtStart { get; private set; }
            protected override ColorTransformResult ExecuteTransform(CancellationToken token)
            {
                BypassAtStart = BypassDithering;
                if (Bypass)
                    BypassDithering = true;
                return ColorTransformResult.CreateValidResult(ImageSource, new ImageData().Create(ImageSource));
            }
        }

        [Fact]
        public void Process_WithReference_KeepsSourceAndReferenceSeparate()
        {
            int colSource = ColorIntExt.FromRGB(255, 0, 0);
            int colReference = ColorIntExt.FromRGB(0, 0, 255);
            var transform = new ColorTransformIdentity();

            // First run without reference image: the reference is the source
            transform.Process(CreateImage(8, 8, colSource));
            Assert.Same(transform.ImageSource, transform.ImageReference);

            // Second run with a distinct reference image
            transform.Process(CreateImage(8, 8, colSource), CreateImage(8, 8, colReference));

            Assert.Equal(colSource, transform.ImageSource.GetMatrix()[0, 0]);
            Assert.Equal(colReference, transform.ImageReference.GetMatrix()[0, 0]);
            Assert.NotSame(transform.ImageSource, transform.ImageReference);
        }

        [Fact]
        public void Process_WithInvalidImage_ReturnsErrorResult()
        {
            var transform = new ColorTransformIdentity();

            var result = transform.Process(new ImageData());

            Assert.False(result.IsSuccess);
            Assert.Contains("invalid source image", result.Message);
        }

        [Fact]
        public void Process_WhenExecuteTransformFails_PropagatesError()
        {
            var transform = new FailingTransform();

            var result = transform.Process(CreateImage(8, 8, 0));

            Assert.False(result.IsSuccess);
            // The nested error is chained after the context of the failing step
            Assert.StartsWith("ColorTransformBase.Transform : ", result.Message);
            Assert.EndsWith(FailingTransform.ErrorMessage, result.Message);
            Assert.NotNull(result.DataIn);
            Assert.IsType<InvalidOperationException>(result.Exception);
        }

        [Fact]
        public void Process_Identity_ReturnsTheImageAndTheMetrics()
        {
            int col = ColorIntExt.FromRGB(10, 20, 30);
            var transform = new ColorTransformIdentity();

            var result = transform.Process(CreateImage(8, 8, col));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(col, result.DataOut.GetMatrix()[0, 0]);
            Assert.Same(result.DataOut, transform.ImageOutput);
            Assert.Equal(0.0, result.TransformationError);
            Assert.Equal(transform.TransformationError, result.TransformationError);
            Assert.False(double.IsNaN(result.ProcessingTimeMilliseconds));
        }

        [Fact]
        public async Task ProcessAsync_ReturnsTheSameResult()
        {
            int col = ColorIntExt.FromRGB(1, 2, 3);
            var transform = new ColorTransformIdentity();

            var result = await transform.ProcessAsync(CreateImage(4, 4, col));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(col, result.DataOut.GetMatrix()[0, 0]);
        }

        [Fact]
        public void Events_AreRaisedInOrder()
        {
            var transform = new ColorTransformFast
            {
                Options = new FastOptions { MaxColors = 2, Dithering = new DitherOptions { DitheringType = ColorDithering.FloydSteinberg } },
            };
            var events = new List<string>();
            transform.StartProcessing += (s, e) => events.Add("StartProcessing");
            transform.Creating += (s, e) => events.Add("Creating");
            transform.Created += (s, e) => events.Add("Created");
            transform.Processing += (s, e) => events.Add("Processing");
            transform.Dithering += (s, e) => events.Add("Dithering");
            transform.Completed += (s, e) => events.Add("Completed");
            var matrix = new int[1, 4] { { 0x000000, 0x404040, 0x808080, 0xFFFFFF } };

            var result = transform.Process(new ImageData().Create(matrix));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(new[] { "StartProcessing", "Creating", "Created", "Processing", "Dithering", "Completed" }, events);
        }

        [Fact]
        public void StartProcessing_CanChangeTheOptionsOfTheRun()
        {
            var transform = new ColorTransformFast { Options = new FastOptions { MaxColors = 16 } };
            transform.StartProcessing += (s, e) => e.Transformation.SetOptions(new FastOptions { MaxColors = 2 });
            var matrix = new int[1, 4] { { 0x000000, 0x404040, 0x808080, 0xFFFFFF } };

            var result = transform.Process(new ImageData().Create(matrix));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(2, result.DataOut.Colors);
        }

        [Fact]
        public void CancellingTheEventTokenSource_StopsTheRun_AndCompletedIsStillRaised()
        {
            var transform = new ColorTransformIdentity();
            transform.StartProcessing += (s, e) => e.CancellationTokenSource!.Cancel();
            ColorTransformResult? completed = null;
            transform.Completed += (s, e) => completed = e.ProcessingResults;

            var result = transform.Process(CreateImage(4, 4, 0));

            Assert.False(result.IsSuccess);
            Assert.Contains("cancelled", result.Message);
            Assert.Same(result, completed);
        }

        [Fact]
        public void Completed_IsRaised_AlsoOnError()
        {
            var transform = new FailingTransform();
            ColorTransformResult? completed = null;
            transform.Completed += (s, e) => completed = e.ProcessingResults;

            var result = transform.Process(CreateImage(4, 4, 0));

            Assert.False(result.IsSuccess);
            Assert.Same(result, completed);
        }

        [Fact]
        public void FailingEventHandler_DoesNotBreakTheRun()
        {
            var transform = new ColorTransformIdentity();
            transform.Processing += (s, e) => throw new InvalidOperationException("handler failure");

            var result = transform.Process(CreateImage(4, 4, 0));

            Assert.True(result.IsSuccess, result.Message);
        }

        [Fact]
        public void BypassDithering_IsResetAtEveryRun()
        {
            var transform = new BypassTransform
            {
                Bypass = true,
                Options = new FastOptions { Dithering = new DitherOptions { DitheringType = ColorDithering.FloydSteinberg } },
            };
            int ditheringEvents = 0;
            transform.Dithering += (s, e) => ditheringEvents++;

            transform.Process(CreateImage(4, 4, 0x808080));
            Assert.True(transform.BypassDithering);
            Assert.Equal(0, ditheringEvents);

            transform.Bypass = false;
            transform.Process(CreateImage(4, 4, 0x808080));
            Assert.False(transform.BypassAtStart);
            Assert.False(transform.BypassDithering);
            Assert.Equal(1, ditheringEvents);
        }

        [Fact]
        public void Type_ComesFromTheOptions()
        {
            Assert.Equal(ColorTransformType.ColorReductionClustering, new ColorTransformClustering().Type);
            Assert.Equal(ColorTransformType.ColorReductionHam, new ColorTransformCbmAmiga().Type);
            Assert.Equal(new ZxOptions().TransformType, new ColorTransformZxSpectrum().Type);
        }

        [Fact]
        public void Dispose_RaisesDestroyingAndDestroyed_AndBlocksNewRuns()
        {
            var transform = new ColorTransformIdentity();
            var events = new List<string>();
            transform.Destroying += (s, e) => events.Add("Destroying");
            transform.Destroyed += (s, e) => events.Add("Destroyed");

            transform.Dispose();
            transform.Dispose();   // a second call does nothing

            Assert.Equal(new[] { "Destroying", "Destroyed" }, events);
            Assert.Throws<ObjectDisposedException>(() => transform.Process(CreateImage(4, 4, 0)));
        }
    }
}
