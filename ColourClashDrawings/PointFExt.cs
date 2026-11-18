using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Arithmetic and clipping extensions for <see cref="PointF"/>.
    /// </summary>
    public static class PointFExt
    {
        #region Math
        /// <summary>
        /// Component-wise sum.
        /// </summary>
        public static PointF Add(this PointF pa, PointF pb)
        {
            return new PointF(pa.X + pb.X, pa.Y + pb.Y);
        }
        /// <summary>
        /// Component-wise difference.
        /// </summary>
        public static PointF Sub(this PointF pa, PointF pb)
        {
            return new PointF(pa.X - pb.X, pa.Y - pb.Y);
        }
        /// <summary>
        /// Component-wise (point to point) product.
        /// </summary>
        public static PointF MulP2P(this PointF pa, PointF pb)
        {
            return new PointF(pa.X * pb.X, pa.Y * pb.Y);
        }
        /// <summary>
        /// Component-wise (point to point) division; a non-positive divisor gives 0.
        /// </summary>
        public static PointF DivP2P(this PointF pa, PointF pb)
        {
            var x = pb.X > 0 ? pa.X / pb.X : 0;
            var y = pb.Y > 0 ? pa.Y / pb.Y : 0;
            return new PointF(x, y);
        }

        #endregion

        #region Clip
        /// <summary>
        /// Returns true when the point lies outside <paramref name="roi"/>.
        /// </summary>
        public static bool IsClipped(this PointF pa, RectangleF roi)
        {
            if (pa.X >= roi.Left && pa.X < roi.Right && pa.Y >= roi.Top && pa.Y < roi.Bottom)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Returns true when the point lies outside <paramref name="roi"/>.
        /// </summary>
        public static bool IsClipped(this PointF pa, Rectangle roi)
            => pa.IsClipped(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));

        /// <summary>
        /// Returns the point, or (0,0) when it lies outside <paramref name="roi"/>.
        /// </summary>
        public static PointF Clip(this PointF pa, RectangleF roi)
            => pa.IsClipped(roi) ? new PointF() : pa;
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside <paramref name="roi"/>.
        /// </summary>
        public static PointF Clip(this PointF pa, Rectangle roi)
            => pa.Clip(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static PointF Clip(this PointF pa, int maxW, int maxH)
            => pa.Clip(new Rectangle(0, 0, maxW, maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static PointF Clip(this PointF pa, float maxW, float maxH)
            => pa.Clip(new Rectangle(0, 0, (int)maxW, (int)maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static PointF Clip(this PointF pa, double maxW, double maxH)
            => pa.Clip(new Rectangle(0, 0, (int)maxW, (int)maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside the area of <paramref name="size"/>.
        /// </summary>
        public static PointF Clip(this PointF pa, Size size)
            => pa.Clip(new Rectangle(0, 0, size.Width, size.Height));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside the area of <paramref name="size"/>.
        /// </summary>
        public static PointF Clip(this PointF pa, SizeF size)
            => pa.Clip(new Rectangle(0, 0, (int)size.Width, (int)size.Height));

        #endregion
    }
}
