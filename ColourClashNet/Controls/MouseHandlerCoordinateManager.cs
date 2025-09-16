using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
{
    public class MouseHandlerCoordinateManager : MouseHandlerBase
    {
        static string sClass = nameof(MouseHandlerCoordinateManager);

        public class ImageTrack
        {
            public PointF ImagePointClick { get; internal set; }
            public PointF ImagePointTranslation { get; internal set; }
            public PointF ImagePointCurrent { get; internal set; }

            public ImageTrack() 
            {
                Reset();
            }

            public void Reset()
            { 
                ImagePointClick = ImagePointTranslation = ImagePointCurrent = new PointF();
            }

        }

        public ImageTrack ImageTrackCurrentX { get; protected set; } = new ImageTrack();   
        public ImageTrack ImageTrackHistoryX { get; protected set; } = new ImageTrack();

        public new void Reset()
        { 
            base.Reset();
            ImageTrackCurrentX.Reset();
            ImageTrackHistoryX.Reset();
        }


        protected CoordinateManager oCMan = new CoordinateManager();

        public CoordinateManager CoordinateManager
        {
            get { return oCMan; }
            set 
            {
                if (value != null)
                {
                    oCMan = value;
                }
            }
        }

        void UpdateCoordinates(PointTrack oPoint, ImageTrack oImage)
        {
            oImage.ImagePointClick = oCMan.WorldToLocal(oPoint.ControlPointClick);
            oImage.ImagePointTranslation = oCMan.WorldToLocal(oPoint.ControlPointTranslation);
            oImage.ImagePointCurrent = oCMan.WorldToLocal(oPoint.ControlPointCurrent);
        }

        bool UpdateCoordinates()
        {
            string sMethod = nameof(UpdateCoordinates);
            try
            {
                UpdateCoordinates(PointTrackCurrent, ImageTrackCurrentX);
                UpdateCoordinates(PointTrackHistory, ImageTrackHistoryX);
                return true;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass,sMethod, ex);
                return false;
            }
        }

        internal override bool OnMouseDown(MouseEventArgs oArgs)
        {
            if (base.OnMouseDown(oArgs))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        internal override bool OnMouseUp(MouseEventArgs oArgs)
        {
            if (base.OnMouseUp(oArgs))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        internal override bool OnMouseMove(MouseEventArgs oArgs)
        {
            // Corretto questo codice, non cambiare la logica
            if (base.OnMouseMove(oArgs))
            {
                return UpdateCoordinates();
            }
            else
            {
                UpdateCoordinates();
                return false;
            }
        }

        public RectangleF ToImageRectangle(int iCX, int iCY, int iCW, int iCH)
        {
            var P1 = oCMan.WorldToLocal(iCX, iCY);
            var P2 = oCMan.WorldToLocal(iCX+iCW, iCY+iCH);
            return( ToRectangleF(P1, P2));
        }

        public RectangleF ToControlRectangle(int iCX, int iCY, int iCW, int iCH)
        {
            var P1 = oCMan.LocalToWorld(iCX, iCY);
            var P2 = oCMan.LocalToWorld(iCX + iCW, iCY + iCH);
            return (ToRectangleF(P1, P2));
        }
    }
}
