using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public interface ColorTransformInterface
    {
        //------------------------------------------------------------
        ColorTransformType Type { get; }
        String Name { get; }
        String Description { get; }

        //------------------------------------------------------------
        ImageData SourceData { get; }
        ImageData OutputData { get; }

        //------------------------------------------------------------
        Palette FixedPalette { get; }       
        Int32 FixedColors { get; }
        ColorTransformationMap TransformationMap { get; }

        //------------------------------------------------------------
        ColorDithering DitheringType { get; set; }
        public bool BypassDithering { get; set; }

        //------------------------------------------------------------
        ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue);
        ColorTransformInterface Create(ImageData oDataSource);
        Task<ColorTransformResults> ProcessColorsAsync(CancellationToken oToken = default);
        Task<ColorTransformResults> CreateAndProcess(ImageData oDataSource, CancellationToken oToken = default);
        Task AbortProcessingAsync(CancellationTokenSource oTokenSource);
        void AbortProcessing(CancellationTokenSource oTokenSource);

        //------------------------------------------------------------

        event EventHandler Creating;
        event EventHandler Created;
        event EventHandler<ColorProcessingEventArgs> Processing;
        event EventHandler<ColorProcessingEventArgs> ProcessAdvance;
        event EventHandler<ColorProcessingEventArgs> ProcessPartial;
        event EventHandler<ColorProcessingEventArgs> Processed;

    }
}
