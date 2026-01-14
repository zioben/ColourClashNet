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
    public partial class ProcessingForm : Form
    {

        public ColorTransformInterface? Transformation { get; private set; }
        CancellationTokenSource cts = null;

        public ProcessingForm()
        {
            InitializeComponent();
            Load += ((s,e)=>Reset());
            FormClosed += ((s,e) => UnregisterEvents());
        }

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
                    pbarComplete.Value = (int)Math.Min(pbarComplete.Maximum, Math.Max(pbarComplete.Minimum, oArgs.CompletedPercent));
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
            Close();
        }

        void UnregisterEvents()
        {
            if (Transformation != null)
            {
                Transformation.Processing -= OTrasf_Processing;
                Transformation.ProcessPartial -= OTrasf_ProcessPartial;
                Transformation.Processed -= OTrasf_Processed;
            }
            Transformation = null;
        }

        public void RegisterEvents(ColorTransformInterface oTrasf)
        {
            UnregisterEvents();
            Transformation = oTrasf;
            Transformation.Processing += OTrasf_Processing;
            Transformation.ProcessPartial += OTrasf_ProcessPartial;
            Transformation.Processed += OTrasf_Processed;
        }

        private void OTrasf_Processed(object? sender, ColorProcessingEventArgs e)
        {
            //AppendData(e);
            Invoke(()=> Close());
        }

        private void OTrasf_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            AppendData(e);
        }

        private void OTrasf_Processing(object? sender, ColorProcessingEventArgs e)
        {
            Invoke(()=>Show());
            cts = e.CancellationTokenSource;
            AppendData(e);
        }

        static public void CreateProcessingForm(ColorTransformInterface oTrasf)
        {
            var t = new Thread(() =>
            {
                var f = new ProcessingForm();
                f.RegisterEvents(oTrasf);
                Application.Run(f); // crea un message loop dedicato
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}
