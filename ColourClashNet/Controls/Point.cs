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
        public static System.Drawing.Point Add(this Point pa, Point pb)
        {
            return new System.Drawing.Point( pa.X+pb.X, pa.Y+pb.Y );
        }
        public static System.Drawing.Point Sub(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X - pb.X, pa.Y - pb.Y);
        }
        public static System.Drawing.Point MulPP(this Point pa, Point pb)
        {
            return new System.Drawing.Point(pa.X * pb.X, pa.Y * pb.Y);
        }
        public static System.Drawing.Point DivPP(this Point pa, Point pb)
        {
            var x = pb.X > 0 ? (double)pa.X / pb.X : 0;
            var y = pb.Y > 0 ? (double)pa.Y / pb.Y : 0;
            return new System.Drawing.Point((int)x,(int)y);
        }
    }
    public static class PointFExt
    {
        public static System.Drawing.PointF Add(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X + pb.X, pa.Y + pb.Y);
        }
        public static System.Drawing.PointF Sub(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X - pb.X, pa.Y - pb.Y);
        }
        public static System.Drawing.PointF MulPP(this PointF pa, PointF pb)
        {
            return new System.Drawing.PointF(pa.X * pb.X, pa.Y * pb.Y);
        }
        public static System.Drawing.PointF DivPP(this PointF pa, PointF pb)
        {
            var x = pb.X > 0 ? pa.X / pb.X : 0;
            var y = pb.Y > 0 ? pa.Y / pb.Y : 0;
            return new System.Drawing.PointF(x, y);
        }
    }
}
