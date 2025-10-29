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

        public void SetTrasfZoom(float fX, float fY)
        {
            TransfZoom = new PointF(fX,fY);
        }
        public void SetTransfZoom(int iX, int iY) => SetTrasfZoom(iX,iY);

        public void SetTransfZoom(double dX, double dY) => SetTrasfZoom((float)dX, (float)dY);
        public void SetTransfZoom(float fScale) => SetTrasfZoom(fScale, fScale);
        public void SetTransfZoom(int iScale) => SetTransfZoom(iScale, iScale);
        public void SetTransfZoom(double dScale) => SetTransfZoom(dScale, dScale);

        public void SetTransfZoom(Point oP)
        {
            TransfZoom = new PointF( oP.X, oP.Y );   
        }
        public void SetTransfZoom(PointF oP)
        {
            TransfZoom = oP;
        }

        public void SetTransfZoom(float fScaleWX, float fScaleWY, float fScaleLX, float fScaleLY )
        {
            TransfZoom = new PointF(
                fScaleLX != 0 ? fScaleWX / fScaleLX : 0,
                fScaleLY != 0 ? fScaleWY / fScaleLY : 0);
        }
        public void SetTransfZoom(int iScaleWX, int iScaleWY, int iScaleLX, int iScaleLY) 
            => SetTransfZoom(iScaleWX,iScaleWY, iScaleLX, (float) iScaleLY);
        public void SetTransfZoom(double dScaleWX, double dScaleWY, double dScaleLX, double dScaleLY)
            => SetTransfZoom((float)dScaleWX, (float)dScaleWY, (float)dScaleLX, (float)dScaleLY);

        //--------------------------------------------------------------------------------
        public void TranslateWorldOrigin(PointF oP)
        {
            WorldOrigin = WorldOrigin.Sub(oP);
        }
        public void TranslateWorldOrigin(Point oP)
        {
            WorldOrigin = WorldOrigin.Sub(oP);
        }
        public void TranslateWorldOrigin(double X, double Y)
            => TranslateWorldOrigin(new PointF((float)X, (float)Y));
        public void TranslateWorldOrigin(float X, float Y)
            => TranslateWorldOrigin(new PointF(X, Y));
        public void TranslateWorldOrigin(int X, int Y)
            => TranslateWorldOrigin(new PointF(X, Y));

        //--------------------------------------------------------------------------------
        public void TranslateRotationPoint(PointF oP)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(oP);
        }
        public void TranslateRotationPoint(Point oP)
        {
            WorldRotationPoint = WorldRotationPoint.Sub(oP);
        }
        public void TranslateRotationPoint(double X, double Y)
          => TranslateRotationPoint(new PointF((float)X, (float)Y));
        public void TranslateRotationPoint(float X, float Y)
            => TranslateRotationPoint(new PointF(X, Y));
        public void TranslateRotationPoint(int X, int Y)
            => TranslateRotationPoint(new PointF(X, Y));

        //--------------------------------------------------------------------------------
        public void AddRotationAngle(double deltaDegrees)
        {
            WorldRotationAngle += deltaDegrees;
        }

        //--------------------------------------------------------------------------------
        public void TranslateTrasformationOrigin(PointF oP)
        {
            TransfOrigin = TransfOrigin.Sub(oP);
        }
        public void TranslateTrasformationOrigin(Point oP)
        {
            TransfOrigin = TransfOrigin.Sub(oP);
        }
        public void TranslateTrasformationOrigin(double X, double Y)
            => TranslateTrasformationOrigin(new PointF((float)X, (float)Y));
        public void TranslateTrasformationOrigin(float X, float Y)
            => TranslateTrasformationOrigin(new PointF(X, Y));
        public void TranslateTrasformationOrigin(int X, int Y)
            => TranslateTrasformationOrigin(new PointF(X, Y));


        public PointF Transform(PointF oP)
        {
            // Tralsare sull'origine rototraslazione
            PointF PL = oP.Sub(WorldOrigin).Sub(WorldRotationPoint);
            // Ruotare punti
            double xl = cos * PL.X + sin * PL.Y;
            double yl = -sin * PL.X + cos * PL.Y;
            // Tralsare sull'origine mondo
            PointF PLL = new PointF((float)xl, (float)yl).Add(WorldRotationPoint).MulP2P(TransfZoom).Add(TransfOrigin);
            return PLL;
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

        public PointF InverseTransform(PointF oP)
        {
            var PL = oP.Sub(TransfOrigin).DivP2P(TransfZoom).Sub(WorldRotationPoint);
            //
            double xl = cos * PL.X - sin * PL.Y;
            double yl = sin * PL.X + cos * PL.Y;
            PL = new PointF((float)xl, (float)yl).Add(WorldRotationPoint).Add(WorldOrigin);
            return PL;            
        }

        public PointF InverseTransform(Point oP)
            => InverseTransform(new PointF((float) oP.X, (float)oP.Y));
        public PointF InverseTransform(double trX,double trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        public PointF InverseTransform(float trX, float trY)
            => InverseTransform(new PointF((float)trX, (float)trY));
        public PointF InverseTransform(int trX, int trY)
            => InverseTransform(new PointF((float)trX, (float)trY));

      
        Matrix RebuildTransform(Graphics oG)
        {
            var oldTR = oG.Transform;
            oG.Transform = new Matrix();
            oG.ResetTransform();
            oG.TranslateTransform(WorldOrigin.X, WorldOrigin.Y, MatrixOrder.Append);
            oG.TranslateTransform(WorldRotationPoint.X, WorldRotationPoint.Y, MatrixOrder.Append);
            oG.RotateTransform((float)WorldRotationAngle, MatrixOrder.Append);
            oG.TranslateTransform(-WorldRotationPoint.X, -WorldRotationPoint.Y, MatrixOrder.Append);
            oG.ScaleTransform(TransfZoom.X, TransfZoom.Y);
            return oldTR;
        }

        public void DrawImage(Graphics oG, Image oImage)
        {
            if (oImage == null || oG == null)
            {
                return;
            }
            var oldTR = RebuildTransform(oG);
            // This transaltion is affected by the Zoom Parameters.
            // To maintains "Original" world size to follow mouse pointer, reversing the zoom is necessaty
            var oPTO = TransfOrigin.DivP2P(TransfZoom);
            oG.DrawImage(oImage, oPTO);
            oG.Transform = oldTR;
        }
    }
}
