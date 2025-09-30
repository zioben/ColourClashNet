using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
{
    public static class PointExt
    {
        #region Math
        public static System.Drawing.Point Add(this Point pa, Point pb)
        {
            return new System.Drawing.Point( pa.X+pb.X, pa.Y+pb.Y );
        }
        public static System.Drawing.Point Sub(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X - pb.X, pa.Y - pb.Y);
        }
        public static System.Drawing.Point MulP2P(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X * pb.X, pa.Y * pb.Y);
        }
        public static System.Drawing.Point DivP2P(this Point pa, Point pb)
        {
            var x = pb.X > 0 ? (double)pa.X / pb.X : 0;
            var y = pb.Y > 0 ? (double)pa.Y / pb.Y : 0;
            return new System.Drawing.Point((int)x,(int)y);
        }

        #endregion

        #region Clip
        public static bool IsClipped(this Point pa, Rectangle roi)
        {
            if (pa.X >= roi.Left && pa.X < roi.Right && pa.Y >= roi.Top && pa.Y <= roi.Bottom)
            {
                return false;
            }
            return true;
        }
        public static bool IsClipped(this Point pa, RectangleF roi)
            => IsClipped(pa, new Rectangle((int)roi.X, (int)roi.Y, (int)roi.Width, (int)roi.Height));

        public static System.Drawing.Point Clip(this Point pa, Rectangle roi)
           => pa.IsClipped(roi) ? new Point() : pa;

        public static System.Drawing.Point Clip(this Point pa, int MaxW, int MaxH )
            => Clip(pa, new Rectangle(0,0,MaxW,MaxH));
        public static System.Drawing.Point Clip(this Point pa, float MaxW, float MaxH)
            => Clip(pa, new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.Point Clip(this Point pa, double MaxW, double MaxH)
            => Clip(pa, new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.Point Clip(this Point pa, Size s)
            => Clip(pa, new Rectangle(0, 0, s.Width,s.Height));
        public static System.Drawing.Point Clip(this Point pa, SizeF s)
           => Clip(pa, new Rectangle(0, 0, (int)s.Width, (int)s.Height));

        #endregion
    }

    public static class PointFExt
    {
        #region Math
        public static System.Drawing.PointF Add(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X + pb.X, pa.Y + pb.Y);
        }
        public static System.Drawing.PointF Sub(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X - pb.X, pa.Y - pb.Y);
        }
        public static System.Drawing.PointF MulP2P(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X * pb.X, pa.Y * pb.Y);
        }
        public static System.Drawing.PointF DivP2P(this PointF pa, PointF pb)
        {
            var x = pb.X > 0 ? pa.X / pb.X : 0;
            var y = pb.Y > 0 ? pa.Y / pb.Y : 0;
            return new System.Drawing.PointF(x, y);
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
            => IsClipped(pa, new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));

        public static System.Drawing.PointF Clip(this PointF pa, RectangleF roi)            
            => pa.IsClipped(roi) ? new PointF() : pa;
        public static System.Drawing.PointF Clip(this PointF pa, Rectangle roi)
            => Clip(pa, new RectangleF(roi.X, roi.Y, roi.Width, roi.Height));
        public static System.Drawing.PointF Clip(this PointF pa, int MaxW, int MaxH)
            => Clip(pa, new Rectangle(0, 0, MaxW, MaxH));
        public static System.Drawing.PointF Clip(this PointF pa, float MaxW, float MaxH)
            => Clip(pa, new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.PointF Clip(this PointF pa, double MaxW, double MaxH)
            => Clip(pa, new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.PointF Clip(this PointF pa, Size s)
            => Clip(pa, new Rectangle(0, 0, s.Width, s.Height));
        public static System.Drawing.PointF Clip(this PointF pa, SizeF s)
            => Clip(pa, new Rectangle(0, 0, (int)s.Width, (int)s.Height));

        #endregion
    }
}
