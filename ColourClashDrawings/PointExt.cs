using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
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
            => pa.IsClipped(new Rectangle((int)roi.X, (int)roi.Y, (int)roi.Width, (int)roi.Height));

        public static System.Drawing.Point Clip(this Point pa, Rectangle roi)
           => pa.IsClipped(roi) ? new Point() : pa;

        public static System.Drawing.Point Clip(this Point pa, int MaxW, int MaxH )
            => pa.Clip(new Rectangle(0,0,MaxW,MaxH));
        public static System.Drawing.Point Clip(this Point pa, float MaxW, float MaxH)
            => pa.Clip(new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.Point Clip(this Point pa, double MaxW, double MaxH)
            => pa.Clip(new Rectangle(0, 0, (int)MaxW, (int)MaxH));
        public static System.Drawing.Point Clip(this Point pa, Size s)
            => pa.Clip(new Rectangle(0, 0, s.Width,s.Height));
        public static System.Drawing.Point Clip(this Point pa, SizeF s)
           => pa.Clip(new Rectangle(0, 0, (int)s.Width, (int)s.Height));

        #endregion
    }
}
