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
    public partial class TabControlExt : TabControl
    {
        public class TabEventArgs : EventArgs
        {
            public TabPage TabClosed { get; set; }

            public string TabText => TabClosed?.Text?.Substring(0,TabClosed.Text.Length-3) ?? string.Empty; 
            public Rectangle ButtonRoi { get; set; }
            public MouseEventArgs MouseEventArgs { get; set; }
            public bool Canceled { get; set; }
        }

        Dictionary<string, TabPage> oTabPages = new Dictionary<string, TabPage>();
        private System.ComponentModel.IContainer components;
        public event EventHandler<TabEventArgs> TabClosing;
        public event EventHandler<TabEventArgs> TabClosed;

        public TabControlExt() : base()
        {
            InitializeComponent();
            RegisterEvents();
        }
        void RegisterEvents()
        {
            DrawItem += TabManager_DrawItem_1;
            MouseDown += TabManager_MouseDown_1;
        }

        public void CreatePage(Control caNew)
        {
            var tpNew = new TabPage();
            tpNew.Name = $"{caNew.Name}";
            tpNew.Controls.Add(caNew);
            tpNew.Text = $"Page{TabPages.Count + 1}OOO";
            TabPages.Add(tpNew);
            oTabPages[caNew.Name] = tpNew;
        }
        public void RemovePage(Control oControl)
        {
            var sTableID = oControl.Name;
            if (oTabPages.ContainsKey(sTableID))
            {
                var oTp = oTabPages[sTableID];
                if (TabPages.ContainsKey(sTableID))
                {
                    TabPages.Remove(oTp);
                }
                oTabPages.Remove(sTableID);
            }

        }


        void TabManager_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = TabPages[e.Index];
            Rectangle tabRect = GetTabRect(e.Index);

            // Move Text Right 
            tabRect.X += 8;

            TextRenderer.DrawText(e.Graphics, tabPage.Text.Substring(0, tabPage.Text.Length - 3), e.Font, tabRect, tabPage.ForeColor);

            Image icon = imageList2.Images[0];
            e.Graphics.DrawImage(icon, e.Bounds.Left + 6, e.Bounds.Top + 3);
            e.DrawFocusRectangle();
        }

        Rectangle GetRoi()
        {
            Image icon = imageList2.Images[0];
            Rectangle r = GetTabRect(SelectedIndex);
            var Roi = new Rectangle(r.X + 6, r.Y + 3, icon.Width, icon.Height);
            return Roi;
        }

        public bool InsideCloseButtonTab(Point Location)
            =>GetRoi().Contains(Location);


        void TabManager_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (InsideCloseButtonTab(e.Location))
                {
                    var oTab = this.SelectedTab;
                    if ( oTab == null)
                    {
                        return;
                    }
                    var oArgs = new TabEventArgs()
                    {
                        ButtonRoi = GetRoi(),
                        TabClosed = oTab,
                        Canceled = false,
                        MouseEventArgs = e
                    };
                    TabClosing?.Invoke(this, oArgs);
                    if (oArgs.Canceled) { return; }
                    TabClosed?.Invoke(this, oArgs);
                }
            }
        }
    }
}
