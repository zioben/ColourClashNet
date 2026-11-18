using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Threading;
using Xunit;

namespace XUnitTester
{
    public class TestColorTransformBase
    {
        static ImageData CreateImage(int width, int height, int rgb)
            => new ImageData().Create(MatrixTools.CreateMatrix(width, height, rgb));

        /// <summary>
        /// Transformation whose ExecuteTransform always fails
        /// </summary>
        class FailingTransform : ColorTransformBase
        {
            public const string ErrorMessage = "ExecuteTransform test failure";
            protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
                => ColorTransformResult.CreateErrorResult(ErrorMessage, new InvalidOperationException(ErrorMessage));
        }

        /// <summary>
        /// Transformation whose Create always fails (and therefore returns null)
        /// </summary>
        class FailingCreateTransform : ColorTransformBase
        {
            public override void Reset() => throw new InvalidOperationException("Reset test failure");
        }

        [Fact]
        public void Create_WithReferenceAfterCreateWithoutReference_DoesNotOverwriteSource()
        {
            int colSource = ColorIntExt.FromRGB(255, 0, 0);
            int colReference = ColorIntExt.FromRGB(0, 0, 255);

            var transform = new ColorTransformIdentity();

            // First call without reference image: reference defaults to source
            transform.Create(CreateImage(8, 8, colSource));

            // Second call with a distinct reference image
            transform.Create(CreateImage(8, 8, colSource), CreateImage(8, 8, colReference));

            Assert.Equal(colSource, transform.ImageSource.GetMatrix()[0, 0]);
            Assert.Equal(colReference, transform.ImageReference.GetMatrix()[0, 0]);
            Assert.NotSame(transform.ImageSource, transform.ImageReference);
        }

        [Fact]
        public void CreateAndProcessColors_WhenCreateFails_ReturnsErrorResult()
        {
            var transform = new FailingCreateTransform();

            var result = transform.CreateAndProcessColors(CreateImage(8, 8, 0));

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void CreateAndProcessColors_WithInvalidImage_ReturnsErrorResult()
        {
            var transform = new ColorTransformIdentity();

            var result = transform.CreateAndProcessColors(new ImageData());

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ProcessColors_WhenExecuteTransformFails_PropagatesError()
        {
            var transform = new FailingTransform();

            var result = transform.CreateAndProcessColors(CreateImage(8, 8, 0));

            Assert.False(result.IsSuccess);
            // The nested error is chained after the context of the failing operation
            Assert.StartsWith("ColorTransformBase.ProcessColors : ", result.Message);
            Assert.EndsWith(FailingTransform.ErrorMessage, result.Message);
            Assert.NotNull(result.DataIn);
            Assert.IsType<InvalidOperationException>(result.Exception);
        }

        [Fact]
        public void CreateAndProcessColors_Identity_Succeeds()
        {
            int col = ColorIntExt.FromRGB(10, 20, 30);
            var transform = new ColorTransformIdentity();

            var result = transform.CreateAndProcessColors(CreateImage(8, 8, col));

            Assert.True(result.IsSuccess);
            Assert.Equal(col, transform.ImageOutput.GetMatrix()[0, 0]);
        }
    }
}
