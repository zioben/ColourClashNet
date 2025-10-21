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

        ColorAnalyzer CreateTab()
        {
            var caNew = new ColorAnalyzer();
            caNew.Dock = caTemplate.Dock;
            caNew.Location = caTemplate.Location;
            caNew.Name = $"ca.{DateTime.Now.Ticks}";
            caNew.Size = caTemplate.Size;
            caNew.ImageCopied += ((s, e) => CreateTab(e.DestBitmap, e.Name));
            caNew.ImageCreated += ((s, e) => TabManager.SetPageText(caNew, e.Name.Substring(Math.Max(0,e.Name.Length-12))));
            TabManager.CreatePage(caNew);   
            return caNew;
        }


        void CreateTab(Image oSrcImage, string sName) => CreateTab().Create(oSrcImage, sName);

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