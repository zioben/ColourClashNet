using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Binary shape stored as horizontal run lengths of foreground pixels.
    /// </summary>
    public class Shape
    {
        static string sC = nameof(Shape);

        /// <summary>Run lengths of the foreground, ordered by (Y, XS).</summary>
        public List<RunLength> RunLengths { get; protected set; } = new List<RunLength>();

        /// <summary>
        /// Width of the image the shape was extracted from.
        /// </summary>
        public int SourceWidth { get; protected set; } = 0;
        /// <summary>
        /// Height of the image the shape was extracted from.
        /// </summary>
        public int SourceHeight { get; protected set; } = 0;

        /// <summary>
        /// True after a successful <see cref="Process"/>.
        /// </summary>
        public bool Valid => SourceWidth > 0 && SourceHeight > 0;

        /// <summary>
        /// Clears the runs and the source size.
        /// </summary>
        public void Reset()
        {
            RunLengths.Clear();
            SourceWidth = 0;
            SourceHeight = 0;
        }

        /// <summary>
        /// Extracts the horizontal runs of non-zero (foreground) pixels from a binary image.
        /// <para/>
        /// Pixels equal to 0 (black) are background; any other value is foreground.
        /// </summary>
        /// <param name="image">Binary image produced by the threshold stage.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A valid result, or an error result.</returns>
        public ColorTransformResult Process(ImageData image, CancellationToken token = default)
        {
            string sM = nameof(Process);
            try
            {
                Reset();
                ImageData.AssertValid(image);
                var matrix = image.GetMatrix();
                int rowCount = matrix.GetLength(0);
                int columns = matrix.GetLength(1);

                // One list per row: threads never share a collection, so no locking is needed
                // and the final concatenation is already ordered by (Y, XS) - no sort required.
                var rowRuns = new List<RunLength>[rowCount];

                // Passing the token through ParallelOptions makes cancellation surface as a plain
                // OperationCanceledException (a throw from inside the body would be wrapped in an
                // AggregateException and would not be caught by the catch below).
                Parallel.For(0, rowCount, new ParallelOptions { CancellationToken = token }, r =>
                {
                    var list = new List<RunLength>();
                    int c = 0;
                    while (c < columns)
                    {
                        if (matrix[r, c] == 0)
                        {
                            c++;
                            continue;
                        }

                        int startC = c;
                        while (c < columns && matrix[r, c] != 0)
                            c++;

                        list.Add(new RunLength(r, startC, c - 1));
                    }
                    rowRuns[r] = list;
                });

                for (int r = 0; r < rowCount; r++)
                    RunLengths.AddRange(rowRuns[r]);

                SourceWidth = columns;
                SourceHeight = rowCount;
                return ColorTransformResult.CreateValidResult(image, null, $"Processed {RunLengths.Count} run lengths");
            }
            catch (OperationCanceledException cex)
            {
                LogMan.Warning(sC, sM, $"Operation cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, "run length extraction cancelled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, "run length extraction failed", ex);
            }
        }

        /// <summary>
        /// Draws the shape on an image of the source size.
        /// </summary>
        /// <exception cref="InvalidOperationException">The shape is not valid.</exception>
        public ImageData ToImageData()
        {
            string sM = nameof(ToImageData);
            if (!Valid)
            {
                throw new InvalidOperationException($"Shape is not valid: SourceWidth={SourceWidth}, SourceHeight={SourceHeight}");
            }
            return RunLength.ToShapeImageData(RunLengths, SourceWidth, SourceHeight);
        }
    }
}
