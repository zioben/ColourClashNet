using ColourClashNet.Imaging.Shapes;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ColourClashNet.Components
{
    /// <summary>
    /// Pixel grid drawn over the image, and alignment of the region of interest to it.
    /// <para/>
    /// The grid has four levels, each with its own line style: 8 pixels dotted, 16 dash dot dot, 64 dash dot, 128 solid.
    /// A level whose lines would be closer than <see cref="GridMinSpacing"/> control pixels is not drawn.
    /// </summary>
    public partial class BitmapRender
    {
        /// <summary>
        /// Grid levels, from the finest: step in image pixels and line style.
        /// </summary>
        static readonly (int Step, DashStyle Style)[] gridLevels =
        {
            (8, DashStyle.Dot),
            (16, DashStyle.DashDotDot),
            (64, DashStyle.DashDot),
            (128, DashStyle.Solid),
        };

        /// <summary>
        /// Grid steps that can be used for the alignment.
        /// </summary>
        public static readonly int[] GridSnapSizes = gridLevels.Select(l => l.Step).ToArray();

        /// <summary>
        /// Minimum distance, in control pixels, between the lines of a grid level.
        /// </summary>
        const int GridMinSpacing = 4;

        /// <summary>
        /// Raised when <see cref="GridVisible"/>, <see cref="GridSnap"/> or <see cref="GridSnapSize"/> change.
        /// </summary>
        [Browsable(true), Category("Grid")]
        [Description("Raised when the grid settings change")]
        public event EventHandler? GridChanged;

        bool gridVisible;
        /// <summary>
        /// Shows the 8 / 16 / 64 / 128 pixel grid over the image.
        /// </summary>
        [Browsable(true), Category("Grid")]
        [DefaultValue(false)]
        public bool GridVisible
        {
            get => gridVisible;
            set
            {
                if (gridVisible != value)
                {
                    gridVisible = value;
                    OnGridChanged();
                }
            }
        }

        bool gridSnap;
        /// <summary>
        /// Aligns the new shape points, the moved handles and the moved shape to a grid of <see cref="GridSnapSize"/> pixels.
        /// <para/>
        /// Freehand paths are not aligned.
        /// </summary>
        [Browsable(true), Category("Grid")]
        [DefaultValue(false)]
        public bool GridSnap
        {
            get => gridSnap;
            set
            {
                if (gridSnap != value)
                {
                    gridSnap = value;
                    OnGridChanged();
                }
            }
        }

        int gridSnapSize = 8;
        /// <summary>
        /// Alignment step in image pixels: one of <see cref="GridSnapSizes"/>.
        /// </summary>
        [Browsable(true), Category("Grid")]
        [DefaultValue(8)]
        public int GridSnapSize
        {
            get => gridSnapSize;
            set
            {
                if (gridSnapSize != value && GridSnapSizes.Contains(value))
                {
                    gridSnapSize = value;
                    OnGridChanged();
                }
            }
        }

        /// <summary>
        /// Color of the grid lines.
        /// </summary>
        [Browsable(true), Category("Grid")]
        [DefaultValue(typeof(System.Drawing.Color), "Cyan")]
        public System.Drawing.Color GridColor { get; set; } = System.Drawing.Color.Cyan;

        // Context menu items of the grid
        ToolStripMenuItem? toolStripMenuItemGridVisible;
        ToolStripMenuItem? toolStripMenuItemGridSnap;
        readonly System.Collections.Generic.Dictionary<int, ToolStripMenuItem> gridSizeMenuItems = new();

        /// <summary>
        /// Updates the menu, redraws and raises <see cref="GridChanged"/>.
        /// </summary>
        void OnGridChanged()
        {
            UpdateGridMenuItems();
            ForceRefresh();
            GridChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Aligns an image point to the snap grid when <see cref="GridSnap"/> is on, and keeps it inside the image.
        /// </summary>
        PointF SnapToGrid(PointF point)
        {
            if (gridSnap)
                point = new PointF(MathF.Round(point.X / gridSnapSize) * gridSnapSize, MathF.Round(point.Y / gridSnapSize) * gridSnapSize);
            return ClampToImage(point);
        }

        /// <summary>
        /// Moves a shape so that the top left corner of its box is on the snap grid, when <see cref="GridSnap"/> is on.
        /// </summary>
        RoiShape? AlignToGrid(RoiShape? roi)
        {
            if (!gridSnap || roi == null)
                return roi;
            var bounds = roi.Bounds;
            var corner = SnapToGrid(new PointF(bounds.Left, bounds.Top));
            return roi.Translate(corner.X - bounds.Left, corner.Y - bounds.Top);
        }

        /// <summary>
        /// Creates the "Grid" context menu: show grid, snap to grid and the snap size.
        /// </summary>
        ToolStripMenuItem CreateGridMenuItem()
        {
            var gridMenu = new ToolStripMenuItem("Grid");
            toolStripMenuItemGridVisible = new ToolStripMenuItem("Show grid");
            toolStripMenuItemGridVisible.Click += (s, e) => GridVisible = !GridVisible;
            toolStripMenuItemGridSnap = new ToolStripMenuItem("Snap to grid");
            toolStripMenuItemGridSnap.Click += (s, e) => GridSnap = !GridSnap;
            gridMenu.DropDownItems.Add(toolStripMenuItemGridVisible);
            gridMenu.DropDownItems.Add(toolStripMenuItemGridSnap);
            gridMenu.DropDownItems.Add(new ToolStripSeparator());
            foreach (var size in GridSnapSizes)
            {
                var item = new ToolStripMenuItem($"Snap {size} x {size}");
                item.Click += (s, e) => GridSnapSize = size;
                gridSizeMenuItems[size] = item;
                gridMenu.DropDownItems.Add(item);
            }
            UpdateGridMenuItems();
            return gridMenu;
        }

        /// <summary>
        /// Checks the grid menu items of the current settings.
        /// </summary>
        void UpdateGridMenuItems()
        {
            if (toolStripMenuItemGridVisible != null)
                toolStripMenuItemGridVisible.Checked = gridVisible;
            if (toolStripMenuItemGridSnap != null)
                toolStripMenuItemGridSnap.Checked = gridSnap;
            foreach (var (size, item) in gridSizeMenuItems)
                item.Checked = size == gridSnapSize;
        }

        /// <summary>
        /// Draws the visible part of the grid over the image.
        /// <para/>
        /// Every line takes the style of the coarsest level it belongs to (a line at x = 128 is solid, at x = 136 dotted).
        /// </summary>
        void DrawGrid(Graphics graphics)
        {
            if (!gridVisible || Image == null || control == null)
                return;
            UpdateControlAndImageRoi();
            float zoomX = Math.Abs(coordinateManager.TransfZoom.X);
            float zoomY = Math.Abs(coordinateManager.TransfZoom.Y);
            if (zoomX <= 0 || zoomY <= 0)
                return;

            // Visible part of the image, in image coordinates
            var topLeft = ToImage(Point.Empty);
            var bottomRight = ToImage(new Point(control.Width, control.Height));
            float left = Math.Max(0, Math.Min(topLeft.X, bottomRight.X));
            float right = Math.Min(Image.Width, Math.Max(topLeft.X, bottomRight.X));
            float top = Math.Max(0, Math.Min(topLeft.Y, bottomRight.Y));
            float bottom = Math.Min(Image.Height, Math.Max(topLeft.Y, bottomRight.Y));
            if (right <= left || bottom <= top)
                return;

            var previousSmoothing = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.None;
            var pens = gridLevels.Select(l => new Pen(System.Drawing.Color.FromArgb(170, GridColor), l.Step >= 128 ? 1.5f : 1f) { DashStyle = l.Style }).ToArray();
            try
            {
                DrawGridLines(graphics, pens, zoomX, left, right, x =>
                {
                    var p1 = ToControl(new PointF(x, top));
                    var p2 = ToControl(new PointF(x, bottom));
                    return (p1, p2);
                });
                DrawGridLines(graphics, pens, zoomY, top, bottom, y =>
                {
                    var p1 = ToControl(new PointF(left, y));
                    var p2 = ToControl(new PointF(right, y));
                    return (p1, p2);
                });
            }
            finally
            {
                foreach (var pen in pens)
                    pen.Dispose();
                graphics.SmoothingMode = previousSmoothing;
            }
        }

        /// <summary>
        /// Draws the grid lines of one direction between <paramref name="from"/> and <paramref name="to"/> (image coordinates).
        /// </summary>
        /// <param name="zoom">Zoom factor in this direction, used to skip the levels that are too dense</param>
        /// <param name="line">Control coordinates of the line at an image coordinate</param>
        static void DrawGridLines(Graphics graphics, Pen[] pens, float zoom, float from, float to, Func<float, (PointF, PointF)> line)
        {
            // Finest level that is not too dense; when even the coarsest one is, nothing is drawn
            int finest = Array.FindIndex(gridLevels, l => l.Step * zoom >= GridMinSpacing);
            if (finest < 0)
                return;
            int step = gridLevels[finest].Step;
            for (int value = (int)Math.Ceiling(from / step) * step; value <= to; value += step)
            {
                // Style of the coarsest level the line belongs to
                int level = finest;
                for (int i = gridLevels.Length - 1; i > finest; i--)
                {
                    if (value % gridLevels[i].Step == 0)
                    {
                        level = i;
                        break;
                    }
                }
                var (p1, p2) = line(value);
                graphics.DrawLine(pens[level], p1, p2);
            }
        }
    }
}
