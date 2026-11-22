using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering;

/// <summary>
/// Pass-through ditherer: returns a copy of the processed image unchanged.
/// </summary>
public class DitherIdentity : DitherBase
{
    static string sC = nameof(DitherIdentity);

    /// <summary>
    /// Creates the pass-through ditherer.
    /// </summary>
    public DitherIdentity()
    {
        Type = ColorDithering.None;
        Description = "Passthrough";
    }

    /// <inheritdoc/>
    public override DitherInterface Create()
    {
        return this;
    }

    /// <summary>
    /// Clones <paramref name="imageProcessed"/> without any dithering.
    /// </summary>
    protected override ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
    {
        string sM = nameof(DitherImplementation);
        try
        {
            ImageData.AssertValidAndDimension(imageReference, imageProcessed);
            LogMan.Trace(sC, sM, $"{Type} : Dithering is a simple clone");
            var imageDithered = new ImageData().Create(imageProcessed);
            return ColorTransformResult.CreateValidResult(imageReference, imageDithered);
        }
        catch (OperationCanceledException ex)
        {
            LogMan.Warning(sC, sM, $"{Type} : dithering cancelled");
            return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering cancelled", ex);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, $"{Type} : dithering failed", ex);
            return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering failed", ex);
        }
    }
}
