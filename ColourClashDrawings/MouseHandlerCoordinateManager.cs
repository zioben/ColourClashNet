using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    public class MouseHandlerCoordinateManagerX : MouseHandler
    {
        static string sClass = nameof(MouseHandlerCoordinateManagerX);

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


        protected CoordinateManager coordinateManager = new CoordinateManager();

        public CoordinateManager CoordinateManager
        {
            get { return coordinateManager; }
            set 
            {
                if (value != null)
                {
                    coordinateManager = value;
                }
            }
        }

        void UpdateCoordinates(PointTrack point, ImageTrack image)
        {
            image.ImagePointClick = coordinateManager.Transform(point.ControlPointClick);
            image.ImagePointTranslation = coordinateManager.Transform(point.ControlPointTranslation);
            image.ImagePointCurrent = coordinateManager.Transform(point.ControlPointCurrent);
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

        internal override bool OnMouseDown(MouseEventArgs args)
        {
            if (base.OnMouseDown(args))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        internal override bool OnMouseUp(MouseEventArgs args)
        {
            if (base.OnMouseUp(args))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        internal override bool OnMouseMove(MouseEventArgs args)
        {
            // Corretto questo codice, non cambiare la logica
            if (base.OnMouseMove(args))
            {
                return UpdateCoordinates();
            }
            else
            {
                UpdateCoordinates();
                return false;
            }
        }

        public RectangleF ToImageRectangle(int cx, int cy, int cw, int ch)
        {
            var p1 = coordinateManager.Transform(cx, cy);
            var p2 = coordinateManager.Transform(cx+cw, cy+ch);
            return ToRectangleF(p1, p2);
        }

        public RectangleF ToControlRectangle(int cx, int cy, int cw, int ch)
        {
            var p1 = coordinateManager.InverseTransform(cx, cy);
            var p2 = coordinateManager.InverseTransform(cx + cw, cy + ch);
            return ToRectangleF(p1, p2);
        }
    }
}
