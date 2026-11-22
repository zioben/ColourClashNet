using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{
    /// <summary>
    /// Tab control with a close icon on each tab, used to host one <c>ColorAnalyzer</c> per tab.
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
            /// Visible tab title (without the reserved trailing characters).
            /// </summary>
            public string TabText => TabClosed?.Text?.Substring(0,TabClosed.Text.Length-3) ?? string.Empty; 
            /// <summary>
            /// Area of the close icon.
            /// </summary>
            public Rectangle ButtonRoi { get; set; }
            /// <summary>
            /// Mouse event that triggered the close.
            /// </summary>
            public MouseEventArgs MouseEventArgs { get; set; }
            /// <summary>
            /// Set to true in <see cref="TabClosing"/> to keep the tab open.
            /// </summary>
            public bool Canceled { get; set; }
        }

        Dictionary<string, TabPage> tabPages = new Dictionary<string, TabPage>();
        private System.ComponentModel.IContainer components;
        /// <summary>
        /// Raised when the close icon is clicked; can be cancelled.
        /// </summary>
        public event EventHandler<TabEventArgs> TabClosing;
        /// <summary>
        /// Raised after <see cref="TabClosing"/> when the close has not been cancelled.
        /// </summary>
        public event EventHandler<TabEventArgs> TabClosed;

        /// <summary>
        /// Creates the control.
        /// </summary>
        public TabControlExt() : base()
        {
            InitializeComponent();
            ShowToolTips = true;
            RegisterEvents();
        }
        /// <summary>
        /// Wires owner draw and mouse events.
        /// </summary>
        void RegisterEvents()
        {
            DrawItem += TabManager_DrawItem_1;
            MouseDown += TabManager_MouseDown_1;
        }

        /// <summary>
        /// Adds a tab hosting <paramref name="content"/>; the tab is indexed by the control name.
        /// </summary>
        /// <returns>The new tab page.</returns>
        public TabPage CreatePage(Control content)
        {
            var newPage = new TabPage();
            newPage.Name = $"{content.Name}";
            newPage.Controls.Add(content);
            newPage.Tag = content;
            SetTitle(newPage, $"Image {++pageCounter}");
            TabPages.Add(newPage);
            tabPages[content.Name] = newPage;   
            return newPage;   
        }
        /// <summary>
        /// Removes the tab hosting <paramref name="control"/>.
        /// </summary>
        public void RemovePage(Control control)
        {
            var tableId = control.Name;
            if (tabPages.ContainsKey(tableId))
            {
                var tabPage = tabPages[tableId];
                if (TabPages.ContainsKey(tableId))
                {
                    TabPages.Remove(tabPage);
                }
                tabPages.Remove(tableId);
            }
        }

        /// <summary>
        /// Sets the title of the tab hosting <paramref name="control"/> (the last 3 characters are reserved and not drawn).
        /// </summary>
        public void SetPageText(Control control, string text)
        {
            var tableId = control.Name;
            if (tabPages.ContainsKey(tableId))
            {
                SetTitle(tabPages[tableId], string.IsNullOrWhiteSpace(text) ? $"Image {tabPages.Keys.ToList().IndexOf(tableId) + 1}" : text);
            }
        }

        /// <summary>
        /// Maximum number of characters of a tab title; longer titles are shortened with an ellipsis
        /// and shown in full in the tooltip.
        /// </summary>
        public const int MaxTitleLength = 24;

        // Progressive number of the tabs, used for the default titles ("Image 1", "Image 2", ...)
        int pageCounter;

        /// <summary>
        /// Sets the title of <paramref name="page"/>: shortened to <see cref="MaxTitleLength"/> characters, full text in the tooltip.
        /// <para/>
        /// Three trailing characters are appended: they reserve room for the close icon and are never drawn
        /// (the tab width is measured on the whole text).
        /// </summary>
        static void SetTitle(TabPage page, string title)
        {
            title = title.Trim();
            var shown = title.Length > MaxTitleLength ? title[..(MaxTitleLength - 1)] + "…" : title;
            page.Text = shown + "OOO";
            page.ToolTipText = title;
        }

        /// <summary>
        /// Draws the tab title and the close icon.
        /// </summary>
        void TabManager_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = TabPages[e.Index];
            Rectangle tabRect = GetTabRect(e.Index);

            // Move the text right, leaving room for the close icon
            tabRect.X += 8;

            TextRenderer.DrawText(e.Graphics, tabPage.Text.Substring(0, tabPage.Text.Length - 3), e.Font, tabRect, tabPage.ForeColor);

            Image icon = imageList2.Images[0];
            e.Graphics.DrawImage(icon, e.Bounds.Left + 6, e.Bounds.Top + 3);
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Returns the area of the close icon of the selected tab.
        /// </summary>
        Rectangle GetRoi()
        {
            Image icon = imageList2.Images[0];
            Rectangle tabRect = GetTabRect(SelectedIndex);
            var roi = new Rectangle(tabRect.X + 6, tabRect.Y + 3, icon.Width, icon.Height);
            return roi;
        }

        /// <summary>
        /// Returns true when <paramref name="location"/> is on the close icon of the selected tab.
        /// </summary>
        public bool InsideCloseButtonTab(Point location)
            =>GetRoi().Contains(location);


        /// <summary>
        /// Raises the closing events when the close icon is clicked.
        /// </summary>
        void TabManager_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (InsideCloseButtonTab(e.Location))
                {
                    var tab = this.SelectedTab;
                    if ( tab == null)
                    {
                        return;
                    }
                    var args = new TabEventArgs()
                    {
                        ButtonRoi = GetRoi(),
                        TabClosed = tab,
                        Canceled = false,
                        MouseEventArgs = e
                    };
                    TabClosing?.Invoke(this, args);
                    if (args.Canceled) { return; }
                    TabClosed?.Invoke(this, args);
                }
            }
        }
    }
}
