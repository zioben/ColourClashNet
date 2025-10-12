using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ColourClashNet
{
    public partial class FormCClash : Form
    {
        public FormCClash()
        {
            InitializeComponent();
            TabManager.TabPages.Clear();
        }

        void CreateTab()
        {
            var caNew = new ColorAnalyzer();
            caNew.Dock = caTemplate.Dock;
            caNew.Location = caTemplate.Location;
            caNew.Name = $"ca.{DateTime.Now.Ticks}";
            caNew.Size = caTemplate.Size;
            caNew.OnCopyImage += ((s, e) => CreateTab(e.DestBitmap));
            TabManager.CreatePage(caNew);
        }
        void CreateTab(Image oSrcImage)
        {
            var caNew = new ColorAnalyzer();
            caNew.Dock = caTemplate.Dock;
            caNew.Location = caTemplate.Location;
            caNew.Name = $"ca.{DateTime.Now.Ticks}";
            caNew.Size = caTemplate.Size;
            caNew.OnCopyImage += ((s, e) => CreateTab(e.DestBitmap));
            TabManager.CreatePage(caNew);
            caNew.Create(oSrcImage);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTab();
        }

        private void TabManager_TabClosing(object sender, ColourClashNet.Controls.TabControlExt.TabEventArgs e)
        {
            if (MessageBox.Show($"Close {e.TabText }?", "Closing Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Canceled = true;
            } 
        }

        private void TabManager_TabClosed(object sender, ColourClashNet.Controls.TabControlExt.TabEventArgs e)
        {
            TabManager.RemovePage(e.TabClosed);
        }
    }
}