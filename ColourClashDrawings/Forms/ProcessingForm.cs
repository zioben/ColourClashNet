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
    /// <summary>
    /// Progress window of a running transformation: message history, preview image, percentage and abort button.
    /// </summary>
    public partial class ProcessingForm : Form
    {

        /// <summary>
        /// Transformation being monitored.
        /// </summary>
        public ColorTransformInterface? Transformation { get; private set; }
        CancellationTokenSource cts = null;

        /// <summary>
        /// Creates the form; <paramref name="cancellationTokenSource"/> is cancelled by the abort button.
        /// </summary>
        public ProcessingForm(CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();
            Load += ((s,e)=>Reset());
            FormClosed += ((s,e) => UnregisterEvents());
            cts = cancellationTokenSource;
        }

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
                if (args?.ProcessingResults?.Message != null)
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

        /// <summary>
        /// Clears history, preview and progress.
        /// </summary>
        void Reset()
        {
            try
            {
                Invoke(() =>
                {
                    lbHistory.Items.Clear();
                    pbarComplete.Value = 0;
                    picImageTemp.Image = null;
                    lblPerc.Text = "";
                });
            } catch { }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            Close();
        }

        /// <summary>
        /// Detaches the form from the monitored transformation.
        /// </summary>
        void UnregisterEvents()
        {
            if (Transformation != null)
            {
                Transformation.Processing -= transform_Processing;
                Transformation.ProcessPartial -= transform_ProcessPartial;
                Transformation.Processed -= transform_Processed;
            }
            Transformation = null;
        }

        /// <summary>
        /// Attaches the form to the processing events of a transformation.
        /// </summary>
        public void RegisterEvents(ColorTransformInterface transform)
        {
            UnregisterEvents();
            Transformation = transform;
            Transformation.Processing += transform_Processing;
            Transformation.ProcessPartial += transform_ProcessPartial;
            Transformation.Processed += transform_Processed;
        }

        private void transform_Processed(object? sender, ColorProcessingEventArgs e)
        {
            Invoke(()=> Close());
        }

        private void transform_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            AppendData(e);
        }

        private void transform_Processing(object? sender, ColorProcessingEventArgs e)
        {
            Invoke(()=>Show());
            cts = e.CancellationTokenSource;
            AppendData(e);
        }

        /// <summary>
        /// Opens a progress form for <paramref name="transform"/> on a dedicated UI thread.
        /// <para/>
        /// The form closes automatically when the transformation ends.
        /// </summary>
        static public void CreateProcessingForm(ColorTransformInterface transform, CancellationTokenSource cancellationTokenSource)
        {
            // The form runs on its own STA thread with a dedicated message loop,
            // so it stays responsive while the transformation keeps the caller busy
            var thread = new Thread(() =>
            {
                var form = new ProcessingForm(cancellationTokenSource);
                form.RegisterEvents(transform);
                Application.Run(form);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
