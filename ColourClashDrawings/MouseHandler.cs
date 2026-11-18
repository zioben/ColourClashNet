using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Tracks mouse press, move and release on a control for drag (pan) operations.
    /// <para/>
    /// Two tracks are kept: <see cref="PointTrackCurrent"/> for the current drag and <see cref="PointTrackHistory"/>,
    /// which accumulates the translation of all the drags since the last <see cref="Reset"/>.
    /// </summary>
    public class MouseHandler
    {
        /// <summary>
        /// Mouse interaction state.
        /// </summary>
        public enum EnumMouseState
        { 
            none,
            push,
            move,
            tracking,
            release
        }

        /// <summary>
        /// Positions of a drag operation, in control coordinates.
        /// </summary>
        public class PointTrack
        {
            /// <summary>
            /// Position of the button press.
            /// </summary>
            public Point ControlPointClick { get; internal set; }
            /// <summary>
            /// Accumulated drag translation.
            /// </summary>
            public Point ControlPointTranslation { get; internal set; }
            /// <summary>
            /// Current mouse position.
            /// </summary>
            public Point ControlPointCurrent { get; internal set; }

            Point controlPointOld;

            /// <summary>
            /// Creates a track at the origin.
            /// </summary>
            public PointTrack()
            {
                Reset();
            }

            /// <summary>
            /// Moves all the points to the origin.
            /// </summary>
            public void Reset()
            {
                ControlPointClick = ControlPointCurrent = controlPointOld = ControlPointTranslation = new Point(0, 0);
            }

            /// <summary>
            /// Starts a new drag at <paramref name="currentPos"/>.
            /// </summary>
            /// <param name="currentPos">Mouse position</param>
            /// <param name="historyMode">When true the accumulated translation is kept and updated, otherwise it restarts from the position</param>
            public void Set(Point currentPos, bool historyMode )
            {
                if (!historyMode)
                {
                    ControlPointClick = currentPos;
                    ControlPointTranslation = currentPos;
                    ControlPointCurrent = currentPos;
                    controlPointOld = currentPos;
                }
                else
                {
                    var controlPointDelta = currentPos.Sub(ControlPointCurrent);
                    ControlPointTranslation = ControlPointTranslation.Add(controlPointDelta);
                    ControlPointCurrent = currentPos;
                    controlPointOld = currentPos;
                }
            }

            /// <summary>
            /// Updates the current position only.
            /// </summary>
            public void Update(Point currentPos)
            {
                ControlPointCurrent = currentPos;
            }
            /// <summary>
            /// Updates the current position and, when <paramref name="tracking"/>, adds the movement to the translation.
            /// </summary>
            public void Follow(Point currentPos,bool tracking)
            {
                ControlPointCurrent = currentPos;
                if (tracking)
                {
                    var controlPointDelta = currentPos.Sub(controlPointOld);
                    controlPointOld = ControlPointCurrent;
                    ControlPointTranslation = ControlPointTranslation.Add(controlPointDelta);
                }
            }
        }

        /// <summary>
        /// Track of the current drag.
        /// </summary>
        public PointTrack PointTrackCurrent { get; protected set; } = new();
        /// <summary>
        /// Track accumulated over all drags.
        /// </summary>
        public PointTrack PointTrackHistory { get; protected set; } = new();

        /// <summary>
        /// Current interaction state.
        /// </summary>
        public EnumMouseState MouseState { get; protected set; } =  EnumMouseState.none;
        /// <summary>
        /// Button that starts a drag.
        /// </summary>
        public MouseButtons MouseButton { get; set; } = MouseButtons.Left;
        /// <summary>
        /// Enables the drag translation tracking.
        /// </summary>
        public bool MouseTracking { get; set; } = true;
        /// <summary>
        /// Normalized rectangle between two corners.
        /// </summary>
        protected Rectangle ToRectangle( Point p1, Point p2) => new Rectangle( Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        /// <summary>
        /// Normalized rectangle between two corners.
        /// </summary>
        protected Rectangle ToRectangle(PointF p1, PointF p2) => new Rectangle((int)Math.Min(p1.X, p2.X), (int)Math.Min(p1.Y, p2.Y), (int)Math.Abs(p2.X - p1.X), (int)Math.Abs(p2.Y - p1.Y));
        /// <summary>
        /// Normalized rectangle between two corners.
        /// </summary>
        protected RectangleF ToRectangleF(Point p1, Point p2) => new RectangleF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        /// <summary>
        /// Normalized rectangle between two corners.
        /// </summary>
        protected RectangleF ToRectangleF(PointF p1, PointF p2) => new RectangleF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));

        bool pressed = false;

        bool startHistory = false;

        /// <summary>
        /// Resets both tracks and the state.
        /// </summary>
        public void Reset()
        {
            PointTrackCurrent.Reset();
            PointTrackHistory.Reset();
            startHistory = false;
            pressed = false;
            MouseState = EnumMouseState.none;
        }

        /// <summary>
        /// Handles a button press.
        /// </summary>
        /// <returns>True when a drag starts.</returns>
        internal virtual bool OnMouseDown(MouseEventArgs args )
        {
            if( args.Button == MouseButton)
            {
                MouseState = EnumMouseState.push;
                if (!pressed)
                {
                    pressed = true;
                    PointTrackCurrent.Set(args.Location, false);
                    if (!startHistory)
                    {
                        startHistory = true;
                        PointTrackHistory.Set(args.Location, false );
                    }
                    else
                    {
                        PointTrackHistory.Set(args.Location, true);
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Handles a button release.
        /// </summary>
        /// <returns>True when a drag ends.</returns>
        internal virtual bool OnMouseUp(MouseEventArgs args)
        {
            if( args.Button == MouseButton)
            {
                PointTrackCurrent.Follow(args.Location,MouseTracking);
                PointTrackHistory.Follow(args.Location,MouseTracking);    
                MouseState = EnumMouseState.release;
                if (pressed)
                {
                    pressed = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles a mouse move.
        /// </summary>
        /// <returns>Always true.</returns>
        internal virtual bool OnMouseMove(MouseEventArgs args)
        {
            PointTrackCurrent.Update(args.Location);
            PointTrackHistory.Update(args.Location);

            if (pressed)
            {
                if (MouseTracking)
                {
                    MouseState = EnumMouseState.tracking;
                    PointTrackCurrent.Follow(args.Location, MouseTracking);
                    PointTrackHistory.Follow(args.Location, MouseTracking);
                }
            }
            else
            {
                MouseState = EnumMouseState.move;
            }
            return true;
        }
    }
}
