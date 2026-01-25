using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public readonly struct ImageRectangle<T> where T : INumber<T>
    {
        

        public T X { get; }
        public T Y { get; }        
        public T Width { get ; }
        public T Height { get ; }

        //public T CS => X;
        //public T RS => Y;
        //public T CE => X + Width;
        //public T RE => Y + Height;
        public T XS => X;
        public T YS => Y;
        public T XE => X + Width;
        public T YE => Y + Height;

        public bool IsEmpty
        {
            get
            {
                return Width <= T.Zero || Height <= T.Zero;
            }
        }

        public ImageRectangle(T x, T y, T width, T height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public MatrixRectangle<T> ToMatrixRectangle()
            => new MatrixRectangle<T>(Y, X, Height, Width); 

        public override string ToString()
            => $"X:{X} Y:{Y} W:{Width} H:{Height}";


        public static ImageRectangle<T> Intestect( ImageRectangle<T> rectA, ImageRectangle<T> rectB)
        {
            T ax1 = rectA.X;
            T ay1 = rectA.Y;
            T ax2 = rectA.X + rectA.Width;
            T ay2 = rectA.Y +  rectA.Height;
            T bx1 = rectB.X;
            T by1 = rectB.Y;
            T bx2 = rectB.X + rectB.Width;
            T by2 = rectB.Y + rectB.Height;
            T ix1 = ax1 > bx1 ? ax1 : bx1;
            T iy1 = ay1 > by1 ? ay1 : by1;
            T ix2 = ax2 < bx2 ? ax2 : bx2;
            T iy2 = ay2 < by2 ? ay2 : by2;
            if (ix2 >= ix1 && iy2 >= iy1)
            {
                return new ImageRectangle<T>(ix1, iy1, ix2 - ix1, iy2 - iy1);
            }
            else
            {
                return new ImageRectangle<T>();
            }
        }

        public static ImageRectangle<T> Union(ImageRectangle<T> rectA, ImageRectangle<T> rectB)
        {
            T ax1 = rectA.X;
            T ay1 = rectA.Y;
            T ax2 = rectA.X + rectA.Width;
            T ay2 = rectA.Y + rectA.Height;
            T bx1 = rectB.X;
            T by1 = rectB.Y;
            T bx2 = rectB.X + rectB.Width;
            T by2 = rectB.Y + rectB.Height;
            T ux1 = ax1 < bx1 ? ax1 : bx1;
            T uy1 = ay1 < by1 ? ay1 : by1;
            T ux2 = ax2 > bx2 ? ax2 : bx2;
            T uy2 = ay2 > by2 ? ay2 : by2;
            return new ImageRectangle<T>(ux1, uy1, ux2 - ux1, uy2 - uy1);
        }
    }
}
