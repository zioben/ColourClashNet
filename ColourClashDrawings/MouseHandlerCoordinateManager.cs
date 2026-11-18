using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Mouse handler that also converts the tracked positions to image coordinates through a <see cref="Drawing.CoordinateManager"/>.
    /// </summary>
    public class MouseHandlerCoordinateManagerX : MouseHandler
    {
        static string sClass = nameof(MouseHandlerCoordinateManagerX);

        /// <summary>
        /// Positions of a drag operation, in image coordinates.
        /// </summary>
        public class ImageTrack
        {
            /// <summary>
            /// Position of the button press.
            /// </summary>
            public PointF ImagePointClick { get; internal set; }
            /// <summary>
            /// Accumulated drag translation.
            /// </summary>
            public PointF ImagePointTranslation { get; internal set; }
            /// <summary>
            /// Current mouse position.
            /// </summary>
            public PointF ImagePointCurrent { get; internal set; }

            /// <summary>
            /// Creates a track at the origin.
            /// </summary>
            public ImageTrack() 
            {
                Reset();
            }

            /// <summary>
            /// Moves all the points to the origin.
            /// </summary>
            public void Reset()
            { 
                ImagePointClick = ImagePointTranslation = ImagePointCurrent = new PointF();
            }

        }

        /// <summary>
        /// Image track of the current drag.
        /// </summary>
        public ImageTrack ImageTrackCurrentX { get; protected set; } = new ImageTrack();   
        /// <summary>
        /// Image track accumulated over all drags.
        /// </summary>
        public ImageTrack ImageTrackHistoryX { get; protected set; } = new ImageTrack();

        /// <summary>
        /// Resets control and image tracks.
        /// </summary>
        public new void Reset()
        { 
            base.Reset();
            ImageTrackCurrentX.Reset();
            ImageTrackHistoryX.Reset();
        }


        /// <summary>
        /// Control to image coordinate conversion.
        /// </summary>
        protected CoordinateManager coordinateManager = new CoordinateManager();

        /// <summary>
        /// Control to image coordinate conversion; null values are ignored.
        /// </summary>
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

        /// <summary>
        /// Converts a control track into an image track.
        /// </summary>
        void UpdateCoordinates(PointTrack point, ImageTrack image)
        {
            image.ImagePointClick = coordinateManager.Transform(point.ControlPointClick);
            image.ImagePointTranslation = coordinateManager.Transform(point.ControlPointTranslation);
            image.ImagePointCurrent = coordinateManager.Transform(point.ControlPointCurrent);
        }

        /// <summary>
        /// Converts both tracks.
        /// </summary>
        /// <returns>False on error.</returns>
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

        /// <inheritdoc/>
        internal override bool OnMouseDown(MouseEventArgs args)
        {
            if (base.OnMouseDown(args))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        /// <inheritdoc/>
        internal override bool OnMouseUp(MouseEventArgs args)
        {
            if (base.OnMouseUp(args))
            {
                return UpdateCoordinates();
            }
            return false;
        }

        /// <inheritdoc/>
        internal override bool OnMouseMove(MouseEventArgs args)
        {
            // Verified logic, do not change: coordinates are refreshed on every move, even when not tracking
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

        /// <summary>
        /// Converts a control rectangle into an image rectangle.
        /// </summary>
        public RectangleF ToImageRectangle(int cx, int cy, int cw, int ch)
        {
            var p1 = coordinateManager.Transform(cx, cy);
            var p2 = coordinateManager.Transform(cx+cw, cy+ch);
            return ToRectangleF(p1, p2);
        }

        /// <summary>
        /// Converts an image rectangle into a control rectangle.
        /// </summary>
        public RectangleF ToControlRectangle(int cx, int cy, int cw, int ch)
        {
            var p1 = coordinateManager.InverseTransform(cx, cy);
            var p2 = coordinateManager.InverseTransform(cx + cw, cy + ch);
            return ToRectangleF(p1, p2);
        }
    }
}
