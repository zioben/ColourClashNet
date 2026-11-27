using System;
using System.Collections.Generic;
using System.Linq;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;

namespace ColourClashNet.Imaging.Shapes
{
    /// <summary>
    /// Kinds of region of interest.
    /// </summary>
    public enum RoiShapeType
    {
        /// <summary>Axis aligned rectangle.</summary>
        Rectangle,
        /// <summary>Ellipse inscribed in a rectangle.</summary>
        Ellipse,
        /// <summary>Closed polygon defined by its vertices.</summary>
        Polygon,
        /// <summary>Closed free hand path.</summary>
        Freehand,
    }

    /// <summary>
    /// Region of interest drawn on an image, in image coordinates (pixel (x, y) covers [x, x+1) x [y, y+1)).
    /// <para/>
    /// Shapes are immutable: editing operations (<see cref="Translate"/>, <see cref="MoveHandle"/>) return a new shape.
    /// Being in image coordinates, a shape follows the zoom and the scroll of the view that draws it.
    /// </summary>
    public abstract record RoiShape
    {
        /// <summary>
        /// Kind of shape.
        /// </summary>
        public abstract RoiShapeType ShapeType { get; }

        /// <summary>
        /// Smallest rectangle containing the shape.
        /// </summary>
        public abstract RectangleF Bounds { get; }

        /// <summary>
        /// Closed outline used to draw the shape (the last point connects to the first one).
        /// </summary>
        public abstract IReadOnlyList<PointF> Outline { get; }

        /// <summary>
        /// Editing handles: box corners and edge midpoints, or polygon vertices. Free hand shapes have none.
        /// </summary>
        public abstract IReadOnlyList<PointF> Handles { get; }

        /// <summary>
        /// True when the point (<paramref name="x"/>, <paramref name="y"/>) is inside the shape.
        /// </summary>
        public abstract bool Contains(double x, double y);

        /// <summary>
        /// Returns the shape moved by (<paramref name="dx"/>, <paramref name="dy"/>).
        /// </summary>
        public abstract RoiShape Translate(float dx, float dy);

        /// <summary>
        /// Returns the shape with the handle <paramref name="index"/> moved to <paramref name="position"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Invalid handle index.</exception>
        public abstract RoiShape MoveHandle(int index, PointF position);

        /// <summary>
        /// True when the shape covers at least one pixel in width and height.
        /// </summary>
        public virtual bool IsValid => Bounds.Width >= 1 && Bounds.Height >= 1;

        /// <summary>
        /// Cuts the shape out of <paramref name="image"/>: the result is the part of the image inside <see cref="Bounds"/>,
        /// and the pixels outside the shape are set to <paramref name="outsideColor"/>.
        /// <para/>
        /// A pixel belongs to the shape when its center is inside it.
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="outsideColor">Value of the pixels outside the shape (e.g. the background color, or a mask tag)</param>
        /// <returns>The extracted image, or null when the shape does not overlap the image.</returns>
        public ImageData? ExtractFrom(ImageData image, int outsideColor)
        {
            ImageData.AssertValid(image);
            var bounds = Bounds;
            int left = Math.Max(0, (int)Math.Floor(bounds.Left));
            int top = Math.Max(0, (int)Math.Floor(bounds.Top));
            int right = Math.Min(image.Width, (int)Math.Ceiling(bounds.Right));
            int bottom = Math.Min(image.Height, (int)Math.Ceiling(bounds.Bottom));
            if (right <= left || bottom <= top)
                return null;

            var source = image.matrix;
            var result = new int[bottom - top, right - left];
            for (int r = top; r < bottom; r++)
            {
                for (int c = left; c < right; c++)
                {
                    result[r - top, c - left] = Contains(c + 0.5, r + 0.5) ? source[r, c] : outsideColor;
                }
            }
            return new ImageData().Create(result);
        }

        /// <summary>
        /// Even-odd point in polygon test.
        /// </summary>
        protected static bool PolygonContains(IReadOnlyList<PointF> points, double x, double y)
        {
            bool inside = false;
            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                var a = points[i];
                var b = points[j];
                // The horizontal ray from (x, y) crosses the edge a-b
                if ((a.Y > y) != (b.Y > y) && x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>
        /// Smallest rectangle containing <paramref name="points"/>.
        /// </summary>
        protected static RectangleF BoundsOf(IReadOnlyList<PointF> points)
        {
            if (points.Count == 0)
                return RectangleF.Empty;
            float left = points.Min(p => p.X), top = points.Min(p => p.Y);
            return new RectangleF(left, top, points.Max(p => p.X) - left, points.Max(p => p.Y) - top);
        }

        /// <summary>
        /// Rectangle with the two opposite corners <paramref name="a"/> and <paramref name="b"/>, in any order.
        /// </summary>
        public static RectangleF RectangleFromCorners(PointF a, PointF b)
            => new RectangleF(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
    }

    /// <summary>
    /// Base of the shapes defined by a box (rectangle and ellipse).
    /// <para/>
    /// Handles, clockwise from the top left corner: 0 top left, 1 top, 2 top right, 3 right,
    /// 4 bottom right, 5 bottom, 6 bottom left, 7 left.
    /// </summary>
    /// <param name="Box">Bounding box of the shape</param>
    public abstract record BoxRoiShape(RectangleF Box) : RoiShape
    {
        /// <inheritdoc/>
        public override RectangleF Bounds => Box;

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Handles
        {
            get
            {
                float l = Box.Left, t = Box.Top, r = Box.Right, b = Box.Bottom;
                float cx = (l + r) / 2, cy = (t + b) / 2;
                return new[]
                {
                    new PointF(l, t), new PointF(cx, t), new PointF(r, t), new PointF(r, cy),
                    new PointF(r, b), new PointF(cx, b), new PointF(l, b), new PointF(l, cy),
                };
            }
        }

        /// <inheritdoc/>
        public override RoiShape MoveHandle(int index, PointF position)
        {
            float l = Box.Left, t = Box.Top, r = Box.Right, b = Box.Bottom;
            switch (index)
            {
                case 0: l = position.X; t = position.Y; break;
                case 1: t = position.Y; break;
                case 2: r = position.X; t = position.Y; break;
                case 3: r = position.X; break;
                case 4: r = position.X; b = position.Y; break;
                case 5: b = position.Y; break;
                case 6: l = position.X; b = position.Y; break;
                case 7: l = position.X; break;
                default: throw new ArgumentOutOfRangeException(nameof(index), $"handle {index} does not exist");
            }
            // Dragging a handle over the opposite side flips the box instead of producing a negative size
            return WithBox(RectangleFromCorners(new PointF(l, t), new PointF(r, b)));
        }

        /// <inheritdoc/>
        public override RoiShape Translate(float dx, float dy)
            => WithBox(new RectangleF(Box.X + dx, Box.Y + dy, Box.Width, Box.Height));

        /// <summary>
        /// Returns the same kind of shape on another box.
        /// </summary>
        protected abstract RoiShape WithBox(RectangleF box);
    }

    /// <summary>
    /// Rectangular region of interest.
    /// </summary>
    public sealed record RectangleRoiShape(RectangleF Box) : BoxRoiShape(Box)
    {
        /// <inheritdoc/>
        public override RoiShapeType ShapeType => RoiShapeType.Rectangle;

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Outline => new[]
        {
            new PointF(Box.Left, Box.Top), new PointF(Box.Right, Box.Top),
            new PointF(Box.Right, Box.Bottom), new PointF(Box.Left, Box.Bottom),
        };

        /// <inheritdoc/>
        public override bool Contains(double x, double y)
            => x >= Box.Left && x < Box.Right && y >= Box.Top && y < Box.Bottom;

        /// <inheritdoc/>
        protected override RoiShape WithBox(RectangleF box) => new RectangleRoiShape(box);
    }

    /// <summary>
    /// Elliptic region of interest, inscribed in <see cref="BoxRoiShape.Box"/>.
    /// </summary>
    public sealed record EllipseRoiShape(RectangleF Box) : BoxRoiShape(Box)
    {
        /// <summary>
        /// Number of points of <see cref="Outline"/>.
        /// </summary>
        const int OutlinePoints = 72;

        /// <inheritdoc/>
        public override RoiShapeType ShapeType => RoiShapeType.Ellipse;

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Outline
        {
            get
            {
                double cx = Box.Left + Box.Width / 2.0, cy = Box.Top + Box.Height / 2.0;
                double rx = Box.Width / 2.0, ry = Box.Height / 2.0;
                var points = new PointF[OutlinePoints];
                for (int i = 0; i < OutlinePoints; i++)
                {
                    double angle = 2 * Math.PI * i / OutlinePoints;
                    points[i] = new PointF((float)(cx + rx * Math.Cos(angle)), (float)(cy + ry * Math.Sin(angle)));
                }
                return points;
            }
        }

        /// <inheritdoc/>
        public override bool Contains(double x, double y)
        {
            double rx = Box.Width / 2.0, ry = Box.Height / 2.0;
            if (rx <= 0 || ry <= 0)
                return false;
            double dx = (x - (Box.Left + rx)) / rx;
            double dy = (y - (Box.Top + ry)) / ry;
            return dx * dx + dy * dy <= 1.0;
        }

        /// <inheritdoc/>
        protected override RoiShape WithBox(RectangleF box) => new EllipseRoiShape(box);
    }

    /// <summary>
    /// Polygonal region of interest: every vertex is an editing handle.
    /// </summary>
    /// <param name="Points">Vertices, in order (at least 3 for a valid polygon)</param>
    public sealed record PolygonRoiShape(IReadOnlyList<PointF> Points) : RoiShape
    {
        /// <inheritdoc/>
        public override RoiShapeType ShapeType => RoiShapeType.Polygon;

        /// <inheritdoc/>
        public override RectangleF Bounds => BoundsOf(Points);

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Outline => Points;

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Handles => Points;

        /// <inheritdoc/>
        public override bool IsValid => Points.Count >= 3 && base.IsValid;

        /// <inheritdoc/>
        public override bool Contains(double x, double y) => PolygonContains(Points, x, y);

        /// <inheritdoc/>
        public override RoiShape Translate(float dx, float dy)
            => new PolygonRoiShape(Points.Select(p => new PointF(p.X + dx, p.Y + dy)).ToArray());

        /// <inheritdoc/>
        public override RoiShape MoveHandle(int index, PointF position)
        {
            if (index < 0 || index >= Points.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"vertex {index} does not exist");
            var points = Points.ToArray();
            points[index] = position;
            return new PolygonRoiShape(points);
        }
    }

    /// <summary>
    /// Free hand region of interest: a closed path that can be moved, but has no editing handles.
    /// </summary>
    /// <param name="Points">Points of the path, in order</param>
    public sealed record FreehandRoiShape(IReadOnlyList<PointF> Points) : RoiShape
    {
        /// <inheritdoc/>
        public override RoiShapeType ShapeType => RoiShapeType.Freehand;

        /// <inheritdoc/>
        public override RectangleF Bounds => BoundsOf(Points);

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Outline => Points;

        /// <inheritdoc/>
        public override IReadOnlyList<PointF> Handles => Array.Empty<PointF>();

        /// <inheritdoc/>
        public override bool IsValid => Points.Count >= 3 && base.IsValid;

        /// <inheritdoc/>
        public override bool Contains(double x, double y) => PolygonContains(Points, x, y);

        /// <inheritdoc/>
        public override RoiShape Translate(float dx, float dy)
            => new FreehandRoiShape(Points.Select(p => new PointF(p.X + dx, p.Y + dy)).ToArray());

        /// <inheritdoc/>
        public override RoiShape MoveHandle(int index, PointF position)
            => throw new ArgumentOutOfRangeException(nameof(index), "free hand shapes have no handles");
    }
}
