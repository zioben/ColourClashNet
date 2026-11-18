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

public abstract class DitherErrorDiffusion : DitherBase
{

    static string sC = nameof(DitherErrorDiffusion);

    protected double[,] errorDiffusionMatrix = null;


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

    public override DitherInterface Create() => this;

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
        catch (TaskCanceledException et)
        {
            LogMan.Exception(sC, sM, $"{Type}", et);
            return ColorTransformResult.CreateErrorResult(et);
        }
        catch (OperationCanceledException ex)
        {
            LogMan.Exception(sC, sM, $"{Type}", ex);
            return ColorTransformResult.CreateErrorResult(ex);
        }
    }
}
