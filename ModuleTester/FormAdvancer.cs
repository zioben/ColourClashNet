using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModuleTester
{
    public partial class FormAdvancer : Form
    {

        CancellationTokenSource cts = null;

        public FormAdvancer()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Hide();
        }

        public void AppendData(ColorTransformEventArgs oArgs)
        {
            if (oArgs == null)
            {
                return;
            }
            Invoke(() =>
            {
                if (oArgs.CancellationTokenSource != null)
                {
                    cts = oArgs.CancellationTokenSource;
                }
                if (oArgs.Message != null)
                {
                    listBox1.Items.Add(oArgs.Message);
                }
                if (oArgs?.ProcessingResults?.DataOut != null)
                {
                    pictureBox1.Image = ImageTools.ToBitmap(oArgs.ProcessingResults.DataOut);
                }
            });
        }

        public void Reset()
        {
            Invoke(() =>
            {
                cts = null;
                listBox1.Items.Clear();
                pictureBox1.Image = null;
            });
        }
    }
}