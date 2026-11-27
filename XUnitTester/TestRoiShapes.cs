using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Imaging.Shapes;
using System.Drawing;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests of the region of interest shapes: containment, editing and extraction.
    /// </summary>
    public class TestRoiShapes
    {
        /// <summary>
        /// Image whose pixel value is its index (row * width + column + 1).
        /// </summary>
        static ImageData Numbered(int width, int height)
        {
            var matrix = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    matrix[r, c] = r * width + c + 1;
            return new ImageData().Create(matrix);
        }

        [Fact]
        public void Rectangle_ContainsItsPixels_AndExtractsThem()
        {
            var shape = new RectangleRoiShape(new RectangleF(2, 1, 3, 2));

            Assert.True(shape.Contains(2.5, 1.5));
            Assert.False(shape.Contains(5.5, 1.5));   // right edge excluded
            var extracted = shape.ExtractFrom(Numbered(8, 4), -1)!;

            Assert.Equal(3, extracted.Width);
            Assert.Equal(2, extracted.Height);
            Assert.Equal(1 * 8 + 2 + 1, extracted.GetMatrix()[0, 0]);
            Assert.Equal(2 * 8 + 4 + 1, extracted.GetMatrix()[1, 2]);
        }

        [Fact]
        public void Ellipse_MasksTheCorners()
        {
            var shape = new EllipseRoiShape(new RectangleF(0, 0, 9, 9));
            int outside = ColorIntExt.FromRGB(255, 0, 255);

            var extracted = shape.ExtractFrom(Numbered(9, 9), outside)!.GetMatrix();

            Assert.Equal(outside, extracted[0, 0]);
            Assert.Equal(outside, extracted[8, 8]);
            Assert.Equal(4 * 9 + 4 + 1, extracted[4, 4]);   // center kept
        }

        [Fact]
        public void Polygon_UsesTheEvenOddRule()
        {
            // Triangle with the right angle at the top left corner
            var shape = new PolygonRoiShape(new[] { new PointF(0, 0), new PointF(10, 0), new PointF(0, 10) });

            Assert.True(shape.Contains(2, 2));
            Assert.False(shape.Contains(8, 8));
            Assert.True(shape.IsValid);
            Assert.False(new PolygonRoiShape(new[] { new PointF(0, 0), new PointF(5, 5) }).IsValid);
        }

        [Fact]
        public void BoxHandles_ResizeTheBox_AndFlipPastTheOppositeSide()
        {
            var shape = new RectangleRoiShape(new RectangleF(10, 10, 20, 10));

            var resized = Assert.IsType<RectangleRoiShape>(shape.MoveHandle(4, new PointF(40, 30)));   // bottom right
            Assert.Equal(new RectangleF(10, 10, 30, 20), resized.Box);

            var widened = Assert.IsType<RectangleRoiShape>(shape.MoveHandle(7, new PointF(5, 99)));    // left edge: Y ignored
            Assert.Equal(new RectangleF(5, 10, 25, 10), widened.Box);

            var flipped = Assert.IsType<RectangleRoiShape>(shape.MoveHandle(3, new PointF(0, 15)));    // right edge past the left one
            Assert.Equal(new RectangleF(0, 10, 10, 10), flipped.Box);
            Assert.Equal(8, shape.Handles.Count);
        }

        [Fact]
        public void PolygonHandles_MoveOneVertex_AndTranslateMovesAll()
        {
            var shape = new PolygonRoiShape(new[] { new PointF(0, 0), new PointF(10, 0), new PointF(0, 10) });

            var edited = Assert.IsType<PolygonRoiShape>(shape.MoveHandle(1, new PointF(20, 5)));
            Assert.Equal(new PointF(20, 5), edited.Points[1]);
            Assert.Equal(new PointF(0, 0), edited.Points[0]);

            var moved = Assert.IsType<PolygonRoiShape>(shape.Translate(3, 4));
            Assert.Equal(new PointF(3, 4), moved.Points[0]);
            Assert.Equal(new PointF(0, 0), shape.Points[0]);   // shapes are immutable
        }

        [Fact]
        public void Freehand_HasNoHandles_ButMoves()
        {
            var shape = new FreehandRoiShape(new[] { new PointF(0, 0), new PointF(4, 0), new PointF(4, 4), new PointF(0, 4) });

            Assert.Empty(shape.Handles);
            Assert.Throws<ArgumentOutOfRangeException>(() => shape.MoveHandle(0, PointF.Empty));
            Assert.True(shape.Translate(10, 0).Contains(12, 2));
        }

        [Fact]
        public void Extract_ClipsToTheImage_AndReturnsNullOutsideIt()
        {
            var image = Numbered(4, 4);

            var clipped = new RectangleRoiShape(new RectangleF(-2, -2, 4, 4)).ExtractFrom(image, 0)!;
            Assert.Equal(2, clipped.Width);
            Assert.Equal(2, clipped.Height);
            Assert.Equal(1, clipped.GetMatrix()[0, 0]);

            Assert.Null(new RectangleRoiShape(new RectangleF(10, 10, 2, 2)).ExtractFrom(image, 0));
        }
    }
}
