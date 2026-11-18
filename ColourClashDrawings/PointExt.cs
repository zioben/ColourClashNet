using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Arithmetic and clipping extensions for <see cref="Point"/>.
    /// </summary>
    public static class PointExt
    {
        #region Math
        /// <summary>
        /// Component-wise sum.
        /// </summary>
        public static System.Drawing.Point Add(this Point pa, Point pb)
        {
            return new System.Drawing.Point( pa.X+pb.X, pa.Y+pb.Y );
        }
        /// <summary>
        /// Component-wise difference.
        /// </summary>
        public static System.Drawing.Point Sub(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X - pb.X, pa.Y - pb.Y);
        }
        /// <summary>
        /// Component-wise (point to point) product.
        /// </summary>
        public static System.Drawing.Point MulP2P(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X * pb.X, pa.Y * pb.Y);
        }
        /// <summary>
        /// Component-wise (point to point) division; a non-positive divisor gives 0.
        /// </summary>
        public static System.Drawing.Point DivP2P(this Point pa, Point pb)
        {
            var x = pb.X > 0 ? (double)pa.X / pb.X : 0;
            var y = pb.Y > 0 ? (double)pa.Y / pb.Y : 0;
            return new System.Drawing.Point((int)x,(int)y);
        }

        #endregion

        #region Clip
        /// <summary>
        /// Returns true when the point lies outside <paramref name="roi"/>.
        /// </summary>
        public static bool IsClipped(this Point pa, Rectangle roi)
        {
            if (pa.X >= roi.Left && pa.X < roi.Right && pa.Y >= roi.Top && pa.Y <= roi.Bottom)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Returns true when the point lies outside <paramref name="roi"/>.
        /// </summary>
        public static bool IsClipped(this Point pa, RectangleF roi)
            => pa.IsClipped(new Rectangle((int)roi.X, (int)roi.Y, (int)roi.Width, (int)roi.Height));

        /// <summary>
        /// Returns the point, or (0,0) when it lies outside <paramref name="roi"/>.
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, Rectangle roi)
           => pa.IsClipped(roi) ? new Point() : pa;

        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, int maxW, int maxH )
            => pa.Clip(new Rectangle(0,0,maxW,maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, float maxW, float maxH)
            => pa.Clip(new Rectangle(0, 0, (int)maxW, (int)maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside [0, maxW) x [0, maxH].
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, double maxW, double maxH)
            => pa.Clip(new Rectangle(0, 0, (int)maxW, (int)maxH));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside the area of <paramref name="size"/>.
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, Size size)
            => pa.Clip(new Rectangle(0, 0, size.Width,size.Height));
        /// <summary>
        /// Returns the point, or (0,0) when it lies outside the area of <paramref name="size"/>.
        /// </summary>
        public static System.Drawing.Point Clip(this Point pa, SizeF size)
           => pa.Clip(new Rectangle(0, 0, (int)size.Width, (int)size.Height));

        #endregion
    }
}
