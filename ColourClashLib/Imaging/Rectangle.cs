using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    /// <summary>
    /// Immutable axis-aligned rectangle with generic numeric coordinates.
    /// </summary>
    /// <typeparam name="T">Numeric type (int, float, double ...)</typeparam>
    public readonly struct Rectangle<T> where T : INumber<T>
    {
        /// <summary>Left coordinate.</summary>
        public T X { get; }
        /// <summary>Top coordinate.</summary>
        public T Y { get; }
        /// <summary>Width.</summary>
        public T Width { get ; }
        /// <summary>Height.</summary>
        public T Height { get ; }
        /// <summary>X start (same as <see cref="X"/>).</summary>
        public T XS => X;
        /// <summary>Y start (same as <see cref="Y"/>).</summary>
        public T YS => Y;
        /// <summary>X end (exclusive): <c>X + Width</c>.</summary>
        public T XE => X + Width;
        /// <summary>Y end (exclusive): <c>Y + Height</c>.</summary>
        public T YE => Y + Height;

        /// <summary>True when width or height is not positive.</summary>
        public bool IsEmpty
        {
            get
            {
                return Width <= T.Zero || Height <= T.Zero;
            }
        }

        /// <summary>
        /// Creates a rectangle from its origin and size.
        /// </summary>
        public Rectangle(T x, T y, T width, T height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }


        public override string ToString()
            => $"X:{X} Y:{Y} W:{Width} H:{Height}";


        /// <summary>
        /// Returns the intersection of two rectangles, or an empty rectangle if they do not overlap.
        /// </summary>
        public static Rectangle<T> Intersect( Rectangle<T> rectA, Rectangle<T> rectB)
        {
            // a/b = rectangle, s/e = start/end, i = intersection
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

        /// <summary>
        /// Returns the smallest rectangle that contains both rectangles.
        /// </summary>
        public static Rectangle<T> Union(Rectangle<T> rectA, Rectangle<T> rectB)
        {
            // a/b = rectangle, s/e = start/end, u = union
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
