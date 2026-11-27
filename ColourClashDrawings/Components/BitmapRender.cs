
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


    /// <summary>
    /// Designer component that draws an image on any <see cref="System.Windows.Forms.Control"/> with zoom and mouse panning.
    /// <para/>
    /// Left button: scrolls the image, a double click resets the position; while a drawing tool is active
    /// (see <see cref="ShapeTool"/>) it draws the region of interest instead.
    /// Middle button: always scrolls. Wheel: zoom.
    /// <para/>
    /// Right click without dragging: context menu (zoom, reset position, shape tools, grid, color picking).
    /// An optional pixel grid can be shown and used to align the shapes (see <see cref="GridVisible"/>, <see cref="GridSnap"/>).
    /// </summary>
    public partial class BitmapRender : Component
    {
        #region Enums

        /// <summary>
        /// Zoom modes.
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
        /// Mouse operation modes.
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

        /// <summary>
        /// Event data for the color selection events.
        /// </summary>
        public class ColorEventArgs : EventArgs
        {
            /// <summary>
            /// Selected color.
            /// </summary>
            public System.Drawing.Color Color { get; internal set; } = System.Drawing.Color.Transparent;
            /// <summary>
            /// Mouse position in control coordinates.
            /// </summary>
            public Point ComponentCoordinates { get; internal set; } = new Point();
            /// <summary>
            /// Mouse position in image coordinates.
            /// </summary>
            public Point ImageCoordinates { get; internal set; } = new Point();
        }

      
        #region Events

        /// <summary>
        /// Raised after the image has been painted on the control.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [Description("Raised when Image Control needs repaint")]
        public event PaintEventHandler Paint;

        /// <summary>
        /// Raised when a mouse button is released on the control.
        /// </summary>
        [Browsable(true), Category("Mouse")]
        [Description("Raised when a mouse button is released in Image Control")]
        public event MouseEventHandler MouseUp;

        /// <summary>
        /// Raised when a mouse button is pressed on the control.
        /// </summary>
        [Browsable(true), Category("Mouse")]
        [Description("Raised when a mouse button is released in Image Control")]
        public event MouseEventHandler MouseDown;

        /// <summary>
        /// Raised when the mouse moves on the control.
        /// </summary>
        [Browsable(true), Category("Mouse")]
        [Description("Raised when mouse is moved in Image Control")]
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// Raised when a color is picked.
        /// </summary>
        [Browsable(true), Category("Color")]
        [Description("Raised when a Color is selected by operation")]
        public event EventHandler<ColorEventArgs> ColorSelected;

        /// <summary>
        /// Raised when a color is added to <see cref="SelectedColors"/>.
        /// </summary>
        [Browsable(true), Category("Color")]
        [Description("Raised when a Color is added on ColorList")]
        public event EventHandler<ColorEventArgs> ColorAdded;

        /// <summary>
        /// Raised when a color is removed from <see cref="SelectedColors"/>.
        /// </summary>
        [Browsable(true), Category("Color")]
        [Description("Raised when a Color is removed from ColorList")]
        public event EventHandler<ColorEventArgs> ColorRemoved;

        /// <summary>
        /// Raised when <see cref="SelectedColors"/> is cleared.
        /// </summary>
        [Browsable(true), Category("Color")]
        [Description("Raised when a Color list is cleared()")]
        public event EventHandler ColorReset;

        #endregion

        #region Fields/Properties

        /// <summary>
        /// Synchronization object.
        /// </summary>
        public object locker = new object();

        /// <summary>
        /// Disables the programmatic scrolling (<see cref="WorldOriginMove(double, double)"/>).
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(false)]
        public Boolean ImageBlockScroll { get; set; } = false;

        /// <summary>
        /// Current zoom mode.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(EnumZoom.Stretch)]
        public EnumZoom ImageZoomMode { get; set; } = EnumZoom.Stretch;

        /// <summary>
        /// Allows the image to be dragged beyond the control borders.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(true)]
        public Boolean ImageMoveOverControlBorder { get; set; } = true;


        float manualZoom = 1.0f;

        /// <summary>
        /// Zoom factor used in <see cref="EnumZoom.Manual"/> mode.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        [DefaultValue(1.0f)]
        public float ImageZoomManual
        {
            get { return manualZoom; }
            set { if (value > 0 && value != manualZoom) manualZoom = value; ForceRefresh(); }
        }

        /// <summary>
        /// Mouse button used to drag the image when no drawing tool is active (the middle button always drags it).
        /// </summary>
        [Browsable(true), Category("Behavior")]
        [Description("Define mouse button for dragging")]
        [DefaultValue(MouseButtons.Left)]
        public MouseButtons MouseMovingButton { get; set; } = MouseButtons.Left;    

        /// <summary>
        /// Mouse button used to draw and edit the region of interest.
        /// </summary>
        [Browsable(true), Category("Behavior")]
        [Description("Define mouse button for drawing the region of interest")]
        [DefaultValue(MouseButtons.Left)]
        public MouseButtons MouseDrawButton { get; set; } = MouseButtons.Left;

        /// <summary>
        /// Mouse button that opens the context menu.
        /// </summary>
        [Browsable(true), Category("Behavior")]
        [Description("Define mouse button for the context menu")]
        [DefaultValue(MouseButtons.Right)]
        public MouseButtons MouseSelectButton { get; set; } = MouseButtons.Right;

        /// <summary>
        /// When true the drag translation accumulates over successive drags.
        /// </summary>
        [Browsable(true), Category("Behavior")]
        [Description("Continuous Image Dragging Mode.")]
        [DefaultValue(true)]
        public bool MouseImageFollowing { get; set; } = true;

        /// <summary>
        /// Mouse track used for panning.
        /// </summary>
        MouseHandler.PointTrack MouseControlTrack => MouseImageFollowing ? mouseManager.PointTrackHistory : mouseManager.PointTrackCurrent;

        /// <summary>
        /// Mouse position in control coordinates.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public Point MouseControlCoordinates => MouseControlTrack.ControlPointCurrent;


        /// <summary>
        /// Mouse position in control coordinates, (0,0) when outside the control.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public Point MouseControlCoordinatesClip => MouseControlCoordinates.Clip( Control?.Width ?? 0, Control?.Height ?? 0);

        /// <summary>
        /// Mouse position in image coordinates.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinates => coordinateManager.InverseTransform(MouseControlCoordinates);

        
        /// <summary>
        /// Mouse position in image coordinates, (0,0) when outside the image.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public PointF MouseImageCoordinatesClip => MouseImageCoordinates.Clip(Image?.Width ?? 0, Image?.Height ?? 0);

        /// <summary>
        /// Image color under the mouse; transparent when outside the image or while dragging.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public System.Drawing.Color MouseImageColor { get; private set; }

            


        /// <summary>
        /// Current operation mode.
        /// </summary>
        [Browsable(true), Category("Behavior")]
        public EnumOperation ComponentOperationMode { get; private set; }

        Control control;
        /// <summary>
        /// Control the image is drawn on; mouse and paint events are hooked automatically.
        /// </summary>
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
        /// <summary>
        /// Image to draw.
        /// </summary>
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
        /// <summary>
        /// Colors picked by the user.
        /// </summary>
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
        /// Common initialization of the constructors.
        /// </summary>
        protected void Create()
        {
            MouseMovingButton = MouseButtons.Left;
            MouseDrawButton = MouseButtons.Left;
            MouseSelectButton = MouseButtons.Right;
            InitializeComponent();
            InitializeGUI();
            RegisterToolStripItems();
            CreateShapeMenuItems();
            SetOperationMode(EnumOperation.TrackMouse);
        }
      
        /// <summary>
        /// Initializes the context menu state.
        /// </summary>
        void InitializeGUI()
        {
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        /// <summary>
        /// Sets the zoom mode from the context menu.
        /// </summary>
        void SetClickZoom(EnumZoom z)
        {
            ImageZoomMode = z;
            control?.Refresh();
        }
        /// <summary>
        /// Toggles <see cref="ImageBlockScroll"/> from the context menu.
        /// </summary>
        void SetClickScroll()
        {
            ImageBlockScroll = !ImageBlockScroll; 
            toolStripMenuItemBlockMoving.Checked = ImageBlockScroll;
        }

        /// <summary>
        /// Wires the context menu items.
        /// </summary>
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


        /// <summary>
        /// Sets the operation mode.
        /// </summary>
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

        /// <summary>
        /// Hooks paint and mouse events of <see cref="Control"/>.
        /// </summary>
        void RegisterControlEvents()
        {
            if (control != null)
            {
                control.Paint += OnPaint;
                control.MouseUp += OnMouseUp;
                control.MouseDown += OnMouseDown;
                control.MouseMove += OnMouseMove;
                control.MouseWheel += OnMouseWheel;
                control.MouseDoubleClick += OnMouseDoubleClick;
                // The context menu is opened by the right button handler, on a click without dragging:
                // assigning it to the control would also open it at the end of a right drag
            }
        }

        /// <summary>
        /// Unhooks paint and mouse events of <see cref="Control"/>.
        /// </summary>
        void UnregisterControlEvents()
        {
            if (control != null)
            {
                control.Paint -= OnPaint;
                control.MouseUp -= OnMouseUp;
                control.MouseDown -= OnMouseDown;
                control.MouseMove -= OnMouseMove;
                control.MouseWheel -= OnMouseWheel;
                control.MouseDoubleClick -= OnMouseDoubleClick;
            }
        }

      

        #region Event codes - core of the component 


        /// <summary>
        /// Changes the zoom step (or the manual zoom factor) with the mouse wheel.
        /// </summary>
        protected void OnMouseWheel(object sender, MouseEventArgs args)
        {
            if (args.Delta != 0)
            {
                if (ImageZoomMode == EnumZoom.Manual)
                {
                    // One wheel notch (Delta = 120) changes the zoom by 25%
                    ImageZoomManual *= args.Delta > 0 ? 1.25f : 0.8f;
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

        /// <summary>
        /// Updates <see cref="MouseImageColor"/> from the pixel under the mouse.
        /// </summary>
        void UpdateMouseImageColor()
        {
            if (mouseManager.MouseState == MouseHandler.EnumMouseState.tracking)
            {
                return;
            }
            if (Image is Bitmap bmp)
            {
                var imagePoint = MouseImageCoordinates;
                if (imagePoint.IsClipped(new Rectangle(0, 0, bmp.Width, bmp.Height)))
                {
                    MouseImageColor = System.Drawing.Color.Transparent;
                }
                else
                {
                    MouseImageColor = bmp.GetPixel((int)imagePoint.X, (int)imagePoint.Y);
                }
            }
            else
            {
                MouseImageColor = System.Drawing.Color.Transparent;
            }   
        }

        /// <summary>
        /// Image origin at the start of the current drag.
        /// </summary>
        public PointF imageOrigin = new PointF(0, 0);

        bool firstClick = true;

        // True when the last press was a double click used by the shape
        bool shapeDoubleClick;

        /// <summary>
        /// Starts a drag.
        /// </summary>
        protected void OnMouseDown(object sender, MouseEventArgs args)
        {
            // A press that draws or edits the shape, or opens the menu, must not scroll the image
            shapeDoubleClick = false;
            if (OnShapeMouseDown(args))
            {
                // A double click that the shape used (closing a polygon, a handle) must not reset the position
                shapeDoubleClick = args.Clicks >= 2;
                MouseDown?.Invoke(sender, args);
                return;
            }
            // The scroll button is chosen at the press: the moving button with no drawing tool, the middle one always
            bool drawing = ShapeTool != EnumShapeTool.None || polygonDraft.Count > 0;
            if (mouseManager.MouseState != MouseHandler.EnumMouseState.push && mouseManager.MouseState != MouseHandler.EnumMouseState.tracking
                && ((args.Button == MouseMovingButton && !drawing) || args.Button == MouseButtons.Middle))
            {
                mouseManager.MouseButton = args.Button;
            }
            if (mouseManager.OnMouseDown(args))
            {
                // Keep the image where it is: the new drag starts from the current translation
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

        /// <summary>
        /// Pans the image while dragging and updates the color under the mouse.
        /// </summary>
        protected void OnMouseMove(object sender, MouseEventArgs args)
        {
            OnShapeMouseMove(args);
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

        /// <summary>
        /// Ends a drag.
        /// </summary>
        protected void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (OnShapeMouseUp(args))
            {
                MouseUp?.Invoke(sender, args);
                return;
            }
            if (mouseManager.OnMouseUp(args))
            {
                UpdateMouseImageColor();
                ForceRefresh();
                imageOrigin = new PointF(0, 0);
            }
            MouseUp?.Invoke(sender, args);
        }

       

        /// <summary>
        /// Draws the image with the current zoom and translation.
        /// </summary>
        public void OnPaint(object sender, PaintEventArgs e)
        {
            if (Control != null && Image != null)
            {
                UpdateControlAndImageRoi();               
                coordinateManager.DrawImage(e.Graphics, Image);
                DrawGrid(e.Graphics);
                // The region of interest is drawn over the image, in the same coordinates
                DrawShapes(e.Graphics);
            }
            Paint?.Invoke(sender, e);
        }

        #endregion

        #region Selected Colors Management

        /// <summary>
        /// Adds a color to <see cref="SelectedColors"/> if not present.
        /// </summary>
        public void AddMouseSelectedColor(System.Drawing.Color color)
        {
            lock (locker)
            {
                if (!SelectedColors.Contains(color))
                {
                    SelectedColors.Add(color);
                }
            }
        }

        /// <summary>
        /// Removes a color from <see cref="SelectedColors"/>.
        /// </summary>
        public void RemoveMouseSelectedColor(System.Drawing.Color color)
        {
            lock (locker)
            {
                if (SelectedColors.Contains(color))
                {
                    SelectedColors.Remove(color);
                }
            }
        }

        /// <summary>
        /// Clears <see cref="SelectedColors"/>.
        /// </summary>
        public void ResetMouseSelectedColors()
        {
            lock (locker)
            {
                SelectedColors.Clear();
            }
        }

        /// <summary>
        /// Rebuilds the context menu list of selected colors; clicking an item removes the color.
        /// </summary>
        void RebuildMouseColorToolStripItems()
        {
            toolStripMenuItemColors.DropDownItems.Clear();
            foreach (var color in SelectedColors)
            {
                var menuItem = new ToolStripMenuItem();
                menuItem.Text = $"RGB : 0x00_{color.R:X2}_{color.G:X2}_{color.B:X2}";
                menuItem.BackColor = color;
                menuItem.ForeColor = (color.R + color.G + color.B) / 3 < 128 ? System.Drawing.Color.White : System.Drawing.Color.Black;
                menuItem.Click += (s, e) =>
                {
                    RemoveMouseSelectedColor(color);
                    RebuildMouseColorToolStripItems();
                    ColorRemoved?.Invoke(this, new ColorEventArgs() {  Color  = menuItem.BackColor } );
                };
                toolStripMenuItemColors.DropDownItems.Add(menuItem);
            }
            toolStripMenuItemColors.Enabled = SelectedColors.Count > 0;
        }

        /// <summary>
        /// Adds the color under the mouse to the selection.
        /// </summary>
        void OnAddColor(object sender, EventArgs e)
        {
            if (MouseImageColor != System.Drawing.Color.Transparent)
            {
                AddMouseSelectedColor(MouseImageColor);
            }
            RebuildMouseColorToolStripItems();
            ColorAdded?.Invoke(this, new ColorEventArgs() { Color = MouseImageColor });
        }
        /// <summary>
        /// Clears the color selection.
        /// </summary>
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

        /// <summary>
        /// Horizontal image to control scale (inverse of the zoom).
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public float ZoomImageX
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.X != 0 ? 1.0f / coordinateManager.TransfZoom.X : 0); }
        }

        /// <summary>
        /// Vertical image to control scale (inverse of the zoom).
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public float ZoomImageY
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.Y != 0 ? 1.0f / coordinateManager.TransfZoom.Y : 0); }
        }

        /// <summary>
        /// Horizontal zoom factor.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public float ZoomControlX
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.X); }
        }

        /// <summary>
        /// Vertical zoom factor.
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public float ZoomControY
        {
            get { UpdateControlAndImageRoi(); return (float)(coordinateManager.TransfZoom.Y); }
        }


      
        /// <summary>
        /// Computes the zoom factors from <see cref="ImageZoomMode"/> and the control and image sizes.
        /// </summary>
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
            // Compute the zoom factors of the area to draw, according to the zoom mode
            coordinateManager.SetTransfZoom(1);
            float controlWidth = Math.Max(1,Control.Size.Width);
            float controlHeight = Math.Max(1, Control.Size.Height);
            float imageWidth = Math.Max(1, Image.Size.Width);
            float imageHeight = Math.Max(1, Image.Size.Height);
            switch (ImageZoomMode)
            {
                // Fixed zoom: shrinks or enlarges the image
                case EnumZoom.ZoomQ: roiZoomX = roiZoomY = 1.0f / 4; break;
                case EnumZoom.ZoomH: roiZoomX = roiZoomY = 1.0f / 2; break;
                case EnumZoom.Zoom1: roiZoomX = roiZoomY = 1.0f * 1; break;
                case EnumZoom.Zoom2: roiZoomX = roiZoomY = 1.0f * 2; break;
                case EnumZoom.Zoom3: roiZoomX = roiZoomY = 1.0f * 3; break;
                case EnumZoom.Zoom4: roiZoomX = roiZoomY = 1.0f * 4; break;
                // Stretch: fills the control, aspect ratio not preserved
                case EnumZoom.Stretch:
                    {
                        roiZoomX = controlWidth / imageWidth;
                        roiZoomY = controlHeight / imageHeight;
                    }
                    break;
                // Fit width or height, aspect ratio preserved
                case EnumZoom.FitW:
                    roiZoomX = controlWidth / imageWidth;
                    roiZoomY = roiZoomX;
                    break;
                case EnumZoom.FitH:
                    roiZoomY = controlHeight / imageHeight;
                    roiZoomX = roiZoomY;
                    break;
                // Fit: the whole image is visible, aspect ratio preserved (smaller of the two factors)
                case EnumZoom.Fit:
                    {
                        roiZoomX = controlWidth / imageWidth;
                        roiZoomY = controlHeight / imageHeight;
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
                case EnumZoom.Manual: roiZoomX = roiZoomY = manualZoom; break;
            }
            coordinateManager.SetTrasfZoom(roiZoomX, roiZoomY);
        }



       
        #endregion

        #region Moving Origin Point

        /// <summary>
        /// Invalidates the control.
        /// </summary>
        protected void ForceRefresh()
        {
            control?.Invalidate();
        }

   
        /// <summary>
        /// Scrolls the image by (<paramref name="deltaX"/>, <paramref name="deltaY"/>), unless <see cref="ImageBlockScroll"/> is set.
        /// </summary>
        public void WorldOriginMove(double deltaX, double deltaY)
        {
            if (!ImageBlockScroll)
            {
                UpdateControlAndImageRoi();
                coordinateManager.TranslateTrasformationOrigin(deltaX, deltaY);
                ForceRefresh();
            }
        }

        /// <summary>
        /// Scrolls the image by <paramref name="delta"/>.
        /// </summary>
        public void WorldOriginMove(Point delta)
        {
            WorldOriginMove(delta.X, delta.Y);
        }

        /// <summary>
        /// Scrolls the image by <paramref name="delta"/>.
        /// </summary>
        public void WorldOriginMove(PointF delta)
        {
            WorldOriginMove(delta.X, delta.Y);
        }

        /// <summary>
        /// Moves the image back to the top left corner of the control, cancelling every scroll.
        /// <para/>
        /// The zoom mode is not changed; the region of interest follows the image.
        /// </summary>
        public void ResetPosition()
        {
            lock (locker)
            {
                imageOrigin = new PointF(0, 0);
                firstClick = true;
                mouseManager.Reset();
                coordinateManager.TransfOrigin = new PointF(0, 0);
            }
            ForceRefresh();
        }

        /// <summary>
        /// Same as <see cref="ResetPosition"/>.
        /// </summary>
        public void OriginZero() => ResetPosition();

        /// <summary>
        /// A double click with the drag button, with no drawing tool, resets the position of the image.
        /// </summary>
        protected void OnMouseDoubleClick(object? sender, MouseEventArgs args)
        {
            // Not with a tool or on a ROI handle: a double click there belongs to the shape (it closes a polygon)
            if (args.Button == MouseMovingButton && !shapeDoubleClick && ShapeTool == EnumShapeTool.None && polygonDraft.Count == 0
                && HitHandle(args.Location) < 0 && !HitCenter(args.Location))
                ResetPosition();
        }

      
        #endregion


        /// <summary>
        /// Creates a small square image filled with <paramref name="color"/>, used as menu icon.
        /// </summary>
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

        /// <summary>
        /// Updates the "add color" menu icon with the color under the mouse.
        /// </summary>
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            toolStripMenuItemAddColor.Image?.Dispose();
            toolStripMenuItemAddColor.Image = MakeColorSwatch(MouseImageColor, 16, 2);
        }
    }
}
