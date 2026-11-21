using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Identity transformation: the output is a copy of the source image (dithering may still apply).
    /// </summary>
    public class ColorTransformIdentity : ColorTransformBase<IdentityOptions>
    {
        static string sC = nameof(ColorTransformIdentity);
        /// <summary>
        /// Creates the identity transformation.
        /// </summary>
        public ColorTransformIdentity()
        {
            Type = ColorTransformType.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        /// <summary>
        /// Copies the source image.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
              string sM = nameof(ExecuteTransform);
                try
                {
                    var result = ColorTransformResult.CreateValidResult(ImageSource, new ImageData().Create(ImageSource));
                    return result;
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sC, sM, $"{Type}", ex);
                    return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : image copy failed", ex);
                }

        }
    }
}
