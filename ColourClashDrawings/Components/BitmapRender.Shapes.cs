using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Imaging.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ColourClashNet.Components
{
    /// <summary>
    /// Region of interest (ROI) drawing and editing, with the right mouse button.
    /// <para/>
    /// The shape is stored in image coordinates, so it follows the zoom and the scroll of the image.
    /// <list type="bullet">
    /// <item>Rectangle, Ellipse: drag to draw.</item>
    /// <item>Freehand: drag to draw, the path is closed on release.</item>
    /// <item>Polygon: every click adds a vertex; double click or click on the first vertex to close it.</item>
    /// <item>Drag a handle (box corners and edge midpoints, polygon vertices) to resize or reshape; drag inside the shape to move it.</item>
    /// <item>Click without dragging inside the shape, with no drawing tool, or with Ctrl: context menu.</item>
    /// </list>
    /// </summary>
    public partial class BitmapRender
    {
        /// <summary>
        /// Drawing tools of the right mouse button.
        /// </summary>
        public enum EnumShapeTool
        {
            /// <summary>No drawing: the right button opens the context menu and edits an existing shape.</summary>
            None,
            /// <summary>Free hand ROI.</summary>
            Freehand,
            /// <summary>Rectangular ROI.</summary>
            Rectangle,
            /// <summary>Elliptic ROI.</summary>
            Ellipse,
            /// <summary>Polygonal ROI, one vertex per click.</summary>
            Polygon,
        }

        /// <summary>
        /// What a right button drag is doing.
        /// </summary>
        enum EnumShapeDrag
        {
            None,
            NewBox,
            NewFreehand,
            MoveShape,
            MoveHandle,
        }

        /// <summary>
        /// Half size of an editing handle, in control pixels (independent of the zoom).
        /// </summary>
        const int HandleHalfSize = 4;

        /// <summary>
        /// Mouse movement, in control pixels, below which a press and release is a click and not a drag.
        /// </summary>
        const int DragThreshold = 3;

        /// <summary>
        /// Raised when <see cref="Shape"/> changes (while drawing or editing too).
        /// </summary>
        [Browsable(true), Category("Shape")]
        [Description("Raised when the region of interest changes")]
        public event EventHandler? ShapeChanged;

        /// <summary>
        /// Raised when <see cref="ShapeTool"/> changes.
        /// </summary>
        [Browsable(true), Category("Shape")]
        [Description("Raised when the drawing tool changes")]
        public event EventHandler? ShapeToolChanged;

        /// <summary>
        /// Raised when "Extract shape" is chosen in the context menu: the host decides what to do with the image.
        /// </summary>
        [Browsable(true), Category("Shape")]
        [Description("Raised when the shape extraction is requested from the context menu")]
        public event EventHandler? ShapeExtractRequested;

        RoiShape? shape;
        /// <summary>
        /// Current region of interest, in image coordinates; null when there is none.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RoiShape? Shape
        {
            get => shape;
            set
            {
                if (!Equals(shape, value))
                {
                    shape = value;
                    ForceRefresh();
                    ShapeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        EnumShapeTool shapeTool = EnumShapeTool.None;
        /// <summary>
        /// Drawing tool of the right mouse button. Changing it cancels a polygon being drawn.
        /// </summary>
        [Browsable(true), Category("Shape")]
        [DefaultValue(EnumShapeTool.None)]
        public EnumShapeTool ShapeTool
        {
            get => shapeTool;
            set
            {
                if (shapeTool != value)
                {
                    shapeTool = value;
                    polygonDraft.Clear();
                    UpdateShapeMenuItems();
                    ForceRefresh();
                    ShapeToolChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Outline color of the shape.
        /// </summary>
        [Browsable(true), Category("Shape")]
        [DefaultValue(typeof(System.Drawing.Color), "Yellow")]
        public System.Drawing.Color ShapeColor { get; set; } = System.Drawing.Color.Yellow;

        // Right button drag state
        bool rightPressed;
        bool rightDragged;
        bool rightMenuRequested;
        Point rightPressPoint;
        PointF dragStartImagePoint;
        RoiShape? dragStartShape;
        int dragHandle = -1;
        EnumShapeDrag dragMode = EnumShapeDrag.None;
        readonly List<PointF> freehandPoints = new();

        // Vertices of the polygon being drawn, in image coordinates, and the position of the next one
        readonly List<PointF> polygonDraft = new();
        PointF polygonPreview;

        // Context menu items created at run time
        ToolStripMenuItem? toolStripMenuItemShape;
        readonly Dictionary<EnumShapeTool, ToolStripMenuItem> shapeToolMenuItems = new();

        /// <summary>
        /// Removes the shape and cancels a polygon being drawn.
        /// </summary>
        public void ResetShape()
        {
            polygonDraft.Clear();
            Shape = null;
            ForceRefresh();
        }

        /// <summary>
        /// Cuts <see cref="Shape"/> out of the image: the pixels outside the shape are set to <paramref name="outsideColor"/>.
        /// </summary>
        /// <param name="outsideColor">Packed color of the pixels outside the shape</param>
        /// <returns>The extracted image, or null when there is no valid shape or image.</returns>
        public ImageData? ExtractShape(int outsideColor)
        {
            if (shape == null || !shape.IsValid || Image == null)
                return null;
            var imageData = ImageToolsGDI.GdiImageToImageData(Image);
            return imageData == null ? null : shape.ExtractFrom(imageData, outsideColor);
        }

        #region Context menu

        /// <summary>
        /// Adds the position and shape items to the context menu.
        /// </summary>
        void CreateShapeMenuItems()
        {
            var resetPosition = new ToolStripMenuItem("Reset position");
            resetPosition.Click += (s, e) => ResetPosition();
            contextMenuStrip.Items.Insert(1, resetPosition);

            toolStripMenuItemShape = new ToolStripMenuItem("Shape");
            foreach (var tool in Enum.GetValues<EnumShapeTool>())
            {
                var item = new ToolStripMenuItem(tool.ToString()) { Tag = tool };
                item.Click += (s, e) => ShapeTool = tool;
                shapeToolMenuItems[tool] = item;
                toolStripMenuItemShape.DropDownItems.Add(item);
            }
            toolStripMenuItemShape.DropDownItems.Add(new ToolStripSeparator());
            var resetShape = new ToolStripMenuItem("Reset shape");
            resetShape.Click += (s, e) => ResetShape();
            toolStripMenuItemShape.DropDownItems.Add(resetShape);
            var extractShape = new ToolStripMenuItem("Extract shape");
            extractShape.Click += (s, e) => ShapeExtractRequested?.Invoke(this, EventArgs.Empty);
            toolStripMenuItemShape.DropDownItems.Add(extractShape);
            toolStripMenuItemShape.DropDownOpening += (s, e) =>
            {
                bool hasShape = shape?.IsValid == true;
                resetShape.Enabled = hasShape || polygonDraft.Count > 0;
                extractShape.Enabled = hasShape && Image != null;
            };
            contextMenuStrip.Items.Insert(2, toolStripMenuItemShape);
            contextMenuStrip.Items.Insert(3, new ToolStripSeparator());
            UpdateShapeMenuItems();
        }

        /// <summary>
        /// Checks the menu item of the current tool.
        /// </summary>
        void UpdateShapeMenuItems()
        {
            foreach (var (tool, item) in shapeToolMenuItems)
                item.Checked = tool == shapeTool;
        }

        /// <summary>
        /// Opens the context menu at <paramref name="location"/> (control coordinates).
        /// </summary>
        void ShowContextMenu(Point location)
        {
            if (control != null)
                contextMenuStrip.Show(control, location);
        }

        #endregion

        #region Right button handling

        /// <summary>
        /// Converts a control point to image coordinates.
        /// </summary>
        PointF ToImage(Point controlPoint)
        {
            UpdateControlAndImageRoi();
            return coordinateManager.InverseTransform(controlPoint);
        }

        /// <summary>
        /// Converts an image point to control coordinates.
        /// </summary>
        PointF ToControl(PointF imagePoint) => coordinateManager.Transform(imagePoint);

        /// <summary>
        /// Rounds the box of rectangles and ellipses to whole pixels, so the drawn box is exactly the extracted one.
        /// </summary>
        static RoiShape? SnapToPixels(RoiShape? roi) => roi is BoxRoiShape box ? box.SnapToPixels() : roi;

        /// <summary>
        /// Size of the region of interest in image pixels (the size of the extracted image), or empty when there is none.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size ShapePixelSize => shape == null ? Size.Empty : shape.PixelBounds.Size;

        /// <summary>
        /// Keeps a point inside the image.
        /// </summary>
        PointF ClampToImage(PointF point)
            => Image == null ? point : new PointF(Math.Clamp(point.X, 0, Image.Width), Math.Clamp(point.Y, 0, Image.Height));

        /// <summary>
        /// Returns the index of the handle of the shape under <paramref name="controlPoint"/>, or -1.
        /// </summary>
        int HitHandle(Point controlPoint)
        {
            if (shape == null)
                return -1;
            UpdateControlAndImageRoi();
            var handles = shape.Handles;
            for (int i = 0; i < handles.Count; i++)
            {
                var p = ToControl(handles[i]);
                if (Math.Abs(p.X - controlPoint.X) <= HandleHalfSize + 1 && Math.Abs(p.Y - controlPoint.Y) <= HandleHalfSize + 1)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// True when <paramref name="controlPoint"/> is inside the shape.
        /// </summary>
        bool HitShape(Point controlPoint)
        {
            if (shape == null)
                return false;
            var p = ToImage(controlPoint);
            return shape.Contains(p.X, p.Y);
        }

        /// <summary>
        /// Starts a right button action: handle drag, shape move, or a new shape of the current tool.
        /// </summary>
        void OnShapeMouseDown(MouseEventArgs args)
        {
            if (args.Button != MouseSelectButton || Image == null)
                return;
            rightPressed = true;
            rightDragged = false;
            rightPressPoint = args.Location;
            rightMenuRequested = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
            dragMode = EnumShapeDrag.None;
            if (rightMenuRequested)
                return;

            // Polygon: a double click closes the polygon being drawn
            if (shapeTool == EnumShapeTool.Polygon && polygonDraft.Count > 0)
            {
                if (args.Clicks >= 2)
                {
                    ClosePolygon();
                    rightPressed = false;
                }
                return;
            }

            dragStartImagePoint = ToImage(args.Location);
            dragStartShape = shape;
            if ((dragHandle = HitHandle(args.Location)) >= 0)
                dragMode = EnumShapeDrag.MoveHandle;
            else if (HitShape(args.Location))
                dragMode = EnumShapeDrag.MoveShape;
            else if (shapeTool is EnumShapeTool.Rectangle or EnumShapeTool.Ellipse)
                dragMode = EnumShapeDrag.NewBox;
            else if (shapeTool == EnumShapeTool.Freehand)
            {
                dragMode = EnumShapeDrag.NewFreehand;
                freehandPoints.Clear();
                freehandPoints.Add(ClampToImage(dragStartImagePoint));
            }
        }

        /// <summary>
        /// Updates the shape while dragging, the polygon preview and the cursor.
        /// </summary>
        void OnShapeMouseMove(MouseEventArgs args)
        {
            if (Image == null)
                return;
            if (rightPressed && !rightDragged)
            {
                var delta = args.Location.Sub(rightPressPoint);
                rightDragged = Math.Abs(delta.X) > DragThreshold || Math.Abs(delta.Y) > DragThreshold;
            }

            var imagePoint = ToImage(args.Location);
            if (rightPressed && rightDragged)
            {
                switch (dragMode)
                {
                    case EnumShapeDrag.NewBox:
                        {
                            var box = RoiShape.RectangleFromCorners(ClampToImage(dragStartImagePoint), ClampToImage(imagePoint));
                            BoxRoiShape newShape = shapeTool == EnumShapeTool.Ellipse ? new EllipseRoiShape(box) : new RectangleRoiShape(box);
                            Shape = newShape.SnapToPixels();
                        }
                        break;
                    case EnumShapeDrag.NewFreehand:
                        {
                            var point = ClampToImage(imagePoint);
                            var last = freehandPoints[^1];
                            // Skip the points closer than one image pixel: they only make the path heavier
                            if (Math.Abs(point.X - last.X) >= 1 || Math.Abs(point.Y - last.Y) >= 1)
                            {
                                freehandPoints.Add(point);
                                Shape = new FreehandRoiShape(freehandPoints.ToArray());
                            }
                        }
                        break;
                    case EnumShapeDrag.MoveShape:
                        Shape = SnapToPixels(dragStartShape?.Translate(imagePoint.X - dragStartImagePoint.X, imagePoint.Y - dragStartImagePoint.Y));
                        break;
                    case EnumShapeDrag.MoveHandle:
                        Shape = SnapToPixels(dragStartShape?.MoveHandle(dragHandle, ClampToImage(imagePoint)));
                        break;
                }
            }
            else if (shapeTool == EnumShapeTool.Polygon && polygonDraft.Count > 0)
            {
                polygonPreview = ClampToImage(imagePoint);
                ForceRefresh();
            }

            if (!rightPressed && mouseManager.MouseState != MouseHandler.EnumMouseState.tracking)
                UpdateShapeCursor(args.Location);
        }

        /// <summary>
        /// Ends a right button action: completes a drag, adds a polygon vertex, or opens the context menu on a click.
        /// </summary>
        void OnShapeMouseUp(MouseEventArgs args)
        {
            if (args.Button != MouseSelectButton || !rightPressed)
                return;
            rightPressed = false;
            var mode = dragMode;
            dragMode = EnumShapeDrag.None;

            if (rightDragged && !rightMenuRequested)
            {
                // A shape too small to be useful is discarded and the previous one restored
                if ((mode == EnumShapeDrag.NewBox || mode == EnumShapeDrag.NewFreehand) && shape?.IsValid != true)
                    Shape = dragStartShape;
                return;
            }

            // Click without dragging
            if (!rightMenuRequested && shapeTool == EnumShapeTool.Polygon && Image != null && (polygonDraft.Count > 0 || !HitShape(args.Location)))
            {
                AddPolygonVertex(args.Location);
                return;
            }
            ShowContextMenu(args.Location);
        }

        /// <summary>
        /// Adds a vertex to the polygon being drawn; a click on the first vertex closes the polygon.
        /// </summary>
        void AddPolygonVertex(Point controlPoint)
        {
            if (polygonDraft.Count >= 3)
            {
                var first = ToControl(polygonDraft[0]);
                if (Math.Abs(first.X - controlPoint.X) <= HandleHalfSize + 2 && Math.Abs(first.Y - controlPoint.Y) <= HandleHalfSize + 2)
                {
                    ClosePolygon();
                    return;
                }
            }
            var point = ClampToImage(ToImage(controlPoint));
            polygonDraft.Add(point);
            polygonPreview = point;
            ForceRefresh();
        }

        /// <summary>
        /// Turns the polygon being drawn into <see cref="Shape"/> (at least 3 vertices).
        /// </summary>
        void ClosePolygon()
        {
            if (polygonDraft.Count >= 3)
            {
                var polygon = new PolygonRoiShape(polygonDraft.ToArray());
                polygonDraft.Clear();
                if (polygon.IsValid)
                    Shape = polygon;
            }
            ForceRefresh();
        }

        /// <summary>
        /// Shows a resize cursor on the handles, a move cursor inside the shape and a cross while a tool is active.
        /// </summary>
        void UpdateShapeCursor(Point controlPoint)
        {
            if (control == null)
                return;
            Cursor cursor = shapeTool == EnumShapeTool.None ? Cursors.Default : Cursors.Cross;
            int handle = HitHandle(controlPoint);
            if (handle >= 0)
            {
                cursor = shape is BoxRoiShape
                    ? (handle % 4) switch
                    {
                        0 => Cursors.SizeNWSE,
                        1 => Cursors.SizeNS,
                        2 => Cursors.SizeNESW,
                        _ => Cursors.SizeWE,
                    }
                    : Cursors.Hand;
            }
            else if (HitShape(controlPoint))
            {
                cursor = Cursors.SizeAll;
            }
            if (control.Cursor != cursor)
                control.Cursor = cursor;
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draws the shape, its handles and the polygon being drawn over the image.
        /// </summary>
        void DrawShapes(Graphics graphics)
        {
            if (Image == null || (shape == null && polygonDraft.Count == 0))
                return;
            var previousSmoothing = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var outlinePen = new Pen(ShapeColor, 1.5f);
            using var dashPen = new Pen(System.Drawing.Color.Black, 1.5f) { DashStyle = DashStyle.Dash };
            using var fillBrush = new SolidBrush(System.Drawing.Color.FromArgb(40, ShapeColor));

            if (shape != null)
            {
                var outline = shape.Outline.Select(ToControl).ToArray();
                if (outline.Length >= 2)
                {
                    graphics.FillPolygon(fillBrush, outline);
                    // Solid line plus dashed black line: visible on light and dark images
                    graphics.DrawPolygon(outlinePen, outline);
                    graphics.DrawPolygon(dashPen, outline);
                }
                foreach (var handle in shape.Handles)
                    DrawHandle(graphics, ToControl(handle));
            }

            if (polygonDraft.Count > 0)
            {
                var points = polygonDraft.Select(ToControl).ToList();
                points.Add(ToControl(polygonPreview));
                if (points.Count >= 2)
                {
                    graphics.DrawLines(outlinePen, points.ToArray());
                    graphics.DrawLines(dashPen, points.ToArray());
                }
                foreach (var vertex in polygonDraft)
                    DrawHandle(graphics, ToControl(vertex));
            }

            DrawSizeLabel(graphics);
            graphics.SmoothingMode = previousSmoothing;
        }

        /// <summary>
        /// While a shape is drawn or edited, shows its size in image pixels next to its bottom right corner.
        /// </summary>
        void DrawSizeLabel(Graphics graphics)
        {
            RoiShape? measured = null;
            if (polygonDraft.Count > 0)
                measured = new PolygonRoiShape(polygonDraft.Append(polygonPreview).ToArray());
            else if (rightPressed && rightDragged && dragMode != EnumShapeDrag.None)
                measured = shape;
            if (measured == null)
                return;

            var pixels = measured.PixelBounds;
            var text = $"{pixels.Width} x {pixels.Height}";
            var font = control?.Font ?? SystemFonts.DefaultFont;
            var textSize = graphics.MeasureString(text, font);
            var corner = ToControl(new PointF(measured.Bounds.Right, measured.Bounds.Bottom));
            var rect = new RectangleF(corner.X + 6, corner.Y + 6, textSize.Width + 6, textSize.Height + 2);
            // Keep the label inside the control
            if (control != null)
            {
                rect.X = Math.Clamp(rect.X, 0, Math.Max(0, control.Width - rect.Width));
                rect.Y = Math.Clamp(rect.Y, 0, Math.Max(0, control.Height - rect.Height));
            }
            using var background = new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.Black));
            graphics.FillRectangle(background, rect);
            graphics.DrawString(text, font, Brushes.White, rect.X + 3, rect.Y + 1);
        }

        /// <summary>
        /// Draws a handle centered on <paramref name="center"/> (control coordinates).
        /// </summary>
        void DrawHandle(Graphics graphics, PointF center)
        {
            var rect = new RectangleF(center.X - HandleHalfSize, center.Y - HandleHalfSize, 2 * HandleHalfSize, 2 * HandleHalfSize);
            graphics.FillRectangle(Brushes.White, rect);
            using var pen = new Pen(System.Drawing.Color.Black, 1);
            graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        #endregion
    }
}
