using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{
    /// <summary>
    /// Tab control with a close button [X] on each tab, used to host one <c>ColorAnalyzer</c> per tab.
    /// <para/>
    /// The tabs are owner drawn: the title on the left, the close button on the right.
    /// Clicking the button raises <see cref="TabClosing"/> (cancelable); when not cancelled the tab is removed,
    /// its content disposed and <see cref="TabClosed"/> raised.
    /// </summary>
    public partial class TabControlExt : TabControl
    {
        /// <summary>
        /// Event data for the tab closing events.
        /// </summary>
        public class TabEventArgs : EventArgs
        {
            /// <summary>
            /// Tab being closed.
            /// </summary>
            public TabPage TabClosed { get; set; }

            /// <summary>
            /// Full tab title.
            /// </summary>
            public string TabText => TabClosed?.ToolTipText is { Length: > 0 } title ? title : TabClosed?.Text ?? string.Empty;

            /// <summary>
            /// Content of the tab (e.g. the <c>ColorAnalyzer</c>).
            /// </summary>
            public Control? Content => TabClosed?.Tag as Control;

            /// <summary>
            /// Set to true in <see cref="TabClosing"/> to keep the tab open.
            /// </summary>
            public bool Canceled { get; set; }
        }

        /// <summary>
        /// Maximum number of characters of a tab title; longer titles are shortened with an ellipsis
        /// and shown in full in the tooltip.
        /// </summary>
        public const int MaxTitleLength = 24;

        /// <summary>
        /// Side of the close button, in pixels.
        /// </summary>
        const int CloseButtonSize = 14;

        /// <summary>
        /// Horizontal room added to each tab for the close button, in pixels.
        /// </summary>
        const int CloseButtonRoom = 22;

        private System.ComponentModel.IContainer components;

        // Tabs indexed by the name of their content control
        readonly Dictionary<string, TabPage> tabPages = new();

        // Title requested for each tab, before the _1, _2 suffix that keeps the titles unique
        readonly Dictionary<TabPage, string> baseTitles = new();

        // Progressive number of the tabs, used for the default titles ("Image 1", "Image 2", ...)
        int pageCounter;

        // Tab whose close button is under the mouse (drawn highlighted), -1 when none
        int hotCloseIndex = -1;

        /// <summary>
        /// Raised when the close button is clicked; set <see cref="TabEventArgs.Canceled"/> to keep the tab.
        /// </summary>
        public event EventHandler<TabEventArgs> TabClosing;

        /// <summary>
        /// Raised after the tab has been removed.
        /// </summary>
        public event EventHandler<TabEventArgs> TabClosed;

        /// <summary>
        /// Creates the control.
        /// </summary>
        public TabControlExt() : base()
        {
            InitializeComponent();
            // Owner drawn tabs: without it Windows draws the plain text and no close button
            DrawMode = TabDrawMode.OwnerDrawFixed;
            // Extra horizontal room for the close button
            Padding = new Point(CloseButtonRoom / 2 + 4, 4);
            ShowToolTips = true;
            DrawItem += TabControlExt_DrawItem;
            MouseDown += TabControlExt_MouseDown;
            MouseMove += TabControlExt_MouseMove;
            MouseLeave += (s, e) => SetHotCloseIndex(-1);
        }

        /// <summary>
        /// Adds a tab hosting <paramref name="content"/>; the tab is indexed by the control name.
        /// </summary>
        /// <returns>The new tab page.</returns>
        public TabPage CreatePage(Control content)
        {
            var newPage = new TabPage
            {
                Name = content.Name,
                Tag = content,
            };
            newPage.Controls.Add(content);
            TabPages.Add(newPage);
            tabPages[content.Name] = newPage;
            SetTitle(newPage, $"Image {++pageCounter}");
            return newPage;
        }

        /// <summary>
        /// Removes the tab hosting <paramref name="control"/>.
        /// </summary>
        public void RemovePage(Control control)
        {
            if (tabPages.TryGetValue(control.Name, out var tabPage))
            {
                TabPages.Remove(tabPage);
                tabPages.Remove(control.Name);
                baseTitles.Remove(tabPage);
            }
        }

        /// <summary>
        /// Sets the title of the tab hosting <paramref name="control"/>.
        /// <para/>
        /// When another tab already uses the same title, a suffix keeps it unique: <c>photo</c>, <c>photo_1</c>, <c>photo_2</c>.
        /// </summary>
        public void SetPageText(Control control, string text)
        {
            if (tabPages.TryGetValue(control.Name, out var tabPage))
            {
                SetTitle(tabPage, string.IsNullOrWhiteSpace(text) ? $"Image {TabPages.IndexOf(tabPage) + 1}" : text);
            }
        }

        /// <summary>
        /// Sets a unique title: shortened to <see cref="MaxTitleLength"/> characters, full text in the tooltip.
        /// </summary>
        void SetTitle(TabPage page, string title)
        {
            title = title.Trim();
            baseTitles[page] = title;

            // Titles already used by the other tabs
            var usedTitles = TabPages.Cast<TabPage>()
                .Where(p => p != page)
                .Select(p => p.ToolTipText)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var uniqueTitle = title;
            for (int n = 1; usedTitles.Contains(uniqueTitle); n++)
                uniqueTitle = $"{title}_{n}";

            page.ToolTipText = uniqueTitle;
            page.Text = uniqueTitle.Length > MaxTitleLength ? uniqueTitle[..(MaxTitleLength - 1)] + "…" : uniqueTitle;
        }

        /// <summary>
        /// Returns the area of the close button of the tab <paramref name="index"/>.
        /// </summary>
        Rectangle GetCloseButtonRect(int index)
        {
            var tabRect = GetTabRect(index);
            return new Rectangle(
                tabRect.Right - CloseButtonSize - 5,
                tabRect.Top + (tabRect.Height - CloseButtonSize) / 2,
                CloseButtonSize,
                CloseButtonSize);
        }

        /// <summary>
        /// Returns the index of the tab whose close button contains <paramref name="location"/>, or -1.
        /// </summary>
        int GetCloseButtonIndex(Point location)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetCloseButtonRect(i).Contains(location))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Draws the tab title and the close button [X].
        /// </summary>
        void TabControlExt_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabCount)
                return;
            var tabPage = TabPages[e.Index];
            var tabRect = GetTabRect(e.Index);
            bool selected = e.Index == SelectedIndex;

            // Title, left aligned, leaving room for the close button
            var textRect = new Rectangle(tabRect.X + 6, tabRect.Y, tabRect.Width - CloseButtonRoom - 6, tabRect.Height);
            var textFont = selected ? new Font(e.Font ?? Font, FontStyle.Bold) : (e.Font ?? Font);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, textFont, textRect, SystemColors.ControlText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (selected)
                textFont.Dispose();

            // Close button: a small box with an X, red when the mouse is over it
            var buttonRect = GetCloseButtonRect(e.Index);
            bool hot = e.Index == hotCloseIndex;
            if (hot)
            {
                using var hotBrush = new SolidBrush(System.Drawing.Color.FromArgb(232, 17, 35));
                e.Graphics.FillRectangle(hotBrush, buttonRect);
            }
            using var pen = new Pen(hot ? System.Drawing.Color.White : SystemColors.ControlDarkDark, 1.6f);
            var cross = Rectangle.Inflate(buttonRect, -4, -4);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, cross.Left, cross.Top, cross.Right, cross.Bottom);
            e.Graphics.DrawLine(pen, cross.Right, cross.Top, cross.Left, cross.Bottom);
        }

        /// <summary>
        /// Highlights the close button under the mouse.
        /// </summary>
        void TabControlExt_MouseMove(object? sender, MouseEventArgs e)
            => SetHotCloseIndex(GetCloseButtonIndex(e.Location));

        /// <summary>
        /// Changes the highlighted close button and repaints the affected tabs.
        /// </summary>
        void SetHotCloseIndex(int index)
        {
            if (index == hotCloseIndex)
                return;
            if (hotCloseIndex >= 0 && hotCloseIndex < TabCount)
                Invalidate(GetTabRect(hotCloseIndex));
            hotCloseIndex = index;
            if (hotCloseIndex >= 0)
                Invalidate(GetTabRect(hotCloseIndex));
        }

        /// <summary>
        /// Closes the tab whose close button has been clicked (after <see cref="TabClosing"/>).
        /// </summary>
        void TabControlExt_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int index = GetCloseButtonIndex(e.Location);
            if (index < 0)
                return;
            CloseTab(TabPages[index]);
        }

        /// <summary>
        /// Closes <paramref name="tabPage"/>: raises <see cref="TabClosing"/>, then removes the tab, disposes its content
        /// and raises <see cref="TabClosed"/>.
        /// </summary>
        /// <returns>True when the tab has been closed, false when the closing was cancelled.</returns>
        public bool CloseTab(TabPage tabPage)
        {
            var args = new TabEventArgs { TabClosed = tabPage };
            TabClosing?.Invoke(this, args);
            if (args.Canceled)
                return false;

            int index = TabPages.IndexOf(tabPage);
            var content = args.Content;
            if (content != null)
                RemovePage(content);
            else
                TabPages.Remove(tabPage);
            hotCloseIndex = -1;
            // Select the tab on the left of the closed one, as most editors do
            if (TabCount > 0)
                SelectedIndex = Math.Clamp(index - 1, 0, TabCount - 1);
            TabClosed?.Invoke(this, args);
            tabPage.Dispose();
            return true;
        }
    }
}
