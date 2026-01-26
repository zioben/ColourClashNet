using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public readonly struct Rectangle<T> where T : INumber<T>
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

        public Rectangle(T x, T y, T width, T height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }


        public override string ToString()
            => $"X:{X} Y:{Y} W:{Width} H:{Height}";


        public static Rectangle<T> Intersect( Rectangle<T> rectA, Rectangle<T> rectB)
        {
            T axs = rectA.X;
            T ays = rectA.Y;
            T axe = rectA.X + rectA.Width;
            T aye = rectA.Y + rectA.Height;
            T bxs = rectB.X;
            T bys = rectB.Y;
            T bxe = rectB.X + rectB.Width;
            T bye = rectB.Y + rectB.Height;
            T ix1 = axs > bxs ? axs : bxs;
            T iy1 = ays > bys ? ays : bys;
            T ix2 = axe < bxe ? axe : bxe;
            T iy2 = aye < bye ? aye : bye;
            if (ix2 >= ix1 && iy2 >= iy1)
            {
                return new Rectangle<T>(ix1, iy1, ix2 - ix1, iy2 - iy1);
            }
            else
            {
                return new Rectangle<T>();
            }
        }

        public static Rectangle<T> Union(Rectangle<T> rectA, Rectangle<T> rectB)
        {
            T axs = rectA.X;
            T ays = rectA.Y;
            T axe = rectA.X + rectA.Width;
            T aye = rectA.Y + rectA.Height;
            T bxs = rectB.X;
            T bys = rectB.Y;
            T bxe = rectB.X + rectB.Width;
            T bye = rectB.Y + rectB.Height;
            T ux1 = axs < bxs ? axs : bxs;
            T uy1 = ays < bys ? ays : bys;
            T ux2 = axe > bxe ? axe : bxe;
            T uy2 = aye > bye ? aye : bye;
            return new Rectangle<T>(ux1, uy1, ux2 - ux1, uy2 - uy1);
        }
    }
}
