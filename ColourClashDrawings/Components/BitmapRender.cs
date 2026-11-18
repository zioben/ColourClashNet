
using ColourClashNet.Drawing;
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

namespace ColourClashNet.Components
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
        [Description("Raised when a Color is selected by operation")]
        public event EventHandler<ColorEventArgs> ColorSelected;

        [Browsable(true), Category("Color")]
        [Description("Raised when a Color is added on ColorList")]
        public event EventHandler<ColorEventArgs> ColorAdded;

        [Browsable(true), Category("Color")]
        [Description("Raised when a Color is removed from ColorList")]
        public event EventHandler<ColorEventArgs> ColorRemoved;

        [Browsable(true), Category("Color")]
        [Description("Raised when a Color list is cleared()")]
        public event EventHandler ColorReset;

        #endregion

        #region Fields/Properties

        public object locker = new object();

        [Browsable(true), Category("Appearance")]
        [DefaultValue(false)]
        public Boolean ImageBlockScroll { get; set; } = false;

        [Browsable(true), Category("Appearance")]
        [DefaultValue(EnumZoom.Stretch)]
        public EnumZoom ImageZoomMode { get; set; } = EnumZoom.Stretch;

        [Browsable(true), Category("Appearance")]
        [DefaultValue(true)]
        public Boolean ImageMoveOverControlBorder { get; set; } = true;


        float roiZoomM = 1.0f;

        [Browsable(true), Category("Appearance")]
        [DefaultValue(1.0f)]
        public float ImageZoomManual
        {
            get { return roiZoomM; }
            set { if (value > 0 && value != roiZoomM) roiZoomM = value; ForceRefresh(); }
        }

        [Browsable(true), Category("Behavior")]
        [Description("Define mouse button for dragging")]
        [DefaultValue(MouseButtons.Left)]
        public MouseButtons MouseMovingButton { get; set; } = MouseButtons.Left;    

        [Browsable(true), Category("Behavior")]
        [Description("Define mouse button for selection")]
        [DefaultValue(MouseButtons.Right)]
        public MouseButtons MouseSelectButton { get; set; } = MouseButtons.Right;

        [Browsable(true), Category("Behavior")]
        [Description("Continuous Image Dragging Mode.")]
        [DefaultValue(true)]
        public bool MouseImageFollowing { get; set; } = true;

        MouseHandler.PointTrack MouseControlTrack => MouseImageFollowing ? mouseManager.PointTrackHistory : mouseManager.PointTrackCurrent;

        [Browsable(true), Category("Appearance")]
        public Point MouseControlCoordinates => MouseControlTrack.ControlPointCurrent;


        [Browsable(true), Category("Appearance")]
        public Point MouseControlCoordinatesClip => MouseControlCoordinates.Clip( Control?.Width ?? 0, Control?.Height ?? 0);

        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinates => coordinateManager.InverseTransform(MouseControlCoordinates);

        
        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinatesClip => MouseImageCoordinates.Clip(Image?.Width ?? 0, Image?.Height ?? 0);

        [Browsable(true), Category("Appearance")]
        public System.Drawing.Color MouseImageColor { get; private set; }

            


        [Browsable(true), Category("Behavior")]
        public EnumOperation ComponentOperationMode { get; private set; }

        Control control;
        [Browsable(true), Category("Appearance")]
        [DefaultValue(null)]
        public Control Control
        {
            get { return control; }
            set
            {
                if (control != value)
                {
                    UnregisterControlEvents(); 
                    control = value;
                    RegisterControlEvents();
                }
            }
        }

        Image image;
        [Browsable(true), Category("Appearance")]
        [DefaultValue(null)]
        public Image Image
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                    mouseManager.Reset();
                    control?.Refresh();
                }
            }
        }


        MouseHandler mouseManager= new MouseHandler();
        CoordinateManager coordinateManager = new CoordinateManager();

        List<System.Drawing.Color> selectedColors = new List<System.Drawing.Color>();  
        [DefaultValue(null)]
        public List<System.Drawing.Color> SelectedColors 
        {
            get { return selectedColors; }
            protected set
            { 
                if( value != selectedColors)
                {
                    selectedColors = value;
                    RebuildMouseColorToolStripItems();
                }
            }
        }

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
            control?.Refresh();
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


        public void SetOperationMode(EnumOperation operationMode)
        {
            lock (locker)
            {
                ComponentOperationMode = operationMode;
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
            if (control != null)
            {
                control.Paint += OnPaint;
                control.MouseUp += OnMouseUp;
                control.MouseDown += OnMouseDown;
                control.MouseMove += OnMouseMove;
                control.MouseWheel += OnMouseWheel;
                control.ContextMenuStrip = contextMenuStrip;
            }
        }

        void UnregisterControlEvents()
        {
            if (control != null)
            {
                control.ContextMenuStrip = null;
                control.Paint -= OnPaint;
                control.MouseUp -= OnMouseUp;
                control.MouseDown -= OnMouseDown;
                control.MouseMove -= OnMouseMove;
                control.MouseWheel -= OnMouseWheel;
            }
        }

      

        #region Event codes - core of the component 


        protected void OnMouseWheel(object sender, MouseEventArgs args)
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
            if (mouseManager.MouseState == MouseHandler.EnumMouseState.tracking)
            {
                return;
            }
            if (Image is Bitmap bmp)
            {
                var pointI = MouseImageCoordinates;
                if (pointI.IsClipped(new Rectangle(0, 0, bmp.Width, bmp.Height)))
                {
                    MouseImageColor = System.Drawing.Color.Transparent;
                }
                else
                {
                    MouseImageColor = bmp.GetPixel((int)pointI.X, (int)pointI.Y);
                }
            }
            else
            {
                MouseImageColor = System.Drawing.Color.Transparent;
            }   
        }

        public PointF imageOrigin = new PointF(0, 0);

        bool firstClick = true;

        protected void OnMouseDown(object sender, MouseEventArgs args)
        {
            if (mouseManager.OnMouseDown(args))
            {
                if (firstClick)
                {
                    firstClick = false;
                    imageOrigin = imageOrigin.Sub(MouseControlTrack.ControlPointTranslation);
                }
                else
                {
                    imageOrigin = imageOrigin.Sub(MouseControlTrack.ControlPointClick);
                }
                coordinateManager.TransfOrigin = imageOrigin.Add(MouseControlTrack.ControlPointTranslation);
                UpdateMouseImageColor();
                ForceRefresh();
            }
            MouseDown?.Invoke(sender, args);
        }

        protected void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (mouseManager.OnMouseMove(args))
            {
                if (mouseManager.MouseState == MouseHandler.EnumMouseState.tracking)
                {
                    coordinateManager.TransfOrigin = imageOrigin.Add(MouseControlTrack.ControlPointTranslation);
                }
                UpdateMouseImageColor();
                ForceRefresh();
            }
            MouseMove?.Invoke(sender, args);
        }

        protected void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (mouseManager.OnMouseUp(args))
            {
                UpdateMouseImageColor();
                ForceRefresh();
                imageOrigin = new PointF(0, 0);
            }
            MouseUp?.Invoke(sender, args);
        }

       

        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (Control != null && Image != null)
            {
                UpdateControlAndImageRoi();
                //coordinateManager.WorldRotationAngle += 1;
                coordinateManager.DrawImage(e.Graphics, Image);
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

        [Browsable(true), Category("Appearance")]
        public float ZoomImageX
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.X != 0 ? 1.0f / coordinateManager.TransfZoom.X : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomImageY
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.Y != 0 ? 1.0f / coordinateManager.TransfZoom.Y : 0); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomControlX
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.X); }
        }

        [Browsable(true), Category("Appearance")]
        public float ZoomControY
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.Y); }
        }


      
        void UpdateControlAndImageRoi()
        {

            float roiZoomX = 0;
            float roiZoomY = 0;
            if (control == null || Control.Size.Width <= 0 || Control.Size.Height <= 0)
            {
                return;
            }
            var bmp = Image as Bitmap;
            if (bmp is null)
            {
                return;
            }
            if (bmp.Width <= 0 && bmp.Height <= 0)
            {
                return;
            }
            // Il + facile
            // Determina la roi da disegnare
            coordinateManager.SetTransfZoom(1);
            float fcw = Math.Max(1,Control.Size.Width);
            float fch = Math.Max(1, Control.Size.Height);
            float fiw = Math.Max(1, Image.Size.Width);
            float fih = Math.Max(1, Image.Size.Height);
            //// Stretch Zoom
            //roiZoomX = (float)Image.Size.Width / fiw;
            //roiZoomY = (float)Image.Size.Height / fih;
            switch (ImageZoomMode)
            {
                // Zoom: restringe o allarga la roi
                case EnumZoom.ZoomQ: roiZoomX = roiZoomY = 1.0f / 4; break;
                case EnumZoom.ZoomH: roiZoomX = roiZoomY = 1.0f / 2; break;
                case EnumZoom.Zoom1: roiZoomX = roiZoomY = 1.0f * 1; break;
                case EnumZoom.Zoom2: roiZoomX = roiZoomY = 1.0f * 2; break;
                case EnumZoom.Zoom3: roiZoomX = roiZoomY = 1.0f * 3; break;
                case EnumZoom.Zoom4: roiZoomX = roiZoomY = 1.0f * 4; break;
                // Stretch: 
                case EnumZoom.Stretch:
                    {
                        roiZoomX = fcw / fiw;
                        roiZoomY = fch / fih;
                    }
                    break;
                // Fit w e h
                // w : 100 = h : x
                case EnumZoom.FitW:
                    roiZoomX = fcw / fiw;
                    roiZoomY = roiZoomX;
                    break;
                case EnumZoom.FitH:
                    roiZoomY = fch / fih;
                    roiZoomX = roiZoomY;
                    break;
                // Fit è il + cancaro
                case EnumZoom.Fit:
                    {
                        roiZoomX = fcw / fiw;
                        roiZoomY = fch / fih;
                        if (roiZoomX < roiZoomY)
                        {
                            roiZoomY = roiZoomX;
                        }
                        else
                        {
                            roiZoomX = roiZoomY;
                        }
                    }
                    break;
                case EnumZoom.Manual: roiZoomX = roiZoomY = roiZoomM; break;
            }
            coordinateManager.SetTrasfZoom(roiZoomX, roiZoomY);
        }



       
        #endregion

        #region Moving Origin Point

        protected void ForceRefresh()
        {
            control?.Invalidate();
        }

   
        public void WorldOriginMove(double deltaX, double deltaY)
        {
            if (!ImageBlockScroll)
            {
                UpdateControlAndImageRoi();
                //--------------------------------------------------------------------------
                coordinateManager.TranslateTrasformationOrigin(deltaX, deltaY);
                ForceRefresh();
            }
        }

        public void WorldOriginMove(Point delta)
        {
            WorldOriginMove(delta.X, delta.Y);
        }

        public void WorldOriginMove(PointF delta)
        {
            WorldOriginMove(delta.X, delta.Y);
        }

        public void OriginZero()
        {
            //--------------------------------------------------------------------------
            //coordinateManager.LocalOrigin = new PointF(0, 0);
            ForceRefresh();
        }

      
        #endregion

        #region MouseTracker

      

        protected Boolean mouseMovePushed = false;
        protected Boolean mouseRoiPushed = false;
        protected Point mousePushedPoint = new Point();
        protected Point mousePushedPointLast = new Point();

       

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
