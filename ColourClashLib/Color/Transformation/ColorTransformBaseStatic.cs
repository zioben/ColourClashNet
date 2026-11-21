using ColourClashNet.Color.Transformation;
using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        /// <summary>
        /// Throws if <paramref name="transform"/> is null.
        /// </summary>
        public static void AssertValid(ColorTransformInterface transform)
        {
            if (transform == null)
                throw new ArgumentNullException($"{nameof(transform)}");
        }

        /// <summary>
        /// Factory: creates the transformation that matches the type of <paramref name="options"/> and assigns them.
        /// </summary>
        /// <param name="options">Options; their record type selects the algorithm</param>
        /// <returns>The configured transformation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported options type.</exception>
        public static ColorTransformInterface CreateColorTransformInterface(TransformOptions options)
        {
            string sMethod = nameof(CreateColorTransformInterface);
            ArgumentNullException.ThrowIfNull(options);
            return options switch
            {
                IdentityOptions o => new ColorTransformIdentity { Options = o },
                BackgroundRemovalOptions o => new ColorTransformBkgRemover { Options = o },
                QuantizationOptions o => new ColorTransformQuantization { Options = o },
                FastOptions o => new ColorTransformFast { Options = o },
                MedianCutOptions o => new ColorTransformMedianCut { Options = o },
                ClusteringOptions o => new ColorTransformClustering { Options = o },
                FixedPaletteOptions o => new ColorTransformFixedPalette { Options = o },
                EnhancedPaletteOptions o => new ColorTransformFixedPaletteEnhanced { Options = o },
                HsvOptions o => new ColorTransformLumSat { Options = o },
                ScanlineOptions o => new ColorTransformScanLine { Options = o },
                HerculesOptions o => new ColorTransformHercules { Options = o },
                CgaOptions o => new ColorTransformCGA { Options = o },
                EgaOptions o => new ColorTransformEGA { Options = o },
                VgaOptions o => new ColorTransformVGA { Options = o },
                C64Options o => new ColorTransformCbm64 { Options = o },
                AmigaOptions o => new ColorTransformCbmAmiga { Options = o },
                CpcOptions o => new ColorTransformAmstradCPC { Options = o },
                ZxOptions o => new ColorTransformZxSpectrum { Options = o },
                _ => throw new ArgumentOutOfRangeException(sMethod, $"Options {options.GetType().Name} not supported"),
            };
        }

        /// <summary>
        /// Factory: creates the transformation <paramref name="transformType"/> with its default options.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported transformation type.</exception>
        public static ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType)
            => CreateColorTransformInterface(TransformOptions.CreateDefault(transformType));
    }
}
