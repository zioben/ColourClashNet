using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{

    public partial class BitmapRender : Component
    {
        public enum EnumZoom
        {
            ZoomQ,
            ZoomH,
            Zoom1,
            Zoom2,
            Zoom3,
            Zoom4,
            Fit,
            FitW,
            FitH,
            Manual,
            Stretch
        } 

        public BitmapRender()
        {
            InitializeComponent();
            Create();
        }

        public BitmapRender(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            Create();
        }
        protected void Create()
        {
            MouseMovingButton = MouseButtons.Left;
            InitializeComponent();
            RegisterToolStripItems();
        }

        #region ToolStripItemZoom

        void RegisterToolStripItems()
        {
            this.toolStripMenuItem1_4.Click += new EventHandler(OnZoom14);
            this.toolStripMenuItem1_2.Click += new EventHandler(OnZoom12);
            this.toolStripMenuItem1_1.Click += new EventHandler(OnZoom11);
            this.toolStripMenuItem2_1.Click += new EventHandler(OnZoom21);
            this.toolStripMenuItem3_1.Click += new EventHandler(OnZoom31);
            this.toolStripMenuItem4_1.Click += new EventHandler(OnZoom41);
            this.toolStripMenuItemFit.Click += new EventHandler(OnZoomF);
            this.toolStripMenuItemFitW.Click += new EventHandler(OnZoomFW);
            this.toolStripMenuItemFitH.Click += new EventHandler(OnZoomFH);
            this.toolStripMenuItemStretch.Click += new EventHandler(OnZoomS);
            this.toolStripMenuItemManual.Click += new EventHandler(OnZoomM);
            this.toolStripMenuItemBlockMoving.Click += new EventHandler(OnBlockMoving);
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        void SetZoom(EnumZoom z)
        {
            ImageZoomMode = z;
            if (oControl != null)
            {
                oControl.Refresh();
            }
        }

        void OnZoom14(object sender, EventArgs e) { SetZoom(EnumZoom.ZoomQ); }
        void OnZoom12(object sender, EventArgs e) { SetZoom(EnumZoom.ZoomH); }
        void OnZoom11(object sender, EventArgs e) { SetZoom(EnumZoom.Zoom1); }
        void OnZoom21(object sender, EventArgs e) { SetZoom(EnumZoom.Zoom2); }
        void OnZoom31(object sender, EventArgs e) { SetZoom(EnumZoom.Zoom3); }
        void OnZoom41(object sender, EventArgs e) { SetZoom(EnumZoom.Zoom4); }
        void OnZoomF(object sender, EventArgs e) { SetZoom(EnumZoom.Fit); }
        void OnZoomFW(object sender, EventArgs e) { SetZoom(EnumZoom.FitW); }
        void OnZoomFH(object sender, EventArgs e) { SetZoom(EnumZoom.FitH); }
        void OnZoomS(object sender, EventArgs e) { SetZoom(EnumZoom.Stretch); }
        void OnZoomM(object sender, EventArgs e) { SetZoom(EnumZoom.Manual); }

        void OnBlockMoving(object sender, EventArgs e)
        {
            ImageBlockScroll = !ImageBlockScroll;
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        #endregion

        #region RAD Setup

        Image oImage;
        [Browsable(true), Category("Appearance")]
        public Image Image
        {
            get { return oImage; }
            set
            {
                oImage = value;
                if (value != null)
                {
                    if (oControl != null)
                    {
                        oControl.Refresh();
                    }
                }
            }
        }

        Control oControl;
        [Browsable(true), Category("Appearance")]
        public Control Control
        {
            get { return oControl; }
            set
            {
                if (oControl != value)
                {
                    if (oControl != null)
                    {
                        oControl.ContextMenuStrip = null;
                        oControl.Paint -= OnPaint;
                        oControl.MouseUp -= OnMouseUp;
                        oControl.MouseDown -= OnMouseDown;
                        oControl.MouseMove -= OnMouseMove;
                        oControl.MouseWheel -= OnMouseWheel;
                    }
                    oControl = value;
                    if (oControl != null)
                    {
                        oControl.ContextMenuStrip = oContextMenuStrip;
                        oControl.Paint += OnPaint;
                        oControl.MouseUp += OnMouseUp;
                        oControl.MouseDown += OnMouseDown;
                        oControl.MouseMove += OnMouseMove;
                        oControl.MouseWheel += OnMouseWheel;
                        oControl.Refresh();
                    }
                }
            }
        }

        [Browsable(true), Category("Appearance")]
        public Boolean ImageBlockScroll { get; set; }

        [Browsable(true), Category("Appearance")]
        public EnumZoom ImageZoomMode { get; set; }

        [Browsable(true), Category("Appearance")]
        public Boolean ImageMoveOverControlBorder { get; set; }

        [Browsable(true), Category("Appearance")]
        public float ImageZoomManual
        {
            get { return RoiZoomM; }
            set { if (value > 0) RoiZoomM = value; ForceRefresh(); }
        }

        #endregion

        #region Render Code

        SolidBrush BrushBlack = new SolidBrush(System.Drawing.Color.Black);
        RectangleF RoiDst = new RectangleF();
        RectangleF RoiSrc = new RectangleF();
        //public float RoiZoomX = 1;
        //public float RoiZoomY = 1;
        public float RoiZoomM = 1;

        [Browsable(true), Category("Appearance")]
        public float ZoomControlX
        {
            get { SetRoiAndZoom(); return (float)(BitmapCoordinates.Zoom.X != 0 ? 1.0f / BitmapCoordinates.Zoom.X : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomControlY
        {
            get { SetRoiAndZoom(); return (float)(BitmapCoordinates.Zoom.Y != 0 ? 1.0f / BitmapCoordinates.Zoom.Y : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomImageX
        {
            get { SetRoiAndZoom(); return (float)(BitmapCoordinates.Zoom.X); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomImageY
        {
            get { SetRoiAndZoom(); return (float)(BitmapCoordinates.Zoom.Y); }
        }


        void SetRoiAndZoom()
        {
            float RoiZoomX = 0;
            float RoiZoomY = 0;
            BitmapCoordinates.Zoom = new PointF(1, 1);
            if (Control == null || Control.Size.Width <= 0 || Control.Size.Height <= 0 || Image == null || Image.Size.Width <= 0 && Image.Size.Height <= 0) return;
            // Il + facile
            // Determina la roi da disegnare
            float fiw = Control.Size.Width;
            float fih = Control.Size.Height;
            // Stretch Zoom
            RoiZoomX = (float)Image.Size.Width / fiw;
            RoiZoomY = (float)Image.Size.Height / fih;
            switch (ImageZoomMode)
            {
                // Zoom: restringe o allarga la roi
                case EnumZoom.ZoomQ: RoiZoomX = RoiZoomY = 1.0f * 4; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                case EnumZoom.ZoomH: RoiZoomX = RoiZoomY = 1.0f * 2; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                case EnumZoom.Zoom1: RoiZoomX = RoiZoomY = 1.0f / 1; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                case EnumZoom.Zoom2: RoiZoomX = RoiZoomY = 1.0f / 2; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                case EnumZoom.Zoom3: RoiZoomX = RoiZoomY = 1.0f / 3; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                case EnumZoom.Zoom4: RoiZoomX = RoiZoomY = 1.0f / 4; fiw *= RoiZoomX; fih *= RoiZoomY; break;
                // Stretch: 
                case EnumZoom.Stretch: fiw = (float)Image.Size.Width; fih = (float)Image.Size.Height; break;
                // Fit w e h
                // w : 100 = h : x
                case EnumZoom.FitW:
                    RoiZoomY = RoiZoomX;
                    fiw = (float)Image.Size.Width;
                    fih *= RoiZoomY;
                    break;
                case EnumZoom.FitH:
                    RoiZoomX = RoiZoomY;
                    fiw *= RoiZoomY;
                    fih = (float)Image.Size.Height;
                    break;
                // Fit è il + cancaro
                case EnumZoom.Fit:
                    {
                        if (RoiZoomX > RoiZoomY)
                        {
                            RoiZoomY = RoiZoomX;
                            fiw = (float)Image.Size.Width;
                            fih *= RoiZoomY;
                        }
                        else
                        {
                            RoiZoomX = RoiZoomY;
                            fiw *= RoiZoomY;
                            fih = (float)Image.Size.Height;
                        }
                    }
                    break;
                case EnumZoom.Manual: RoiZoomX = RoiZoomY = RoiZoomM; fiw *= RoiZoomM; fih *= RoiZoomM; break;
            }
            BitmapCoordinates.Zoom = new PointF( (float)RoiZoomX,(float)RoiZoomY);
            RoiDst.X = RoiDst.Y = 0;
            RoiDst.Width = Control.Size.Width;
            RoiDst.Height = Control.Size.Height;
            RoiSrc.X = -(float)BitmapCoordinates.Origin.X;
            RoiSrc.Y = -(float)BitmapCoordinates.Origin.Y;
            RoiSrc.Width = fiw;
            RoiSrc.Height = fih;
        }

        public event PaintEventHandler Paint;

        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (Control != null && Image != null)
            {
                SetRoiAndZoom();
                e.Graphics.DrawImage(Image, RoiDst, RoiSrc, GraphicsUnit.Pixel);
                if (Paint != null) Paint(sender, e);
            }
        }
        #endregion

        #region Moving Origin Point

        protected void ForceRefresh()
        {
            if (oControl != null)
            {
                oControl.Invalidate();
            }
        }

        protected Coordinates2D BitmapCoordinates = new Coordinates2D();

        public void WorldOriginMove(double deltaX, double deltaY)
        {
            if (!ImageBlockScroll)
            {
                SetRoiAndZoom();
                BitmapCoordinates.MoveOriginPointRespectWorldCoordinates(deltaX, deltaY);
                ForceRefresh();
            }
        }

        public void WorldOriginMove(Point Delta)
        {
            WorldOriginMove(Delta.X, Delta.Y);
        }

        public void WorldOriginMove(PointF Delta)
        {
            WorldOriginMove(Delta.X, Delta.Y);
        }

        public void OriginZero()
        {
            BitmapCoordinates.Origin = new PointF(0, 0);
            ForceRefresh();
        }

        public PointF PointControlToPointBitmap(Point p)
        {
            if (oControl != null && Image != null)
            {
                return BitmapCoordinates.WorldToLocal(p);
            }
            return new PointF(-1, -1);
        }

        #endregion

        #region MouseTracker

        [Browsable(true), Category("Appearance")]
        public MouseButtons MouseMovingButton { get; set; }

        protected Boolean MousePushed = false;
        protected Point MousePushedPoint = new Point();
        protected Point MousePushedPointLast = new Point();

        protected void OnMouseWheel(object Sender, MouseEventArgs args)
        {
            if (args.Delta != 0)
            {
                ImageZoomManual += 0.5f * args.Delta;
                ImageZoomMode = EnumZoom.Manual;
            }
        }

        protected void OnMouseDown(object Sender, MouseEventArgs args)
        {
            if (args.Button == MouseMovingButton)
            {
                if (!MousePushed)
                {
                    MousePushed = true;
                    MousePushedPoint = args.Location;
                    MousePushedPointLast = args.Location;
                }
            }
        }

        protected void OnMouseUp(object Sender, MouseEventArgs args)
        {
            if (MousePushed)
            {
                MousePushed = false;
            }
        }

        public event MouseEventHandler MouseMove;

        protected void OnMouseMove(object Sender, MouseEventArgs args)
        {
            if (MouseMove != null) MouseMove(Sender, args);
            else
            {
                if (MousePushed)
                {
                    int DX = MousePushedPointLast.X - args.Location.X;
                    int DY = MousePushedPointLast.Y - args.Location.Y;
                    WorldOriginMove(DX, DY);
                    MousePushedPointLast = args.Location;
                }
            }
        }

        #endregion
    }
}
