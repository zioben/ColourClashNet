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
    /// <summary>
    /// Embedded progress view of a running transformation: message history, preview image, percentage and abort button.
    /// </summary>
    public partial class ProcessingViewer : UserControl
    {
        /// <summary>
        /// Creates the viewer.
        /// </summary>
        public ProcessingViewer()
        {
            InitializeComponent();
            Reset();
        }

        CancellationTokenSource? cts = null;

        /// <summary>
        /// Shows the message, preview and progress of a processing event (UI thread safe).
        /// </summary>
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
                if (args.ProcessingResults?.Message != null)
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
                    pbarComplete.Value = (int)Math.Min(pbarComplete.Maximum, Math.Max(pbarComplete.Minimum, args.CompletedPercent));
                }
            });
        }

        void ResetInternal()
        {
            lbHistory.Items.Clear();
            pbarComplete.Value = 0;
            picImageTemp.Image = null;
            lblPerc.Text = "";

        }

        /// <summary>
        /// Clears history, preview and progress.
        /// </summary>
        void Reset()
        {
            if (InvokeRequired)
                Invoke(() => ResetInternal());
            else
                ResetInternal();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Hide();
        }

        /// <summary>
        /// Detaches the viewer from a transformation.
        /// </summary>
        void UnregisterEvents(ColorTransformInterface transform)
        {
            transform.Processing -= transform_Processing;
            transform.ProcessPartial -= transform_ProcessPartial;
            transform.Processed -= transform_Processed;
        }

        /// <summary>
        /// Attaches the viewer to the processing events of a transformation.
        /// </summary>
        public void RegisterEvents(ColorTransformInterface transform)
        {
            UnregisterEvents(transform);
            transform.Processing += transform_Processing;
            transform.ProcessPartial += transform_ProcessPartial;
            transform.Processed += transform_Processed;
        }

        private void transform_Processed(object? sender, ColorProcessingEventArgs e)
        {
            Hide();
        }

        private void transform_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            AppendData(e);
        }

        private void transform_Processing(object? sender, ColorProcessingEventArgs e)
        {
            Show();
            cts = e.CancellationTokenSource;
            AppendData(e);
        }
    }
}
