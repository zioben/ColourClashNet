using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
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
            Origin = new PointF();
            RotationPoint = new PointF();
            Zoom = new PointF(1.0f, 1.0f);
            RotationAngle = 0;
        }

        [Browsable(true), Category("Coordinates")]
        public PointF Origin { get; set; }
        [Browsable(true), Category("Coordinates")]
        public PointF RotationPoint { get; set; }
        [Browsable(true), Category("Coordinates")]
        public PointF Zoom { get;  set; }

        [Browsable(true), Category("Coordinates")]
        public double RotationAngle
        {
            get { return rot; }
            set
            {
                rot = value;
                rad = RotationAngle / 180 * Math.PI;
                cos = Math.Cos(rad);
                sin = Math.Sin(rad);
            }
        }

        double rot;
        double rad;
        double cos;
        double sin;

        public void SetZoom(float fX, float fY)
        {
            Zoom = new PointF(fX,fY);
        }

        public void SetZoom(int iX, int iY) => SetZoom((float)iX,(float)iY);

        public void SetZoom(double dX, double dY) => SetZoom((float)dX, (float)dY);
        public void SetZoom(float fScale) => SetZoom(fScale, fScale);
        public void SetZoom(int iScale) => SetZoom(iScale, iScale);
        public void SetZoom(double dScale) => SetZoom(dScale, dScale);

        public void SetZoom(float fScaleWX, float fScaleWY, float fScaleLX, float fScaleLY )
        {
            Zoom = new PointF(
                fScaleLX != 0 ? fScaleWX / fScaleLX : 0,
                fScaleLY != 0 ? fScaleWY / fScaleLY : 0);
        }
        public void SetZoom(int iScaleWX, int iScaleWY, int iScaleLX, int iScaleLY) 
            => SetZoom((float)iScaleWX,(float)iScaleWY, (float)iScaleLX, (float) iScaleLY);
        public void SetZoom(double dScaleWX, double dScaleWY, double dScaleLX, double dScaleLY)
            => SetZoom((float)dScaleWX, (float)dScaleWY, (float)dScaleLX, (float)dScaleLY);

        public void TranslateOriginPointRespectWorldCoordinates(double dx, double dy)
        {
            Origin = new PointF( Origin.X -(float)(dx * Zoom.X ), Origin.Y - (float)(dy * Zoom.Y));
        }

        public void TranslateRotationPointRespectWorldCoordinates(double dx, double dy)
        {
            RotationPoint = new PointF(RotationPoint.X - (float)(dx * Zoom.X), RotationPoint.Y - (float)(dy * Zoom.Y));
        }

        public void TranslateOriginPoint(double dx, double dy)
        {
            Origin = new PointF(Origin.X - (float)(dx), Origin.Y - (float)(dy));
        }

        public void TranslateRotationPoint(double dx, double dy)
        {
            RotationPoint = new PointF(RotationPoint.X - (float)(dx), RotationPoint.Y - (float)(dy));
        }

        public void AddRotationAngle(double deltaDegrees)
        {
            RotationAngle += deltaDegrees;
        }


        // Il trucco, si tratta di traslazione dall'origine seguita da rototraslazione rispetto al punto di rotazione
        //
        // Pw = World coordinates
        // Po = Origin coordinates respect World coordinates
        // Pr = Rotation point coordinates respect -> Po <-
        // 
        //  Rispetto al punto di Origine Po 
        //  xw' = xw * zoomx - xo
        //  yw' = yw * zoomy - yo
        //
        // Rispetto al punto di rotazione Pr:
        //  |xl| =  | cos sen | | xw' - xr | + |xr|
        //  |yl| =  |-sen cos | | yw' - yr | + |yr|
        // riassumendo:
        //  |xl| =  | cos sen | | xw * zoomx - xo - xr | + |xr|
        //  |yl| =  |-sen cos | | yw * zoomy - yo - yr | + |yr|

        public PointF WorldToLocal(double xw, double yw)
        {
            // Origine rispetto alle coordinate del mondo
            double xt = xw * Zoom.X - Origin.X - RotationPoint.X;
            double yt = yw * Zoom.Y - Origin.Y - RotationPoint.Y;
            double xl = cos * xt + sin * yt + RotationPoint.X;
            double yl = -sin * xt + cos * yt + RotationPoint.Y;
            // Le coordinate sono rispetto al sistema locale 
            return new PointF((float)xl, (float)yl);
        }

        public PointF WorldToLocal(PointF PointWorld)
        {
            return WorldToLocal(PointWorld.X, PointWorld.Y);
        }

        public PointF WorldToLocal(Point PointWorld)
        {
            return WorldToLocal(PointWorld.X, PointWorld.Y);
        }

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

        public PointF LocalToWorld(double xl, double yl)
        {
            double xr = xl - RotationPoint.X;
            double yr = yl - RotationPoint.Y;

            double xww = (cos * xr - sin * yr + RotationPoint.X + Origin.X);
            double yww = (sin * xr + cos * yr + RotationPoint.Y + Origin.Y);

            double xw = Zoom.X != 0 ? xww / Zoom.X : 0;
            double yw = Zoom.Y != 0 ? yww / Zoom.Y : 0;

            return new PointF((float)xw, (float)yw);
        }


        public PointF LocalToWorld(PointF LocalPoint)
        {
            return LocalToWorld(LocalPoint.X, LocalPoint.Y);
        }

        public PointF LocalToWorld(Point LocalPoint)
        {
            return LocalToWorld(LocalPoint.X, LocalPoint.Y);
        }
    }
}
