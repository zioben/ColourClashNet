using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    public static class PointFExt
    {
        #region Math
        public static PointF Add(this PointF pa, PointF pb)
        {
            return new PointF(pa.X + pb.X, pa.Y + pb.Y);
        }
        public static PointF Sub(this PointF pa, PointF pb)
        {
            return new PointF(pa.X - pb.X, pa.Y - pb.Y);
        }
        public static PointF MulP2P(this PointF pa, PointF pb)
        {
            return new PointF(pa.X * pb.X, pa.Y * pb.Y);
        }
        public static PointF DivP2P(this PointF pa, PointF pb)
        {
            var x = pb.X > 0 ? pa.X / pb.X : 0;
            var y = pb.Y > 0 ? pa.Y / pb.Y : 0;
            return new PointF(x, y);
        }

        #endregion

        #region Clip
        public static bool IsClipped(this PointF pa, RectangleF roi)
        {
            if (pa.X >= roi.Left && pa.X < roi.Right && pa.Y >= roi.Top && pa.Y < roi.Bottom)
            {
                return false;
            }
            return true;
        }
        public static bool IsClipped(this PointF pa, Rectangle roi)
            => pa.IsClipped(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));

        public static PointF Clip(this PointF pa, RectangleF roi)
            => pa.IsClipped(roi) ? new PointF() : pa;
        public static PointF Clip(this PointF pa, Rectangle roi)
            => pa.Clip(new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));
        public static PointF Clip(this PointF pa, int MaxW, int MaxH)
            => pa.Clip(new Rectangle(0, 0, MaxW, MaxH));
        public static PointF Clip(this PointF pa, float MaxW, float MaxH)
            => pa.Clip(new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static PointF Clip(this PointF pa, double MaxW, double MaxH)
            => pa.Clip(new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static PointF Clip(this PointF pa, Size s)
            => pa.Clip(new Rectangle(0, 0, s.Width, s.Height));
        public static PointF Clip(this PointF pa, SizeF s)
            => pa.Clip(new Rectangle(0, 0, (int)s.Width, (int)s.Height));

        #endregion
    }
}
