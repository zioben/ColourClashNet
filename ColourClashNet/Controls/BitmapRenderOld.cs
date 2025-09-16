using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{


    public partial class BitmapRenderOld : Component
    {
        #region Enums

        /// <summary>
        /// Zoom modes
        /// </summary>
        public enum EnumZoom
        {
            ZoomQ = 0,
            ZoomH = 1,
            Zoom1 = 2,
            Zoom2 = 3,
            Zoom3 = 4,
            Zoom4 = 5,
            Fit = 6,
            FitW = 7,
            FitH = 8,
            Stretch = 9,
            Manual = 100
        }

        /// <summary>
        /// ROI modes
        /// </summary>
        public enum EnumRoiMode
        {
            Disabled,
            Rectangle,
            Polygon,
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BitmapRenderOld()
        {
            InitializeComponent();
            Create();
        }

        /// <summary>
        /// Constructor with container
        /// </summary>
        /// <param name="container"></param>
        public BitmapRenderOld(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            Create();
        }

        /// <summary>
        /// Common create code
        /// </summary>
        protected void Create()
        {
            MouseMovingButton = MouseButtons.Left;
            InitializeComponent();
            RegisterToolStripItems();
        }

        #endregion

        #region ROI Picking        

        EnumRoiMode eRoiMode = EnumRoiMode.Rectangle;
        List<PointF> RoiRectanglePointList = new List<PointF>();
        List<PointF> RoiPolygonPointList = new List<PointF>();

        public EnumRoiMode MouseRoiMode
        {
            get { return eRoiMode; }
            set
            {
                if (eRoiMode != value)
                {
                    eRoiMode = value;
                    UpdateControlAndImageRoi();
                }
            }
        }

        void ResetRoi()
        {
            RoiRectanglePointList.Clear();
            RoiPolygonPointList.Clear();
            ForceRefresh();
        }

        void HandleMouseRoi(MouseEventArgs args)
        {
        }

        #endregion


        #region Color Picking

        /// <summary>
        /// If true the color under the mouse pointer is tracked
        /// </summary>
        public bool MouseColorPeek { get; set; }

        /// <summary>
        /// Color under the mouse pointer
        /// </summary>
        public System.Drawing.Color MouseColor { get; private set; }

        System.Drawing.Color HandleMouseColorPick(Point oMousePoint)
        {
            var MouseCoordinates = oMousePoint;
            var ImageCoordinates = PointControlToPointBitmap(oMousePoint);
            if (Image == null)
            {
                MouseColor = System.Drawing.Color.Transparent;
                return MouseColor;
            }
            if (MouseColorPeek)
            {
                if (Image is Bitmap oBmp)
                {
                    var ImgX = ImageCoordinates.X;
                    var ImgY = ImageCoordinates.Y;
                    if (ImgX >= 0 && ImgX < oBmp.Width)
                    {
                        if (ImgY >= 0 && ImgY < oBmp.Height)
                        {
                            MouseColor = oBmp.GetPixel((int)ImgX, (int)ImgY);
                            return MouseColor;
                        }
                    }
                }
            }
            MouseColor = System.Drawing.Color.Transparent;
            return MouseColor;

        }

        /// <summary>
        /// List of selected colors
        /// </summary>
        public List<System.Drawing.Color> SelectedColors { get; private set; } = new List<System.Drawing.Color>();

        System.Drawing.Color oContextMenuColor = System.Drawing.Color.Transparent;
        public void AddMouseSelectedColor(System.Drawing.Color oColor)
        {
            if (!SelectedColors.Contains(oColor))
            {
                SelectedColors.Add(oColor);
            }
        }

        public void ResetMouseSelectedColors()
        {
            SelectedColors.Clear();
        }

        public void RemoveMouseSelectedColor(System.Drawing.Color oColor)
        {
            if (!SelectedColors.Contains(oColor))
            {
                SelectedColors.Remove(oColor);
            }
        }

        #endregion


        #region Mouse Movement

        /// <summary>
        /// Mouse coordinates in the control
        /// </summary>
        //public Point MouseCoordinates { get; set; }

        /// <summary>
        /// Mouse coordinates in the image
        /// </summary>
        //public PointF ImageCoordinates { get; set; }


    
        #endregion


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
            this.toolStripMenuItemAddColor.Click += new EventHandler(OnAddColor);
            this.toolStripMenuItemResetColors.Click += new EventHandler(OnResetColors);
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

        void RebuildMouseColorItems()
        {
            toolStripMenuItemColors.DropDownItems.Clear();
            foreach (var c in SelectedColors)
            {
                var tsi = new ToolStripMenuItem();
                tsi.Text = $"R:{c.R} G:{c.G} B:{c.B}";
                tsi.BackColor = c;
                tsi.ForeColor = (c.R + c.G + c.B) / 3 < 128 ? System.Drawing.Color.White : System.Drawing.Color.Black;
                tsi.Click += (s, e) =>
                {
                    RemoveMouseSelectedColor(c);
                    RebuildMouseColorItems();
                    ColorRemoved?.Invoke(this, EventArgs.Empty);
                };
                toolStripMenuItemColors.DropDownItems.Add(tsi);
            }
            toolStripMenuItemColors.Enabled = SelectedColors.Count > 0;
        }

        void OnAddColor(object sender, EventArgs e)
        {
            if (oContextMenuColor != System.Drawing.Color.Transparent)
            {
                AddMouseSelectedColor(oContextMenuColor);
            }
            RebuildMouseColorItems();
            ColorAdded?.Invoke(this, EventArgs.Empty);
        }
        void OnResetColors(object sender, EventArgs e)
        {
            ResetMouseSelectedColors();
            RebuildMouseColorItems();
            ColorRemoved?.Invoke(this, EventArgs.Empty);
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

        #region Clip and Zoom Code

        SolidBrush BrushBlack = new SolidBrush(System.Drawing.Color.Black);
        RectangleF RoiControl = new RectangleF();
        RectangleF RoiImgage = new RectangleF();
        float RoiZoomM = 1;

        [Browsable(true), Category("Appearance")]
        public float ZoomControlX
        {
            get { UpdateControlAndImageRoi(); return (float)(oCoordinateManager.Zoom.X != 0 ? 1.0f / oCoordinateManager.Zoom.X : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomControlY
        {
            get { UpdateControlAndImageRoi(); return (float)(oCoordinateManager.Zoom.Y != 0 ? 1.0f / oCoordinateManager.Zoom.Y : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomImageX
        {
            get { UpdateControlAndImageRoi(); return (float)(oCoordinateManager.Zoom.X); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomImageY
        {
            get { UpdateControlAndImageRoi(); return (float)(oCoordinateManager.Zoom.Y); }
        }


        void UpdateControlAndImageRoi()
        {

            float RoiZoomX = 0;
            float RoiZoomY = 0;
            oCoordinateManager.Zoom = new PointF(1, 1);
            if (oControl == null || Control.Size.Width <= 0 || Control.Size.Height <= 0)
            {
                return;
            }
            var oBmp = Image as Bitmap;
            if (oBmp is null)
            {
                return;
            }
            if (oBmp.Width <= 0 && oBmp.Height <= 0)
            {
                return;
            }
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
            oCoordinateManager.Zoom = new PointF((float)RoiZoomX, (float)RoiZoomY);
            RoiControl.X = RoiControl.Y = 0;
            RoiControl.Width = Control.Size.Width;
            RoiControl.Height = Control.Size.Height;
            RoiImgage.X = -(float)oCoordinateManager.Origin.X;
            RoiImgage.Y = -(float)oCoordinateManager.Origin.Y;
            RoiImgage.Width = fiw;
            RoiImgage.Height = fih;
        }

        public event PaintEventHandler Paint;

        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (Control != null && Image != null)
            {
                UpdateControlAndImageRoi();
                e.Graphics.DrawImage(Image, RoiControl, RoiImgage, GraphicsUnit.Pixel);
                switch (eRoiMode)
                {
                    case EnumRoiMode.Rectangle:
                        {
                            if (RoiRectanglePointList.Count == 2)
                            {
                                var P1 = oCoordinateManager.LocalToWorld(RoiRectanglePointList[0]);
                                var P2 = oCoordinateManager.LocalToWorld(RoiRectanglePointList[1]);
                                var R = new RectangleF(Math.Min(P1.X, P2.X), Math.Min(P1.Y, P2.Y), Math.Abs(P2.X - P1.X), Math.Abs(P2.Y - P1.Y));
                                using (var PenRoi = new Pen(System.Drawing.Color.Red, 2))
                                {
                                    e.Graphics.DrawRectangle(PenRoi, R.X, R.Y, R.Width, R.Height);
                                }
                            }
                            else if (RoiRectanglePointList.Count == 1)
                            {
                                var P1 = oCoordinateManager.LocalToWorld(RoiRectanglePointList[0]);
                                using (var PenRoi = new Pen(System.Drawing.Color.Red, 2))
                                {
                                    e.Graphics.DrawEllipse(PenRoi, P1.X - 2, P1.Y - 2, 4, 4);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                if (Paint != null)
                {
                    Paint(sender, e);
                }
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

        protected CoordinateManager oCoordinateManager = new CoordinateManager();

        public void WorldOriginMove(double deltaX, double deltaY)
        {
            if (!ImageBlockScroll)
            {
                UpdateControlAndImageRoi();
                oCoordinateManager.TranslateOriginPointRespectWorldCoordinates(deltaX, deltaY);
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
            oCoordinateManager.Origin = new PointF(0, 0);
            ForceRefresh();
        }

        public PointF PointControlToPointBitmap(Point p)
        {
            if (oControl != null && Image != null)
            {
                return oCoordinateManager.WorldToLocal(p);
            }
            return new PointF(-1, -1);
        }

        #endregion

        #region MouseTracker

        [Browsable(true), Category("Appearance")]
        public MouseButtons MouseMovingButton { get; set; } = MouseButtons.Left;

        [Browsable(true), Category("Appearance")]
        public MouseButtons MouseRoiButton { get; set; } = MouseButtons.Right;

        protected Boolean MouseMovePushed = false;
        protected Boolean MouseRoiPushed = false;
        protected Point MousePushedPoint = new Point();
        protected Point MousePushedPointLast = new Point();

        protected void OnMouseWheel(object Sender, MouseEventArgs args)
        {
            if (args.Delta != 0)
            {
                if (ImageZoomMode == EnumZoom.Manual)
                {
                    ImageZoomManual += 0.5f * args.Delta;
                }
                else
                {
                    if (args.Delta < 0 && (int)ImageZoomMode > (int)EnumZoom.ZoomQ)
                    {
                        ImageZoomMode--;
                    }
                    else if (args.Delta > 0 && (int)ImageZoomMode < (int)EnumZoom.Zoom4)
                    {
                        ImageZoomMode++;
                    }
                }
                ForceRefresh();
            }
        }

       


        protected void OnMouseDown(object Sender, MouseEventArgs args)
        {
            if (args.Button == MouseMovingButton)
            {
                if (!MouseMovePushed)
                {
                    MouseMovePushed = true;
                    MousePushedPoint = args.Location;
                    MousePushedPointLast = args.Location;
                }
            }
            if (eRoiMode != EnumRoiMode.Disabled)
            {
                if (args.Button == MouseRoiButton)
                {
                    if (!MouseRoiPushed)
                    {
                        MouseRoiPushed = true;
                        RoiPolygonPointList.Clear();
                        RoiRectanglePointList.Clear();
                        if (MouseRoiMode == EnumRoiMode.Rectangle)
                        {                           
                            HandleRectangleRoi(args);
                        }
                        else if (MouseRoiMode == EnumRoiMode.Polygon)
                        {
                            HandlePolygonRoi(args);
                        }
                        ForceRefresh();
                    }
                }
            }
        }

        private void HandleRectangleRoi(MouseEventArgs args)
        {
            if (RoiRectanglePointList.Count == 0)
            {
                var P1 = PointControlToPointBitmap(args.Location);
                RoiRectanglePointList.Add(P1);
            }
            else
            {
                var P1 = RoiRectanglePointList[0];
                var P2 = PointControlToPointBitmap(args.Location);
                var P3 = new PointF(Math.Min(P1.X, P2.X), Math.Min(P1.Y, P2.Y));
                var P4 = new PointF(Math.Max(P1.X, P2.X), Math.Max(P1.Y, P2.Y));
                RoiRectanglePointList.Clear();
                RoiRectanglePointList.Add(P3);
                RoiRectanglePointList.Add(P4);
            }
            ForceRefresh();
        }

        private void HandlePolygonRoi(MouseEventArgs args)
        {
            RoiRectanglePointList.Clear();
            var P1 = PointControlToPointBitmap(args.Location);
            RoiRectanglePointList.Add(P1);
            ForceRefresh();
        }

        protected void OnMouseUp(object Sender, MouseEventArgs args)
        {
            if (MouseMovePushed)
            {
                MouseMovePushed = false;
            }
            if (MouseRoiPushed)
            {
                MouseRoiPushed = false;
            }
        }

        public event MouseEventHandler MouseMove;

        public event EventHandler ColorAdded;
        public event EventHandler ColorRemoved;



        protected void OnMouseMove(object Sender, MouseEventArgs args)
        {
            HandleMouseColorPick(new Point(args.X, args.Y));
            if (MouseMovePushed)
            {
                int DX = MousePushedPointLast.X - args.Location.X;
                int DY = MousePushedPointLast.Y - args.Location.Y;
                WorldOriginMove(DX, DY);
                MousePushedPointLast = args.Location;
            }
            if (MouseMove != null)
            {
                MouseMove(Sender, args);
            }
            if (MouseRoiPushed)
            {
                switch (eRoiMode)
                {
                    case EnumRoiMode.Rectangle:
                            HandleRectangleRoi(args);
                        break;
                    case EnumRoiMode.Polygon:
                            HandlePolygonRoi(args);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        private static Image MakeColorSwatch(System.Drawing.Color color, int size = 12, int border = 1)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(System.Drawing.Color.FromArgb(120, System.Drawing.Color.Black), border))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.Clear(System.Drawing.Color.Transparent);
                g.FillRectangle(brush, border, border, size - 2 * border, size - 2 * border);
                g.DrawRectangle(pen, border, border, size - 2 * border - 1, size - 2 * border - 1);
            }
            return bmp;
        }

        private void oContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            //oContextMenuColor = HandleMouseColorPick(MouseCoordinates);
            toolStripMenuItemAddColor.Image?.Dispose();
            toolStripMenuItemAddColor.Image = MakeColorSwatch(oContextMenuColor, 16, 2);
        }
    }
}
