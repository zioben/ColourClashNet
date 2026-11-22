using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Concurrent;
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
    /// Progress view of a running transformation: original image, latest processed preview, message history,
    /// percentage and abort button.
    /// <para/>
    /// The processing events arrive on worker threads: the handlers only enqueue them, and a UI timer
    /// (every <see cref="RefreshIntervalMilliseconds"/>) drains the queue and updates the controls.
    /// The transformation never waits for the UI, and only the latest preview of each batch is converted to a bitmap.
    /// </summary>
    public partial class ProcessingViewer : UserControl
    {
        static readonly string sC = nameof(ProcessingViewer);

        /// <summary>
        /// Interval of the UI refresh, in milliseconds.
        /// </summary>
        public const int RefreshIntervalMilliseconds = 500;

        /// <summary>
        /// Maximum number of messages kept in the history (the oldest are removed).
        /// </summary>
        public const int MaxHistoryItems = 2000;

        /// <summary>
        /// Raised on the UI thread when the monitored transformation has ended and the last messages are shown.
        /// </summary>
        public event EventHandler? ProcessingCompleted;

        /// <summary>
        /// Raised on the UI thread when the user presses the abort button (the processing is already cancelled).
        /// </summary>
        public event EventHandler? AbortRequested;

        /// <summary>
        /// One processing event, as captured on the worker thread.
        /// </summary>
        sealed class ProgressItem
        {
            public DateTime Time { get; init; }
            public string? Message { get; init; }
            public ImageData? Preview { get; init; }
            public ImageData? Source { get; init; }
            public double Percent { get; init; } = double.NaN;
            public bool Completed { get; init; }
        }

        readonly ConcurrentQueue<ProgressItem> queue = new();
        readonly System.Windows.Forms.Timer refreshTimer = new() { Interval = RefreshIntervalMilliseconds };
        ColorTransformInterface? transformation;
        CancellationTokenSource? cts;
        volatile bool completedNotified;

        /// <summary>
        /// Creates the viewer.
        /// </summary>
        public ProcessingViewer()
        {
            InitializeComponent();
            picImageSource.SizeMode = PictureBoxSizeMode.Zoom;
            picImageTemp.SizeMode = PictureBoxSizeMode.Zoom;
            refreshTimer.Tick += (s, e) => DrainQueue();
            Disposed += (s, e) =>
            {
                refreshTimer.Dispose();
                UnregisterEvents();
            };
            ResetControls();
        }

        /// <summary>
        /// Attaches the viewer to the processing events of <paramref name="transform"/> and starts the UI refresh.
        /// <para/>
        /// Can be called from any thread: the events are queued until the timer shows them.
        /// </summary>
        /// <param name="transform">Transformation to monitor</param>
        /// <param name="cancellationTokenSource">Token source cancelled by the abort button (optional: the processing events also provide it)</param>
        public void RegisterEvents(ColorTransformInterface transform, CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(transform);
            UnregisterEvents();
            queue.Clear();
            completedNotified = false;
            transformation = transform;
            cts = cancellationTokenSource;
            transform.Processing += transform_Processing;
            transform.ProcessPartial += transform_ProcessPartial;
            transform.Processed += transform_Processed;
            RunOnUiThread(() =>
            {
                ResetControls();
                refreshTimer.Start();
            });
            LogMan.Trace(sC, nameof(RegisterEvents), $"monitoring {transform.Type}");
        }

        /// <summary>
        /// Detaches the viewer from the monitored transformation.
        /// </summary>
        public void UnregisterEvents()
        {
            var transform = transformation;
            if (transform != null)
            {
                transform.Processing -= transform_Processing;
                transform.ProcessPartial -= transform_ProcessPartial;
                transform.Processed -= transform_Processed;
            }
            transformation = null;
        }

        // ---------------------------------------------------------------- Worker thread side: enqueue only

        private void transform_Processing(object? sender, ColorProcessingEventArgs e)
        {
            if (e.CancellationTokenSource != null)
                cts = e.CancellationTokenSource;
            queue.Enqueue(new ProgressItem
            {
                Time = DateTime.Now,
                Message = $"{e.ColorTransformInterface?.Type} : processing started",
                Source = e.ColorTransformInterface?.ImageSource,
                Percent = 0,
            });
        }

        private void transform_ProcessPartial(object? sender, ColorProcessingEventArgs e)
        {
            var message = e.ProcessingResults?.Message;
            queue.Enqueue(new ProgressItem
            {
                Time = DateTime.Now,
                Message = string.IsNullOrEmpty(message) ? null : $"{e.ColorTransformInterface?.Type} : {message}",
                Preview = e.ProcessingResults?.DataOut,
                Percent = e.CompletedPercent,
            });
        }

        private void transform_Processed(object? sender, ColorProcessingEventArgs e)
        {
            var result = e.ProcessingResults;
            queue.Enqueue(new ProgressItem
            {
                Time = DateTime.Now,
                Message = result == null
                    ? $"{e.ColorTransformInterface?.Type} : processing ended"
                    : result.IsSuccess
                        ? $"{e.ColorTransformInterface?.Type} : processing completed"
                        : $"{e.ColorTransformInterface?.Type} : processing failed : {result.Message}",
                Preview = result?.DataOut,
                Percent = result?.IsSuccess == true ? 100 : double.NaN,
                Completed = true,
            });
        }

        // ---------------------------------------------------------------- UI thread side

        /// <summary>
        /// Shows every queued event: messages are all added, the images and the percentage only from the latest event.
        /// </summary>
        void DrainQueue()
        {
            if (queue.IsEmpty)
                return;
            string sM = nameof(DrainQueue);
            try
            {
                var messages = new List<string>();
                ImageData? preview = null;
                ImageData? source = null;
                double percent = double.NaN;
                bool completed = false;
                while (queue.TryDequeue(out var item))
                {
                    if (item.Message != null)
                        messages.Add($"{item.Time:HH:mm:ss.f}  {item.Message}");
                    preview = item.Preview ?? preview;
                    source = item.Source ?? source;
                    if (!double.IsNaN(item.Percent))
                        percent = item.Percent;
                    completed |= item.Completed;
                }

                if (messages.Count > 0)
                {
                    lbHistory.BeginUpdate();
                    // Newest message on top
                    foreach (var message in messages)
                        lbHistory.Items.Insert(0, message);
                    while (lbHistory.Items.Count > MaxHistoryItems)
                        lbHistory.Items.RemoveAt(lbHistory.Items.Count - 1);
                    lbHistory.EndUpdate();
                }
                if (source != null)
                    ReplaceImage(picImageSource, ImageToolsGDI.ImageDataToGdiImage(source));
                if (preview != null)
                    ReplaceImage(picImageTemp, ImageToolsGDI.ImageDataToGdiImage(preview));
                if (!double.IsNaN(percent))
                {
                    lblPerc.Text = $"{percent:f1} %";
                    pbarComplete.Value = (int)Math.Clamp(percent, pbarComplete.Minimum, pbarComplete.Maximum);
                }
                if (completed && !completedNotified)
                {
                    completedNotified = true;
                    refreshTimer.Stop();
                    UnregisterEvents();
                    ProcessingCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "cannot update the progress view", ex);
            }
        }

        /// <summary>
        /// Shows <paramref name="image"/> and disposes the previous one.
        /// </summary>
        static void ReplaceImage(PictureBox pictureBox, Image? image)
        {
            var old = pictureBox.Image;
            pictureBox.Image = image;
            old?.Dispose();
        }

        /// <summary>
        /// Clears images, history and progress.
        /// </summary>
        void ResetControls()
        {
            lbHistory.Items.Clear();
            pbarComplete.Value = 0;
            ReplaceImage(picImageTemp, null);
            ReplaceImage(picImageSource, null);
            lblPerc.Text = "";
        }

        /// <summary>
        /// Runs <paramref name="action"/> on the UI thread (directly when already on it or when the handle does not exist yet).
        /// </summary>
        void RunOnUiThread(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            LogMan.Warning(sC, nameof(btnAbort_Click), $"{transformation?.Type} : abort requested by the user");
            cts?.Cancel();
            AbortRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
