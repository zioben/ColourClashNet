using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Drawing
{
    public class MouseHandler
    {
        public enum EnumMouseState
        { 
            none,
            push,
            move,
            tracking,
            release
        }

        public class PointTrack
        {
            public Point ControlPointClick { get; internal set; }
            public Point ControlPointTranslation { get; internal set; }
            public Point ControlPointCurrent { get; internal set; }

            Point controlPointOld;

            public PointTrack()
            {
                Reset();
            }

            public void Reset()
            {
                ControlPointClick = ControlPointCurrent = controlPointOld = ControlPointTranslation = new Point(0, 0);
            }

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

            public void Update(Point currentPos)
            {
                ControlPointCurrent = currentPos;
            }
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

        public PointTrack PointTrackCurrent { get; protected set; } = new();
        public PointTrack PointTrackHistory { get; protected set; } = new();

        public EnumMouseState MouseState { get; protected set; } =  EnumMouseState.none;
        public MouseButtons MouseButton { get; set; } = MouseButtons.Left;
        public bool MouseTracking { get; set; } = true;
        protected Rectangle ToRectangle( Point p1, Point p2) => new Rectangle( Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        protected Rectangle ToRectangle(PointF p1, PointF p2) => new Rectangle((int)Math.Min(p1.X, p2.X), (int)Math.Min(p1.Y, p2.Y), (int)Math.Abs(p2.X - p1.X), (int)Math.Abs(p2.Y - p1.Y));
        protected RectangleF ToRectangleF(Point p1, Point p2) => new RectangleF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        protected RectangleF ToRectangleF(PointF p1, PointF p2) => new RectangleF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));

        bool pressed = false;

        bool startHistory = false;

        public void Reset()
        {
            PointTrackCurrent.Reset();
            PointTrackHistory.Reset();
            startHistory = false;
            pressed = false;
            MouseState = EnumMouseState.none;
        }

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
