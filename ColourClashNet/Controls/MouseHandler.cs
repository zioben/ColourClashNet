using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
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

            Point ControlPointOld;

            public PointTrack()
            {
                Reset();
            }

            public void Reset()
            {
                ControlPointClick = ControlPointCurrent = ControlPointOld = ControlPointTranslation = new Point(0, 0);
            }

            public void Set(Point oCurrentPos, bool bHistoryMode )
            {
                if (!bHistoryMode)
                {
                    ControlPointClick = oCurrentPos;
                    ControlPointTranslation = oCurrentPos;
                    ControlPointCurrent = oCurrentPos;
                    ControlPointOld = oCurrentPos;
                }
                else
                {
                    var ControlPointDelta = oCurrentPos.Sub(ControlPointCurrent);
                    ControlPointTranslation = ControlPointTranslation.Add(ControlPointDelta);
                    ControlPointCurrent = oCurrentPos;
                    ControlPointOld = oCurrentPos;
                }
            }

            public void Update(Point oCurrentPos)
            {
                ControlPointCurrent = oCurrentPos;
            }
            public void Follow(Point oCurrentPos,bool bTracking)
            {
                ControlPointCurrent = oCurrentPos;
                if (bTracking)
                {
                    var ControlPointDelta = oCurrentPos.Sub(ControlPointOld);
                    ControlPointOld = ControlPointCurrent;
                    ControlPointTranslation = ControlPointTranslation.Add(ControlPointDelta);
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

        bool bPressed = false;

        bool bStartHistory = false;

        public void Reset()
        {
            PointTrackCurrent.Reset();
            PointTrackHistory.Reset();
            bStartHistory = false;
            bPressed = false;
            MouseState = EnumMouseState.none;
        }

        internal virtual bool OnMouseDown(MouseEventArgs oArgs )
        {
            if( oArgs.Button == MouseButton)
            {
                MouseState = EnumMouseState.push;
                if (!bPressed)
                {
                    bPressed = true;
                    PointTrackCurrent.Set(oArgs.Location, false);
                    if (!bStartHistory)
                    {
                        bStartHistory = true;
                        PointTrackHistory.Set(oArgs.Location, false );
                    }
                    else
                    {
                        PointTrackHistory.Set(oArgs.Location, true);
                    }
                    return true;
                }
            }
            return false;
        }
        internal virtual bool OnMouseUp(MouseEventArgs oArgs)
        {
            if( oArgs.Button == MouseButton)
            {
                PointTrackCurrent.Follow(oArgs.Location,MouseTracking);
                PointTrackHistory.Follow(oArgs.Location,MouseTracking);    
                MouseState = EnumMouseState.release;
                if (bPressed)
                {
                    bPressed = false;
                    return true;
                }
            }
            return false;
        }

        internal virtual bool OnMouseMove(MouseEventArgs oArgs)
        {
            PointTrackCurrent.Update(oArgs.Location);
            PointTrackHistory.Update(oArgs.Location);

            if (bPressed)
            {
                if (MouseTracking)
                {
                    MouseState = EnumMouseState.tracking;
                    PointTrackCurrent.Follow(oArgs.Location, MouseTracking);
                    PointTrackHistory.Follow(oArgs.Location, MouseTracking);
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
