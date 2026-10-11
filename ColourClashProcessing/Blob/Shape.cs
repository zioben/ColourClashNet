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
    public class Shape
    {
        static string sC = nameof(Shape);

        /// <summary>Run lengths of the foreground, ordered by (Y, XS).</summary>
        public List<RunLength> RunLengths { get; protected set; } = new List<RunLength>();

        public int SourceWidth { get; protected set; } = 0;
        public int SourceHeight { get; protected set; } = 0;

        public bool Valid => SourceWidth > 0 && SourceHeight > 0;

        public void Reset()
        {
            RunLengths.Clear();
            SourceWidth = 0;
            SourceHeight = 0;
        }

        /// <param name="image">Binary image produced by the threshold stage.</param>
        /// <param name="foregroundColor">
        /// Exact color value that marks foreground pixels. Passing it explicitly (instead of
        /// testing "!= 0") keeps the extraction correct when the threshold is inverted and the
        /// foreground is black, and when the black color carries a non-zero alpha.
        /// </param>
        public ColorTransformResult Process(ImageData image, CancellationToken token = default)
        {
            string sM = nameof(Process);
            try
            {
                Reset();
                ImageData.AssertValid(image);
                var matrix = image.GetMatrix();
                int R = matrix.GetLength(0);
                int C = matrix.GetLength(1);

                // One list per row: threads never share a collection, so no locking is needed
                // and the final concatenation is already ordered by (Y, XS) - no sort required.
                var rows = new List<RunLength>[R];

                // Passing the token through ParallelOptions makes cancellation surface as a plain
                // OperationCanceledException (a throw from inside the body would be wrapped in an
                // AggregateException and would not be caught by the catch below).
                Parallel.For(0, R, new ParallelOptions { CancellationToken = token }, r =>
                {
                    var list = new List<RunLength>();
                    int c = 0;
                    while (c < C)
                    {
                        if (matrix[r, c] == 0)
                        {
                            c++;
                            continue;
                        }

                        int startC = c;
                        while (c < C && matrix[r, c] != 0)
                            c++;

                        list.Add(new RunLength(r, startC, c - 1));
                    }
                    rows[r] = list;
                });

                for (int r = 0; r < R; r++)
                    RunLengths.AddRange(rows[r]);

                SourceWidth = C;
                SourceHeight = R;
                return ColorTransformResult.CreateValidResult(image, null, $"Processed {RunLengths.Count} run lengths");
            }
            catch (OperationCanceledException cex)
            {
                LogMan.Exception(sC, sM, $"Operation canceled", cex);
                return ColorTransformResult.CreateErrorResult(image, null, $"Operation canceled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                throw;
            }
        }

        public ImageData ToImageData()
        {
            string sM = nameof(ToImageData);
            if (!Valid)
            {
                throw new InvalidOperationException($"Shape is not valid: SourceWidth={SourceWidth}, SourceHeight={SourceHeight}");
            }
            return RunLength.ToImageData(RunLengths, SourceWidth, SourceHeight);
        }
    }
}
