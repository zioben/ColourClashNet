using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ColourClashNet.Controls.BitmapRenderOld;

namespace ColourClashNet.Controls
{


    public partial class BitmapRender : Component
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
        /// Operative mode
        /// </summary>
        public enum EnumOperation
        {
            None,
            TrackMouse,
            SetRoiRect,
            SetRoiPolygon,
            EditRoi,
            GetColor,
        }

        #endregion

        public class ColorEventArgs : EventArgs
        {
            public System.Drawing.Color Color { get; internal set; } = System.Drawing.Color.Transparent;
            public Point ComponentCoordinates { get; internal set; } = new Point();
            public Point ImageCoordinates { get; internal set; } = new Point();
        }

      
        #region Events

        [Browsable(true), Category("Appearance")]
        [Description("Raised when Image Control needs repaint")]
        public event PaintEventHandler Paint;

        [Browsable(true), Category("Mouse")]
        [Description("Raised when a mouse button is released in Image Control")]
        public event MouseEventHandler MouseUp;

        [Browsable(true), Category("Mouse")]
        [Description("Raised when a mouse button is released in Image Control")]
        public event MouseEventHandler MouseDown;

        [Browsable(true), Category("Mouse")]
        [Description("Raised when mouse is moved in Image Control")]
        public event MouseEventHandler MouseMove;

        [Browsable(true), Category("Color")]
        [Description("Raised when a color is selected by operation")]
        public event EventHandler<ColorEventArgs> ColorSelected;

        [Browsable(true), Category("Color")]
        [Description("Raised when a color is added on ColorList")]
        public event EventHandler<ColorEventArgs> ColorAdded;

        [Browsable(true), Category("Color")]
        [Description("Raised when a color is removed from ColorList")]
        public event EventHandler<ColorEventArgs> ColorRemoved;

        [Browsable(true), Category("Color")]
        [Description("Raised when a color list is cleared()")]
        public event EventHandler ColorReset;

        #endregion

        #region Fields/Properties

        public object locker = new object();

        [Browsable(true), Category("Appearance")]
        public Boolean ImageBlockScroll { get; set; }

        [Browsable(true), Category("Appearance")]
        public EnumZoom ImageZoomMode { get; set; }

        [Browsable(true), Category("Appearance")]
        public Boolean ImageMoveOverControlBorder { get; set; }


        float fRoiZoomM = 1;

        [Browsable(true), Category("Appearance")]
        public float ImageZoomManual
        {
            get { return fRoiZoomM; }
            set { if (value > 0) fRoiZoomM = value; ForceRefresh(); }
        }

        [Browsable(true), Category("Behavior")]
        public MouseButtons MouseMovingButton { get; set; } = MouseButtons.Left;

        [Browsable(true), Category("Behavior")]
        public MouseButtons MouseSelectButton { get; set; } = MouseButtons.Right;

        public bool ContinuousFollowing { get; set; } = true;

        [Browsable(false), Category("Appearance")]
        MouseHandlerCoordinateManager.PointTrack MouseControlTrack => ContinuousFollowing ? oMouseManager.PointTrackHistory : oMouseManager.PointTrackCurrent;

        [Browsable(true), Category("Appearance")]
        public Point MouseControlCoordinatesX => MouseControlTrack.ControlPointCurrent;

        [Browsable(false), Category("Appearance")]
        MouseHandlerCoordinateManager.ImageTrack MouseImageTrack => ContinuousFollowing ? oMouseManager.ImageTrackHistoryX : oMouseManager.ImageTrackCurrentX;

        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinatesX => MouseImageTrack.ImagePointCurrent;

        [Browsable(true), Category("Appearance")]
        public PointF MouseControlCoordinatesClip => new PointF(
            Math.Min(Control?.Width ?? 0, Math.Max(0, MouseControlCoordinatesX.X)),
            Math.Min(Control?.Height ?? 0, Math.Max(0, MouseControlCoordinatesX.Y))
         );

        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinatesClip => new PointF(
            Math.Min( Image?.Width ?? 0, Math.Max(0, MouseImageCoordinatesX.X)),
            Math.Min( Image?.Height ?? 0, Math.Max(0, MouseImageCoordinatesX.Y))
         );

        [Browsable(true), Category("Appearance")]
        public System.Drawing.Color MouseImageColor { get; private set; }

            


        [Browsable(true), Category("Behavior")]
        public EnumOperation ComponentOperationMode { get; private set; }

        Control oControl;
        [Browsable(true), Category("Appearance")]
        public Control Control
        {
            get { return oControl; }
            set
            {
                if (oControl != value)
                {
                    UnregisterControlEvents(); 
                    oControl = value;
                    RegisterControlEvents();
                }
            }
        }

        Image oImage;
        [Browsable(true), Category("Appearance")]
        public Image Image
        {
            get { return oImage; }
            set
            {
                if (oImage != value)
                {
                    oImage = value;
                    oMouseManager.Reset();
                    oControl?.Refresh();
                }
            }
        }


        MouseHandlerCoordinateManager oMouseManager= new MouseHandlerCoordinateManager();
        CoordinateManager oCoordinateManager => oMouseManager.CoordinateManager;

        public List<System.Drawing.Color> SelectedColors { get; protected set; } = new List<System.Drawing.Color>();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BitmapRender()
        {
            InitializeComponent();
            Create();
        }

        /// <summary>
        /// Constructor with container
        /// </summary>
        /// <param name="container"></param>
        public BitmapRender(IContainer container)
        {
            container.Add(this);
            Create();
        }

        /// <summary>
        /// Common create code
        /// </summary>
        protected void Create()
        {
            MouseMovingButton = MouseButtons.Left;
            MouseSelectButton = MouseButtons.Right;
            InitializeComponent();
            InitializeGUI();
            RegisterToolStripItems();
            SetOperationMode(EnumOperation.TrackMouse);
        }
      
        void InitializeGUI()
        {
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        void SetClickZoom(EnumZoom z)
        {
            ImageZoomMode = z;
            oControl?.Refresh();
        }
        void SetClickScroll()
        {
            ImageBlockScroll = !ImageBlockScroll; 
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        void RegisterToolStripItems()
        {

            this.toolStripMenuItem1_4.Click += (s, e) =>  { SetClickZoom(EnumZoom.ZoomQ); };
            this.toolStripMenuItem1_2.Click += (s, e) =>  { SetClickZoom(EnumZoom.ZoomH); };
            this.toolStripMenuItem1_1.Click += (s, e) =>  { SetClickZoom(EnumZoom.Zoom1); };
            this.toolStripMenuItem2_1.Click += (s, e) =>  { SetClickZoom(EnumZoom.Zoom2); };
            this.toolStripMenuItem3_1.Click += (s, e) =>  { SetClickZoom(EnumZoom.Zoom3); };
            this.toolStripMenuItem4_1.Click += (s, e) =>  { SetClickZoom(EnumZoom.Zoom4); };
            this.toolStripMenuItemFit.Click += (s, e) =>  { SetClickZoom(EnumZoom.Fit); }; 
            this.toolStripMenuItemFitW.Click += (s, e) => { SetClickZoom(EnumZoom.FitW); }; 
            this.toolStripMenuItemFitH.Click += (s, e) => { SetClickZoom(EnumZoom.FitH); }; 
            this.toolStripMenuItemStretch.Click += (s, e) => { SetClickZoom(EnumZoom.Stretch); }; 
            this.toolStripMenuItemManual.Click += (s, e) =>  { SetClickZoom(EnumZoom.Manual); };
            this.toolStripMenuItemBlockMoving.Click += (s, e) => { SetClickScroll(); };
            this.toolStripMenuItemAddColor.Click += new EventHandler(OnAddColor);
            this.toolStripMenuItemResetColors.Click += new EventHandler(OnResetColors);
        }

        #endregion


        public void SetOperationMode(EnumOperation eOperationMode)
        {
            lock (locker)
            {
                ComponentOperationMode = eOperationMode;
                switch (ComponentOperationMode)
                {
                    case EnumOperation.None:
                        break;
                    case EnumOperation.TrackMouse:
                        break;
                    case EnumOperation.SetRoiRect:
                        break;
                    case EnumOperation.SetRoiPolygon:
                        break;
                    case EnumOperation.EditRoi:
                        break;
                    case EnumOperation.GetColor:
                    default:
                        break;
                }
            }
        }

        void RegisterControlEvents()
        {
            if (oControl != null)
            {
                oControl.Paint += OnPaint;
                oControl.MouseUp += OnMouseUp;
                oControl.MouseDown += OnMouseDown;
                oControl.MouseMove += OnMouseMove;
                oControl.MouseWheel += OnMouseWheel;
                oControl.ContextMenuStrip = oContextMenuStrip;
            }
        }

        void UnregisterControlEvents()
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
        }

        #region Event codes - core of the component 


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

        void UpdateMouseImageColor()
        {
            if (oMouseManager.MouseState == MouseHandlerBase.EnumMouseState.tracking)
            {
                return;
            }
            if (Image is Bitmap oBmp)
            {
                // Control Origin point
                var oPointC = new PointF(RoiControl.X,RoiControl.Y);
                // Current mouse coordinates
                var oPointM = new PointF(MouseControlCoordinatesX.X, MouseControlCoordinatesX.Y);
                // Transalte to control roi
                var oPointT = oPointM.Sub(oPointC);
                // convert to image coordinates
                var oPointI = oCoordinateManager.WorldToLocal(oPointT);
                if (oPointI.X < 0 || oPointI.Y < 0 || oPointI.X >= oBmp.Width || oPointI.Y >= oBmp.Height)
                {
                    MouseImageColor = System.Drawing.Color.Transparent;
                    return;
                }
                MouseImageColor = oBmp.GetPixel((int)oPointI.X, (int)oPointI.Y);
            }
            else
            {
                MouseImageColor = System.Drawing.Color.Transparent;
            }   
        }

        protected void OnMouseDown(object Sender, MouseEventArgs args)
        {
            if (oMouseManager.OnMouseDown(args))
            {
                UpdateMouseImageColor();
                ForceRefresh();
            }
            MouseDown?.Invoke(Sender, args);
        }

        protected void OnMouseUp(object Sender, MouseEventArgs args)
        {
            if (oMouseManager.OnMouseUp(args))
            {
                UpdateMouseImageColor();
                ForceRefresh();
            }
            MouseUp?.Invoke(Sender, args);
        }

        protected void OnMouseMove(object Sender, MouseEventArgs args)
        {
            if (oMouseManager.OnMouseMove(args))
            {
                UpdateMouseImageColor();
                ForceRefresh();
            }
            MouseMove?.Invoke(Sender, args);
        }

        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (Control != null && Image != null)
            {
                UpdateControlAndImageRoi();
                e.Graphics.DrawImage(Image, RoiControl, RoiImage, GraphicsUnit.Pixel);
            }
            Paint?.Invoke(sender, e);
        }

        #endregion

        #region Selected Colors Management

        public void AddMouseSelectedColor(System.Drawing.Color c)
        {
            lock (locker)
            {
                if (!SelectedColors.Contains(c))
                {
                    SelectedColors.Add(c);
                }
            }
        }

        public void RemoveMouseSelectedColor(System.Drawing.Color c)
        {
            lock (locker)
            {
                if (SelectedColors.Contains(c))
                {
                    SelectedColors.Remove(c);
                }
            }
        }

        public void ResetMouseSelectedColors()
        {
            lock (locker)
            {
                SelectedColors.Clear();
            }
        }

        void RebuildMouseColorToolStripItems()
        {
            toolStripMenuItemColors.DropDownItems.Clear();
            foreach (var c in SelectedColors)
            {
                var tsi = new ToolStripMenuItem();
                tsi.Text = $"RGB : 0x00_{c.R:X2}_{c.G:X2}_{c.B:X2}";
                tsi.BackColor = c;
                tsi.ForeColor = (c.R + c.G + c.B) / 3 < 128 ? System.Drawing.Color.White : System.Drawing.Color.Black;
                tsi.Click += (s, e) =>
                {
                    RemoveMouseSelectedColor(c);
                    RebuildMouseColorToolStripItems();
                    ColorRemoved?.Invoke(this, new ColorEventArgs() {  Color  = tsi.BackColor } );
                };
                toolStripMenuItemColors.DropDownItems.Add(tsi);
            }
            toolStripMenuItemColors.Enabled = SelectedColors.Count > 0;
        }

        void OnAddColor(object sender, EventArgs e)
        {
            if (MouseImageColor != System.Drawing.Color.Transparent)
            {
                AddMouseSelectedColor(MouseImageColor);
            }
            RebuildMouseColorToolStripItems();
            ColorAdded?.Invoke(this, new ColorEventArgs() { Color = MouseImageColor });
        }
        void OnResetColors(object sender, EventArgs e)
        {
            ResetMouseSelectedColors();
            RebuildMouseColorToolStripItems();
            ColorReset?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region RAD Setup

      
       

        #endregion

        #region Clip and Zoom Code

        SolidBrush BrushBlack = new SolidBrush(System.Drawing.Color.Black);
        RectangleF RoiControl = new RectangleF();
        RectangleF RoiImage = new RectangleF();


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
            oCoordinateManager.SetZoom(1);
            float fiw = Control.Size.Width;
            float fih = Control.Size.Height;
            // Stretch Zoom
            RoiZoomX = (float)Image.Size.Width / fiw;
            RoiZoomY = (float)Image.Size.Height / fih;
            switch (ImageZoomMode)
            {
                // Zoom: restringe o allarga la roi
                case EnumZoom.ZoomQ: RoiZoomX = RoiZoomY = 1.0f * 4; break;
                case EnumZoom.ZoomH: RoiZoomX = RoiZoomY = 1.0f * 2; break;
                case EnumZoom.Zoom1: RoiZoomX = RoiZoomY = 1.0f / 1; break;
                case EnumZoom.Zoom2: RoiZoomX = RoiZoomY = 1.0f / 2; break;
                case EnumZoom.Zoom3: RoiZoomX = RoiZoomY = 1.0f / 3; break;
                case EnumZoom.Zoom4: RoiZoomX = RoiZoomY = 1.0f / 4; break;
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
                case EnumZoom.Manual: RoiZoomX = RoiZoomY = fRoiZoomM; fiw *= fRoiZoomM; fih *= fRoiZoomM; break;
            }
            oCoordinateManager.Zoom = new PointF((float)RoiZoomX, (float)RoiZoomY);
            
            // Convert Image ROI in Co
            // In soldoni questa è la roi sul controllo da disegnare
            RoiControl = oMouseManager.ToControlRectangle(0, 0, oBmp.Width, oBmp.Height);
            RoiControl.X += MouseControlTrack.ControlPointTranslation.X;
            RoiControl.Y += MouseControlTrack.ControlPointTranslation.Y;

            // Converto le coordinate in pixel in quelle dell'immagine
            // In soldoni è porzione di immagine da disegnare 
            // Per ora teniamo tutta l'immagine
            RoiImage = new RectangleF(0, 0, oBmp.Width, oBmp.Height);
            //
            // Ragionamento semplice, la RoiImage viene Riscalata nella RoiControl

        }



       
        #endregion

        #region Moving Origin Point

        protected void ForceRefresh()
        {
            oControl?.Invalidate();
        }

        //protected CoordinateManager oCoordinateManager = new CoordinateManager();

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

      

        protected Boolean MouseMovePushed = false;
        protected Boolean MouseRoiPushed = false;
        protected Point MousePushedPoint = new Point();
        protected Point MousePushedPointLast = new Point();

       

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
            toolStripMenuItemAddColor.Image = MakeColorSwatch(MouseImageColor, 16, 2);
        }
    }
}
