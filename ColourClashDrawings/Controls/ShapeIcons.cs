using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ColourClashNet.Controls
{
    /// <summary>
    /// Toolbar icons of the region of interest tools, drawn at run time (no image files needed).
    /// </summary>
    public static class ShapeIcons
    {
        /// <summary>
        /// Default icon size, in pixels.
        /// </summary>
        public const int DefaultSize = 16;

        static readonly System.Drawing.Color Outline = System.Drawing.Color.FromArgb(40, 40, 40);
        static readonly System.Drawing.Color Fill = System.Drawing.Color.FromArgb(255, 214, 0);

        /// <summary>
        /// Free hand path.
        /// </summary>
        public static Bitmap Freehand(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            using var path = new GraphicsPath();
            path.AddBezier(P(s, 0.12, 0.70), P(s, 0.10, 0.15), P(s, 0.55, 0.05), P(s, 0.55, 0.40));
            path.AddBezier(P(s, 0.55, 0.40), P(s, 0.55, 0.75), P(s, 0.95, 0.70), P(s, 0.88, 0.30));
            using var pen = new Pen(Outline, s / 10f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawPath(pen, path);
            // Pen tip
            using var brush = new SolidBrush(Fill);
            g.FillEllipse(brush, new RectangleF(P(s, 0.06, 0.64), new SizeF(s * 0.16f, s * 0.16f)));
        });

        /// <summary>
        /// Rectangle.
        /// </summary>
        public static Bitmap Rectangle(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            var rect = new RectangleF(P(s, 0.12, 0.22), new SizeF(s * 0.76f, s * 0.56f));
            FillAndOutline(g, s, path => path.AddRectangle(rect));
        });

        /// <summary>
        /// Ellipse.
        /// </summary>
        public static Bitmap Ellipse(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            var rect = new RectangleF(P(s, 0.08, 0.20), new SizeF(s * 0.84f, s * 0.60f));
            FillAndOutline(g, s, path => path.AddEllipse(rect));
        });

        /// <summary>
        /// Polygon, with its vertices.
        /// </summary>
        public static Bitmap Polygon(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            var points = new[] { P(s, 0.15, 0.80), P(s, 0.25, 0.20), P(s, 0.70, 0.12), P(s, 0.88, 0.55), P(s, 0.55, 0.88) };
            FillAndOutline(g, s, path => path.AddPolygon(points));
            using var brush = new SolidBrush(Outline);
            float h = s * 0.09f;
            foreach (var p in points)
                g.FillRectangle(brush, p.X - h, p.Y - h, 2 * h, 2 * h);
        });

        /// <summary>
        /// Reset: a red cross.
        /// </summary>
        public static Bitmap Reset(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            using var pen = new Pen(System.Drawing.Color.FromArgb(200, 30, 30), s / 7f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(pen, P(s, 0.22, 0.22), P(s, 0.78, 0.78));
            g.DrawLine(pen, P(s, 0.78, 0.22), P(s, 0.22, 0.78));
        });

        /// <summary>
        /// Extract: a dashed selection with an arrow coming out of it.
        /// </summary>
        public static Bitmap Extract(int size = DefaultSize) => Draw(size, (g, s) =>
        {
            using var dash = new Pen(Outline, s / 14f) { DashStyle = DashStyle.Dash };
            g.DrawRectangle(dash, s * 0.08f, s * 0.30f, s * 0.58f, s * 0.58f);
            using var arrow = new Pen(System.Drawing.Color.FromArgb(0, 120, 215), s / 8f)
            {
                StartCap = LineCap.Round,
                CustomEndCap = new AdjustableArrowCap(3, 3),
            };
            g.DrawLine(arrow, P(s, 0.38, 0.60), P(s, 0.86, 0.12));
        });

        /// <summary>
        /// Point at the relative position (<paramref name="x"/>, <paramref name="y"/>) of an icon of size <paramref name="size"/>.
        /// </summary>
        static PointF P(int size, double x, double y) => new((float)(x * size), (float)(y * size));

        /// <summary>
        /// Fills a shape with the ROI color and outlines it.
        /// </summary>
        static void FillAndOutline(Graphics g, int size, Action<GraphicsPath> build)
        {
            using var path = new GraphicsPath();
            build(path);
            using var brush = new SolidBrush(System.Drawing.Color.FromArgb(150, Fill));
            g.FillPath(brush, path);
            using var pen = new Pen(Outline, size / 12f);
            g.DrawPath(pen, path);
        }

        /// <summary>
        /// Creates a transparent bitmap and draws on it with anti aliasing.
        /// </summary>
        static Bitmap Draw(int size, Action<Graphics, int> draw)
        {
            var bitmap = new Bitmap(size, size);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);
            draw(g, size);
            return bitmap;
        }
    }
}
