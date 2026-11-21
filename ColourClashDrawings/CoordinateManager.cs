using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Converts coordinates between the world (control) space and the local (bitmap) space.
    /// <para/>
    /// The transformation is: translate by <see cref="WorldOrigin"/>, rotate by <see cref="WorldRotationAngle"/>
    /// around <see cref="WorldRotationPoint"/>, scale by <see cref="TransfZoom"/>, translate by <see cref="TransfOrigin"/>.
    /// The same transformation is applied to a <see cref="Graphics"/> to draw images.
    /// </summary>
    [Serializable]
    public class CoordinateManager
    {
        /// <summary>
        /// Creates an identity transformation.
        /// </summary>
        public CoordinateManager()
        {
            WorldOrigin = new PointF(0,0);
            WorldRotationPoint = new PointF(0,0);
            WorldRotationAngle = 0;
            TransfZoom = new PointF(1.0f, 1.0f);
            TransfOrigin = new PointF(0.0f, 0.0f);  
        }

        /// <summary>
        /// Origin of the world space.
        /// </summary>
        [Browsable(true), Category("Coordinates")]
        public PointF WorldOrigin { get; set; }

        /// <summary>
        /// Center of rotation.
        /// </summary>
        [Browsable(true), Category("Coordinates")]
        public PointF WorldRotationPoint { get; set; }

        /// <summary>
        /// Rotation angle in degrees.
        /// </summary>
        [Browsable(true), Category("Coordinates")]
        public double WorldRotationAngle
        {
            get { return rotationDegrees; }
            set
            {
                rotationDegrees = value;
                rotationRadians = WorldRotationAngle / 180 * Math.PI;
                cos = Math.Cos(rotationRadians);
                sin = Math.Sin(rotationRadians);
            }
        }

        /// <summary>
        /// Zoom factors (X, Y).
        /// </summary>
        [Browsable(true), Category("Coordinates")]
        public PointF TransfZoom { get; set; }

        /// <summary>
        /// Origin of the destination space.
        /// </summary>
        [Browsable(true), Category("Coordinates")]
        public PointF TransfOrigin { get; set; }

        // Rotation angle and its cached trigonometric values
        double rotationDegrees;
        double rotationRadians;
        double cos;
        double sin;

        /// <summary>
        /// Sets the zoom factors.
        /// </summary>
        public void SetTrasfZoom(float x, float y)
        {
            TransfZoom = new PointF(x,y);
        }
        /// <summary>
        /// Sets the zoom factors.
        /// </summary>
        public void SetTransfZoom(int x, int y) => SetTrasfZoom(x,y);

        /// <summary>
        /// Sets the zoom factors.
        /// </summary>
        public void SetTransfZoom(double x, double y) => SetTrasfZoom((float)x, (float)y);
        /// <summary>
        /// Sets the same zoom on both axes.
        /// </summary>
        public void SetTransfZoom(float scale) => SetTrasfZoom(scale, scale);
        /// <summary>
        /// Sets the same zoom on both axes.
        /// </summary>
        public void SetTransfZoom(int scale) => SetTransfZoom(scale, scale);
        /// <summary>
        /// Sets the same zoom on both axes.
        /// </summary>
        public void SetTransfZoom(double scale) => SetTransfZoom(scale, scale);

        /// <summary>
        /// Sets the zoom factors from a point.
        /// </summary>
        public void SetTransfZoom(Point p)
        {
            TransfZoom = new PointF( p.X, p.Y );   
        }
        /// <summary>
        /// Sets the zoom factors from a point.
        /// </summary>
        public void SetTransfZoom(PointF p)
        {
            TransfZoom = p;
        }

        /// <summary>
        /// Sets the zoom as the ratio between world and local sizes (0 when a local size is 0).
        /// </summary>
        public void SetTransfZoom(float scaleWX, float scaleWY, float scaleLX, float scaleLY )
        {
            TransfZoom = new PointF(
                scaleLX != 0 ? scaleWX / scaleLX : 0,
                scaleLY != 0 ? scaleWY / scaleLY : 0);
        }
        /// <summary>
        /// Sets the zoom as the ratio between world and local sizes.
        /// </summary>
        public void SetTransfZoom(int scaleWX, int scaleWY, int scaleLX, int scaleLY) 
            => SetTransfZoom(scaleWX,scaleWY, scaleLX, (float) scaleLY);
        /// <summary>
        /// Sets the zoom as the ratio between world and local sizes.
        /// </summary>
        public void SetTransfZoom(double scaleWX, double scaleWY, double scaleLX, double scaleLY)
            => SetTransfZoom((float)scaleWX, (float)scaleWY, (float)scaleLX, (float)scaleLY);

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Moves the world origin by -<paramref name="p"/>.
        /// </summary>
        public void TranslateWorldOrigin(PointF p)
        {
            WorldOrigin = WorldOrigin.Sub(p);
        }
        /// <summary>
        /// Moves the world origin by -<paramref name="p"/>.
        /// </summary>
        public void TranslateWorldOrigin(Point p)
        {
            WorldOrigin = WorldOrigin.Sub(p);
        }
        /// <summary>
        /// Moves the world origin by (-x, -y).
        /// </summary>
        public void TranslateWorldOrigin(double x, double y)
            => TranslateWorldOrigin(new PointF((float)x, (float)y));
        /// <summary>
        /// Moves the world origin by (-x, -y).
        /// </summary>
        public void TranslateWorldOrigin(float x, float y)
            => TranslateWorldOrigin(new PointF(x, y));
        /// <summary>
        /// Moves the world origin by (-x, -y).
        /// </summary>
        public void TranslateWorldOrigin(int x, int y)
            => TranslateWorldOrigin(new PointF(x, y));

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Moves the rotation center by -<paramref name="p"/>.
        /// </summary>
        public void TranslateRotationPoint(PointF p)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(p);
        }
        /// <summary>
        /// Moves the rotation center by -<paramref name="p"/>.
        /// </summary>
        public void TranslateRotationPoint(Point p)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(p);
        }
        /// <summary>
        /// Moves the rotation center by (-x, -y).
        /// </summary>
        public void TranslateRotationPoint(double x, double y)
          => TranslateRotationPoint(new PointF((float)x, (float)y));
        /// <summary>
        /// Moves the rotation center by (-x, -y).
        /// </summary>
        public void TranslateRotationPoint(float x, float y)
            => TranslateRotationPoint(new PointF(x, y));
        /// <summary>
        /// Moves the rotation center by (-x, -y).
        /// </summary>
        public void TranslateRotationPoint(int x, int y)
            => TranslateRotationPoint(new PointF(x, y));

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Adds <paramref name="deltaDegrees"/> to the rotation angle.
        /// </summary>
        public void AddRotationAngle(double deltaDegrees)
        {
            WorldRotationAngle += deltaDegrees;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Moves the destination origin by -<paramref name="p"/>.
        /// </summary>
        public void TranslateTrasformationOrigin(PointF p)
        {
            TransfOrigin = TransfOrigin.Sub(p);
        }
        /// <summary>
        /// Moves the destination origin by -<paramref name="p"/>.
        /// </summary>
        public void TranslateTrasformationOrigin(Point p)
        {
            TransfOrigin = TransfOrigin.Sub(p);
        }
        /// <summary>
        /// Moves the destination origin by (-x, -y).
        /// </summary>
        public void TranslateTrasformationOrigin(double x, double y)
            => TranslateTrasformationOrigin(new PointF((float)x, (float)y));
        /// <summary>
        /// Moves the destination origin by (-x, -y).
        /// </summary>
        public void TranslateTrasformationOrigin(float x, float y)
            => TranslateTrasformationOrigin(new PointF(x, y));
        /// <summary>
        /// Moves the destination origin by (-x, -y).
        /// </summary>
        public void TranslateTrasformationOrigin(int x, int y)
            => TranslateTrasformationOrigin(new PointF(x, y));


        /// <summary>
        /// Applies the forward transformation to a point.
        /// </summary>
        public PointF Transform(PointF p)
        {
            // Move to the rotation center
            PointF localPoint = p.Sub(WorldOrigin).Sub(WorldRotationPoint);
            // Rotate
            double rotatedX = cos * localPoint.X + sin * localPoint.Y;
            double rotatedY = -sin * localPoint.X + cos * localPoint.Y;
            // Move back, then apply zoom and destination origin
            PointF transformedPoint = new PointF((float)rotatedX, (float)rotatedY).Add(WorldRotationPoint).MulP2P(TransfZoom).Add(TransfOrigin);
            return transformedPoint;
        }

        /// <summary>
        /// Applies the forward transformation to a point.
        /// </summary>
        public PointF Transform(Point p)
            => Transform(new PointF(p.X,p.Y));
        /// <summary>
        /// Applies the forward transformation to (<paramref name="xw"/>, <paramref name="yw"/>).
        /// </summary>
        public PointF Transform(double xw, double yw)
            => Transform(new PointF((float)xw, (float)yw));
        /// <summary>
        /// Applies the forward transformation to (<paramref name="xw"/>, <paramref name="yw"/>).
        /// </summary>
        public PointF Transform(float xw, float yw)
            => Transform(new PointF((float)xw, (float)yw));
        /// <summary>
        /// Applies the forward transformation to (<paramref name="xw"/>, <paramref name="yw"/>).
        /// </summary>
        public PointF Transform(int xw, int yw)
            => Transform(new PointF((float)xw, (float)yw));


        /// <summary>
        /// Applies the inverse transformation to a point.
        /// </summary>
        public PointF InverseTransform(PointF p)
        {
            // Exact inverse of Transform: remove origin and zoom, rotate back, restore the world origin
            var localPoint = p.Sub(TransfOrigin).DivP2P(TransfZoom).Sub(WorldRotationPoint);
            double rotatedX = cos * localPoint.X - sin * localPoint.Y;
            double rotatedY = sin * localPoint.X + cos * localPoint.Y;
            localPoint = new PointF((float)rotatedX, (float)rotatedY).Add(WorldRotationPoint).Add(WorldOrigin);
            return localPoint;            
        }

        /// <summary>
        /// Applies the inverse transformation to a point.
        /// </summary>
        public PointF InverseTransform(Point p)
            => InverseTransform(new PointF((float) p.X, (float)p.Y));
        /// <summary>
        /// Applies the inverse transformation to (<paramref name="trX"/>, <paramref name="trY"/>).
        /// </summary>
        public PointF InverseTransform(double trX,double trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        /// <summary>
        /// Applies the inverse transformation to (<paramref name="trX"/>, <paramref name="trY"/>).
        /// </summary>
        public PointF InverseTransform(float trX, float trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        /// <summary>
        /// Applies the inverse transformation to (<paramref name="trX"/>, <paramref name="trY"/>).
        /// </summary>
        public PointF InverseTransform(int trX, int trY)
            => InverseTransform(new PointF((float)trX, (float)trY));

      
        /// <summary>
        /// Loads the world translation, rotation and zoom into <paramref name="graphics"/>.
        /// </summary>
        /// <returns>The previous graphics transformation, to be restored after drawing.</returns>
        Matrix RebuildTransform(Graphics graphics)
        {
            var previousTransform = graphics.Transform;
            graphics.Transform = new Matrix();
            graphics.ResetTransform();
            graphics.TranslateTransform(WorldOrigin.X, WorldOrigin.Y, MatrixOrder.Append);
            graphics.TranslateTransform(WorldRotationPoint.X, WorldRotationPoint.Y, MatrixOrder.Append);
            graphics.RotateTransform((float)WorldRotationAngle, MatrixOrder.Append);
            graphics.TranslateTransform(-WorldRotationPoint.X, -WorldRotationPoint.Y, MatrixOrder.Append);
            graphics.ScaleTransform(TransfZoom.X, TransfZoom.Y);
            return previousTransform;
        }

        /// <summary>
        /// Draws <paramref name="image"/> on <paramref name="graphics"/> with the current transformation.
        /// </summary>
        public void DrawImage(Graphics graphics, Image image)
        {
            if (image == null || graphics == null)
            {
                return;
            }
            var previousTransform = RebuildTransform(graphics);
            // This translation is affected by the zoom parameters:
            // the zoom is reversed so that the origin keeps following the mouse pointer in world units
            var previousInterpolation = graphics.InterpolationMode;
            var previousPixelOffset = graphics.PixelOffsetMode;
            var previousSmoothing = graphics.SmoothingMode;

            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.SmoothingMode = SmoothingMode.None;


            var origin = TransfOrigin.DivP2P(TransfZoom);
            graphics.DrawImage(image, origin);

            graphics.InterpolationMode = previousInterpolation;
            graphics.PixelOffsetMode = previousPixelOffset;
            graphics.SmoothingMode = previousSmoothing;

            graphics.Transform = previousTransform;
        }
    }
}
