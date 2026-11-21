using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Replaces every color of <see cref="BackgroundRemovalOptions.BackgroundColors"/> with
    /// <see cref="BackgroundRemovalOptions.ReplacementColor"/>; other colors are kept.
    /// </summary>
    public class ColorTransformBkgRemover : ColorTransformBase<BackgroundRemovalOptions>
    {
        static string sC = nameof(ColorTransformBkgRemover);
        /// <summary>
        /// Creates the background remover.
        /// </summary>
        public ColorTransformBkgRemover()
        {
            Type = ColorTransformType.ColorRemover;
            Description = "Basic Background Color Replacement";
        }






        /// <summary>
        /// Builds an identity map with the background colors redirected, then applies it.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
           
                string sM = nameof(ExecuteTransform);
                LogDebug(sC, sM, $"{Type} : replacing {Options.BackgroundColors?.Count ?? 0} background colors with {Options.ReplacementColor:X6}");

                TransformationMap.Reset();
                var bkgList = Options.BackgroundColors ?? Array.Empty<int>();
                var sourceList = ImageSource.ColorPalette.ToList();
                foreach (var rgb in sourceList)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                foreach (var rgb in bkgList)
                {
                    TransformationMap.Remove(rgb);
                    TransformationMap.Add(rgb, Options.ReplacementColor);
                }
            return base.ExecuteTransform(token);
        }
    }
}
