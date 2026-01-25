using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public readonly struct MatrixRectangle<T> where T : INumber<T>
    {
        

        public T Column { get; }
        public T Row { get; }        
        public T ColumnsLenght { get ; }
        public T RowsLenght { get ; }

        //public T CS => X;
        //public T RS => Y;
        //public T CE => X + Width;
        //public T RE => Y + Height;
        public T CS => Column;
        public T RS => Row;
        public T CE => Column + ColumnsLenght;
        public T RE => Row + RowsLenght;

        public bool IsEmpty
        {
            get
            {
                return ColumnsLenght <= T.Zero || RowsLenght <= T.Zero;
            }
        }

        public MatrixRectangle(T x, T y, T width, T height)
        {
            Column = x;
            Row = y;
            ColumnsLenght = width;
            RowsLenght = height;
        }

        public ImageRectangle<T> ToImageRectangle
            => new ImageRectangle<T>(Column, Row, ColumnsLenght, RowsLenght);

        public override string ToString()
            => $"X:{Column} Y:{Row} W:{ColumnsLenght} H:{RowsLenght}";


        public static MatrixRectangle<T> Intestect(MatrixRectangle<T> rectA, MatrixRectangle<T> rectB)
        {
            T ax1 = rectA.Column;
            T ay1 = rectA.Row;
            T ax2 = rectA.Column + rectA.ColumnsLenght;
            T ay2 = rectA.Row +  rectA.RowsLenght;
            T bx1 = rectB.Column;
            T by1 = rectB.Row;
            T bx2 = rectB.Column + rectB.ColumnsLenght;
            T by2 = rectB.Row + rectB.RowsLenght;
            T ix1 = ax1 > bx1 ? ax1 : bx1;
            T iy1 = ay1 > by1 ? ay1 : by1;
            T ix2 = ax2 < bx2 ? ax2 : bx2;
            T iy2 = ay2 < by2 ? ay2 : by2;
            if (ix2 >= ix1 && iy2 >= iy1)
            {
                return new MatrixRectangle<T>(ix1, iy1, ix2 - ix1, iy2 - iy1);
            }
            else
            {
                return new MatrixRectangle<T>();
            }
        }

        public static MatrixRectangle<T> Union(MatrixRectangle<T> rectA, MatrixRectangle<T> rectB)
        {
            T ax1 = rectA.Column;
            T ay1 = rectA.Row;
            T ax2 = rectA.Column + rectA.ColumnsLenght;
            T ay2 = rectA.Row + rectA.RowsLenght;
            T bx1 = rectB.Column;
            T by1 = rectB.Row;
            T bx2 = rectB.Column + rectB.ColumnsLenght;
            T by2 = rectB.Row + rectB.RowsLenght;
            T ux1 = ax1 < bx1 ? ax1 : bx1;
            T uy1 = ay1 < by1 ? ay1 : by1;
            T ux2 = ax2 > bx2 ? ax2 : bx2;
            T uy2 = ay2 > by2 ? ay2 : by2;
            return new MatrixRectangle<T>(ux1, uy1, ux2 - ux1, uy2 - uy1);
        }
    }
}
