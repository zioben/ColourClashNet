using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Threading;
using System.Windows.Forms;

namespace ModuleTester
{
    /// <summary>
    /// Progress window of a running transformation: hosts a <see cref="ColourClashNet.Controls.ProcessingViewer"/>
    /// and closes when the processing ends or is aborted.
    /// </summary>
    public partial class ProcessingForm : Form
    {
        static readonly string sC = nameof(ProcessingForm);

        /// <summary>
        /// Maximum wait for the progress window to attach to the transformation, in milliseconds.
        /// </summary>
        const int AttachTimeoutMilliseconds = 5000;

        /// <summary>
        /// Creates the form.
        /// </summary>
        public ProcessingForm()
        {
            InitializeComponent();
            pvProcessing.ProcessingCompleted += (s, e) => Close();
            pvProcessing.AbortRequested += (s, e) => Close();
            FormClosed += (s, e) => pvProcessing.UnregisterEvents();
        }

        /// <summary>
        /// Attaches the form to the processing events of <paramref name="transform"/>.
        /// </summary>
        /// <param name="transform">Transformation to monitor</param>
        /// <param name="cancellationTokenSource">Token source cancelled by the abort button</param>
        public void RegisterEvents(IColorTransform transform, CancellationTokenSource? cancellationTokenSource)
        {
            Text = $"Processing - {transform.Type}";
            pvProcessing.RegisterEvents(transform, cancellationTokenSource);
        }

        /// <summary>
        /// Opens a progress form for <paramref name="transform"/> on a dedicated UI thread.
        /// <para/>
        /// Returns once the form is attached to the transformation, so no processing event is lost when
        /// the caller starts the processing right after. The form closes automatically when the transformation ends.
        /// </summary>
        static public void CreateProcessingForm(IColorTransform transform, CancellationTokenSource cancellationTokenSource)
        {
            string sM = nameof(CreateProcessingForm);
            // Not disposed: the thread may still signal it after a timeout
            var attached = new ManualResetEventSlim(false);
            // The form runs on its own STA thread with a dedicated message loop,
            // so it stays responsive while the transformation keeps the caller busy
            var thread = new Thread(() =>
            {
                try
                {
                    var form = new ProcessingForm();
                    form.RegisterEvents(transform, cancellationTokenSource);
                    attached.Set();
                    Application.Run(form);
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sC, sM, $"{transform.Type} : progress window failed", ex);
                    attached.Set();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            if (!attached.Wait(AttachTimeoutMilliseconds))
                LogMan.Warning(sC, sM, $"{transform.Type} : progress window not ready, the first events may be missing");
        }
    }
}
