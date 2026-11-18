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
    /// <para>
    /// World = Control Coordinates
    /// Local = Bitmap Coordinates
    /// </para>
    /// </summary>
    [Serializable]
   
    public class CoordinateManager
    {
        public CoordinateManager()
        {
            WorldOrigin = new PointF(0,0);
            WorldRotationPoint = new PointF(0,0);
            WorldRotationAngle = 0;
            TransfZoom = new PointF(1.0f, 1.0f);
            TransfOrigin = new PointF(0.0f, 0.0f);  
        }

        [Browsable(true), Category("Coordinates")]
        public PointF WorldOrigin { get; set; }

        [Browsable(true), Category("Coordinates")]
        public PointF WorldRotationPoint { get; set; }

        [Browsable(true), Category("Coordinates")]
        public double WorldRotationAngle
        {
            get { return rot; }
            set
            {
                rot = value;
                rad = WorldRotationAngle / 180 * Math.PI;
                cos = Math.Cos(rad);
                sin = Math.Sin(rad);
            }
        }

        [Browsable(true), Category("Coordinates")]
        public PointF TransfZoom { get; set; }

        [Browsable(true), Category("Coordinates")]
        public PointF TransfOrigin { get; set; }

        double rot;
        double rad;
        double cos;
        double sin;

        public void SetTrasfZoom(float x, float y)
        {
            TransfZoom = new PointF(x,y);
        }
        public void SetTransfZoom(int x, int y) => SetTrasfZoom(x,y);

        public void SetTransfZoom(double x, double y) => SetTrasfZoom((float)x, (float)y);
        public void SetTransfZoom(float scale) => SetTrasfZoom(scale, scale);
        public void SetTransfZoom(int scale) => SetTransfZoom(scale, scale);
        public void SetTransfZoom(double scale) => SetTransfZoom(scale, scale);

        public void SetTransfZoom(Point p)
        {
            TransfZoom = new PointF( p.X, p.Y );   
        }
        public void SetTransfZoom(PointF p)
        {
            TransfZoom = p;
        }

        public void SetTransfZoom(float scaleWX, float scaleWY, float scaleLX, float scaleLY )
        {
            TransfZoom = new PointF(
                scaleLX != 0 ? scaleWX / scaleLX : 0,
                scaleLY != 0 ? scaleWY / scaleLY : 0);
        }
        public void SetTransfZoom(int scaleWX, int scaleWY, int scaleLX, int scaleLY) 
            => SetTransfZoom(scaleWX,scaleWY, scaleLX, (float) scaleLY);
        public void SetTransfZoom(double scaleWX, double scaleWY, double scaleLX, double scaleLY)
            => SetTransfZoom((float)scaleWX, (float)scaleWY, (float)scaleLX, (float)scaleLY);

        //--------------------------------------------------------------------------------
        public void TranslateWorldOrigin(PointF p)
        {
            WorldOrigin = WorldOrigin.Sub(p);
        }
        public void TranslateWorldOrigin(Point p)
        {
            WorldOrigin = WorldOrigin.Sub(p);
        }
        public void TranslateWorldOrigin(double x, double y)
            => TranslateWorldOrigin(new PointF((float)x, (float)y));
        public void TranslateWorldOrigin(float x, float y)
            => TranslateWorldOrigin(new PointF(x, y));
        public void TranslateWorldOrigin(int x, int y)
            => TranslateWorldOrigin(new PointF(x, y));

        //--------------------------------------------------------------------------------
        public void TranslateRotationPoint(PointF p)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(p);
        }
        public void TranslateRotationPoint(Point p)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(p);
        }
        public void TranslateRotationPoint(double x, double y)
          => TranslateRotationPoint(new PointF((float)x, (float)y));
        public void TranslateRotationPoint(float x, float y)
            => TranslateRotationPoint(new PointF(x, y));
        public void TranslateRotationPoint(int x, int y)
            => TranslateRotationPoint(new PointF(x, y));

        //--------------------------------------------------------------------------------
        public void AddRotationAngle(double deltaDegrees)
        {
            WorldRotationAngle += deltaDegrees;
        }

        //--------------------------------------------------------------------------------
        public void TranslateTrasformationOrigin(PointF p)
        {
            TransfOrigin = TransfOrigin.Sub(p);
        }
        public void TranslateTrasformationOrigin(Point p)
        {
            TransfOrigin = TransfOrigin.Sub(p);
        }
        public void TranslateTrasformationOrigin(double x, double y)
            => TranslateTrasformationOrigin(new PointF((float)x, (float)y));
        public void TranslateTrasformationOrigin(float x, float y)
            => TranslateTrasformationOrigin(new PointF(x, y));
        public void TranslateTrasformationOrigin(int x, int y)
            => TranslateTrasformationOrigin(new PointF(x, y));


        public PointF Transform(PointF p)
        {
            // Tralsare sull'origine rototraslazione
            PointF localPoint = p.Sub(WorldOrigin).Sub(WorldRotationPoint);
            // Ruotare punti
            double xl = cos * localPoint.X + sin * localPoint.Y;
            double yl = -sin * localPoint.X + cos * localPoint.Y;
            // Tralsare sull'origine mondo
            PointF transformedPoint = new PointF((float)xl, (float)yl).Add(WorldRotationPoint).MulP2P(TransfZoom).Add(TransfOrigin);
            return transformedPoint;
        }

        public PointF Transform(Point p)
            => Transform(new PointF(p.X,p.Y));
        public PointF Transform(double xw, double yw)
            => Transform(new PointF((float)xw, (float)yw));
        public PointF Transform(float xw, float yw)
            => Transform(new PointF((float)xw, (float)yw));
        public PointF Transform(int xw, int yw)
            => Transform(new PointF((float)xw, (float)yw));


        // Il trucco, si tratta di calcolare la rototraslazione e traslare rispetto alle origini
        // 
        // Pw = World coordinates
        // Po = Origin coordinates respect World coordinates
        // Pr = Rotation point coordinates respect -> Po <-
        // 
        // Rispetto al punto di rotazione Pr:
        //  |xw'| =  | cos -sen | | xl - xr | + |xr|
        //  |yw'| =  | sen  cos | | xl - yr | + |yr|
        //  |xw | = | xw' + xo | / ZoomX;
        //  |yw | = | yw' + yo | / ZoomY;
        // riassumendo:
        //  |xw| =  | | cos sen | | xl - xr | + |xr + xo| | ^ |1/zx|
        //  |yw| =  | |-sen cos | | yl - yr | + |yr + yo| |   |1/zy|

        public PointF InverseTransform(PointF p)
        {
            var localPoint = p.Sub(TransfOrigin).DivP2P(TransfZoom).Sub(WorldRotationPoint);
            //
            double xl = cos * localPoint.X - sin * localPoint.Y;
            double yl = sin * localPoint.X + cos * localPoint.Y;
            localPoint = new PointF((float)xl, (float)yl).Add(WorldRotationPoint).Add(WorldOrigin);
            return localPoint;            
        }

        public PointF InverseTransform(Point p)
            => InverseTransform(new PointF((float) p.X, (float)p.Y));
        public PointF InverseTransform(double trX,double trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        public PointF InverseTransform(float trX, float trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        public PointF InverseTransform(int trX, int trY)
            => InverseTransform(new PointF((float)trX, (float)trY));

      
        Matrix RebuildTransform(Graphics graphics)
        {
            var oldTR = graphics.Transform;
            graphics.Transform = new Matrix();
            graphics.ResetTransform();
            graphics.TranslateTransform(WorldOrigin.X, WorldOrigin.Y, MatrixOrder.Append);
            graphics.TranslateTransform(WorldRotationPoint.X, WorldRotationPoint.Y, MatrixOrder.Append);
            graphics.RotateTransform((float)WorldRotationAngle, MatrixOrder.Append);
            graphics.TranslateTransform(-WorldRotationPoint.X, -WorldRotationPoint.Y, MatrixOrder.Append);
            graphics.ScaleTransform(TransfZoom.X, TransfZoom.Y);
            return oldTR;
        }

        public void DrawImage(Graphics graphics, Image image)
        {
            if (image == null || graphics == null)
            {
                return;
            }
            var oldTR = RebuildTransform(graphics);
            // This transaltion is affected by the Zoom Parameters.
            // To maintains "Original" world size to follow mouse pointer, reversing the zoom is necessaty
            var origin = TransfOrigin.DivP2P(TransfZoom);
            graphics.DrawImage(image, origin);
            graphics.Transform = oldTR;
        }
    }
}
