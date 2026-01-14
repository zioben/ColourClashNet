using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
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
    public partial class ProcessingViewer : UserControl
    {
        public ProcessingViewer()
        {
            InitializeComponent();
            Reset();
        }

        CancellationTokenSource? cts = null;

        void AppendData(ColorProcessingEventArgs oArgs)
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
                if (oArgs.ProcessingResults.Message != null)
                {
                    lbHistory.Items.Insert(0, oArgs.ProcessingResults.Message);
                }
                if (oArgs?.ProcessingResults?.DataOut != null)
                {
                    picImageTemp.Image = ImageTools.ImageDataToGdiImage(oArgs.ProcessingResults.DataOut);
                }
                if (!double.IsNaN(oArgs.CompletedPercent))
                {
                    lblPerc.Text = $"{oArgs.CompletedPercent:f1} %";
                    pbarComplete.Value = (int)Math.Min(0,Math.Max(0,oArgs.CompletedPercent));
                }
            });
        }

        void Reset()
        {
            Invoke(() =>
            {
                lbHistory.Items.Clear();
                pbarComplete.Value = 0;
                picImageTemp.Image = null;
                lblPerc.Text = "";
            });
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Hide();
        }

        void UnregisterEvents(ColorTransformInterface oTrasf)
        {
            oTrasf.Processing -= OTrasf_Processing;
            oTrasf.ProcessPartial -= OTrasf_ProcessPartial;
            oTrasf.Processed -= OTrasf_Processed;
        }

        public void RegisterEvents(ColorTransformInterface oTrasf)
        {
            UnregisterEvents(oTrasf);
            oTrasf.Processing += OTrasf_Processing;
            oTrasf.ProcessPartial += OTrasf_ProcessPartial;
            oTrasf.Processed += OTrasf_Processed;
        }

        private void OTrasf_Processed(object? sender, ColorProcessingEventArgs e)
        {
            //AppendData(e);
            Hide();
        }

        private void OTrasf_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            AppendData(e);
        }

        private void OTrasf_Processing(object? sender, ColorProcessingEventArgs e)
        {
            Show();
            cts = e.CancellationTokenSource;
            AppendData(e);
        }
    }
}
