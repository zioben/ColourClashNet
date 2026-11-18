using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering;

/// <summary>
/// Base class for error diffusion dithering (Floyd-Steinberg, Atkinson, Stucki, ...).
/// <para/>
/// Pixels are scanned left to right, top to bottom. Each pixel is replaced by the nearest palette color
/// and the quantization error is distributed to the not yet processed neighbours, weighted by
/// <see cref="errorDiffusionMatrix"/>. Derived classes only provide the kernel.
/// </summary>
public abstract class DitherErrorDiffusion : DitherBase
{

    static string sC = nameof(DitherErrorDiffusion);

    /// <summary>
    /// Error diffusion kernel. The first row is aligned with the current pixel, which sits in the
    /// middle column; weights on the left of (and including) the current pixel in the first row must be zero.
    /// </summary>
    protected double[,] errorDiffusionMatrix = null;

    /// <summary>
    /// Divides every kernel weight by <paramref name="n"/>.
    /// </summary>
    /// <param name="n">Divider; ignored when zero.</param>
    protected void Normalize(double n)
    {
        if (errorDiffusionMatrix == null)
        {
            return;
        }
        if (n == 0)
        {
            return;
        }
        int rows = errorDiffusionMatrix.GetLength(0);
        int columns = errorDiffusionMatrix.GetLength(1);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                errorDiffusionMatrix[r, c] /= n;
            }
        }
    }

    /// <summary>
    /// Normalizes the kernel so that its weights sum to 1 (the whole error is diffused).
    /// </summary>
    protected void Normalize()
    {
        if (errorDiffusionMatrix == null)
        {
            return;
        }
        var n = 0.0;
        foreach (var d in errorDiffusionMatrix)
        {
            n += d;
        }
        Normalize(n);
    }

    /// <inheritdoc/>
    public override DitherInterface Create() => this;

    /// <summary>
    /// Runs the error diffusion over the whole image.
    /// <para/>
    /// The output palette is the palette of <paramref name="imageProcessed"/>; tagged (negative) pixels
    /// of the processed image are copied unchanged to the output.
    /// </summary>
    protected override ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
    {

        string sM = nameof(DitherImplementation);
        try
        {
            ImageData.AssertValid(imageReference);
            ImageData.AssertValid(imageProcessed);

            Create();

            var dataOriginal = imageReference.matrix;
            var dataProcessed = imageProcessed.matrix;
            var dataProcessedPalette = imageProcessed.ColorPalette;

            LogMan.Trace(sC, sM, $"{Type} : Dithering");

            int rows = dataOriginal.GetLength(0);
            int columns = dataOriginal.GetLength(1);
            int kernelRows = errorDiffusionMatrix.GetLength(0);
            int kernelColumns = errorDiffusionMatrix.GetLength(1);
            int kernelCenter = kernelColumns / 2;

            // Work on per-channel double buffers: the accumulated error must not be clamped or truncated
            var redOriginal = new double[rows, columns];
            var greenOriginal = new double[rows, columns];
            var blueOriginal = new double[rows, columns];
            var redProcessed = new double[rows, columns];
            var greenProcessed = new double[rows, columns];
            var blueProcessed = new double[rows, columns];

            for(int r = 0; r < rows; r++)
            //Parallel.For(0, rows, r =>
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < columns; c++)
                {
                    var originalColor = dataOriginal[r, c];
                    redOriginal[r, c] = originalColor.ToR();
                    greenOriginal[r, c] = originalColor.ToG();
                    blueOriginal[r, c] = originalColor.ToB();
                    var processedColor = dataProcessed[r, c];
                    redProcessed[r, c] = processedColor.ToR();
                    greenProcessed[r, c] = processedColor.ToG();
                    blueProcessed[r, c] = processedColor.ToB();
                }
            }//);

            var dataOut = new int[rows, columns];

            var strength = DitheringStrenght;// / 100.0;

            for (int r = 0; r < rows; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < columns; c++)
                {
                    token.ThrowIfCancellationRequested();

                    // Original color plus the error received so far, clamped to a valid range
                    var oldPixelR = Math.Max(0, Math.Min(255, redOriginal[r, c]));
                    var oldPixelG = Math.Max(0, Math.Min(255, greenOriginal[r, c]));
                    var oldPixelB = Math.Max(0, Math.Min(255, blueOriginal[r, c]));

                    var oldPixel = ColorIntExt.FromRGB(oldPixelR, oldPixelG, oldPixelB);
                    var newPixel = ColorIntExt.GetNearestColor(oldPixel, dataProcessedPalette, colorEvaluationMode);
                    dataOut[r, c] = newPixel;

                    var newPixelR = newPixel.ToR();
                    var newPixelG = newPixel.ToG();
                    var newPixelB = newPixel.ToB();
                    var errorR = oldPixelR - newPixelR;
                    var errorG = oldPixelG - newPixelG;
                    var errorB = oldPixelB - newPixelB;

                    // Spread the error over the neighbours covered by the kernel, centered on the current column
                    for (int rr = 0; rr < kernelRows; rr++)
                    {
                        int rOffset = rr + r;
                        if (rOffset >= rows)
                            break;
                        for (int cc = 0; cc < kernelColumns; cc++)
                        {
                            int colOffset = c + cc - kernelCenter;
                            if (colOffset < 0)
                                continue;
                            //if (c == kernelCenter)
                             //   continue;
                            if (colOffset >= columns)
                                break;
                            redOriginal[rOffset, colOffset] += errorR * errorDiffusionMatrix[rr, cc] * strength;
                            greenOriginal[rOffset, colOffset] += errorG * errorDiffusionMatrix[rr, cc] * strength;
                            blueOriginal[rOffset, colOffset] += errorB * errorDiffusionMatrix[rr, cc] * strength;
                        }
                    }
                }
            }

            // Restore tagged pixels (masks, tiles, ...) from the processed image
            for (int r = 0; r < rows; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < columns; c++)
                {
                    if (dataProcessed[r, c] < 0)
                    {
                        dataOut[r, c] = dataProcessed[r, c];
                    }
                }
            }

            LogMan.Trace(sC, sM, $"{Type} : Dithering completed");
            var imageDithered = new ImageData().Create(dataOut);
            return ColorTransformResult.CreateValidResult( imageReference, imageDithered );
        }
        catch (OperationCanceledException ex)
        {
            LogMan.Exception(sC, sM, $"{Type} : dithering cancelled", ex);
            return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering cancelled", ex);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, $"{Type} : dithering failed", ex);
            return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering failed", ex);
        }
    }
}
