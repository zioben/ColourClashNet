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

        void AppendData(ColorProcessingEventArgs args)
        {
            if (args == null)
            {
                return;
            }
            Invoke(() =>
            {
                if (args.CancellationTokenSource != null)
                {
                    cts = args.CancellationTokenSource;
                }
                if (args.ProcessingResults.Message != null)
                {
                    lbHistory.Items.Insert(0, args.ProcessingResults.Message);
                }
                if (args?.ProcessingResults?.DataOut != null)
                {
                    picImageTemp.Image = ImageToolsGDI.ImageDataToGdiImage(args.ProcessingResults.DataOut);
                }
                if (!double.IsNaN(args.CompletedPercent))
                {
                    lblPerc.Text = $"{args.CompletedPercent:f1} %";
                    pbarComplete.Value = (int)Math.Min(0,Math.Max(0,args.CompletedPercent));
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

        void UnregisterEvents(ColorTransformInterface transf)
        {
            transf.Processing -= transf_Processing;
            transf.ProcessPartial -= transf_ProcessPartial;
            transf.Processed -= transf_Processed;
        }

        public void RegisterEvents(ColorTransformInterface transf)
        {
            UnregisterEvents(transf);
            transf.Processing += transf_Processing;
            transf.ProcessPartial += transf_ProcessPartial;
            transf.Processed += transf_Processed;
        }

        private void transf_Processed(object? sender, ColorProcessingEventArgs e)
        {
            //AppendData(e);
            Hide();
        }

        private void transf_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            AppendData(e);
        }

        private void transf_Processing(object? sender, ColorProcessingEventArgs e)
        {
            Show();
            cts = e.CancellationTokenSource;
            AppendData(e);
        }
    }
}
