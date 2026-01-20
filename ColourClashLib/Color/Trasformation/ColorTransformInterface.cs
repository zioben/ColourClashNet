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
        ColorTransformInterface Create(ImageData image);
        Task<ColorTransformResults> ProcessColorsAsync(CancellationToken token = default);
        Task<ColorTransformResults> CreateAndProcess(ImageData oDataSource, CancellationToken token = default);
        Task AbortProcessingAsync(CancellationTokenSource tokenSource);
        void AbortProcessing(CancellationTokenSource tokenSource);

        //------------------------------------------------------------

        event EventHandler? Creating;
        event EventHandler? Created;
        event EventHandler<ColorProcessingEventArgs>? Processing;
        event EventHandler<ColorProcessingEventArgs>? ProcessAdvance;
        event EventHandler<ColorProcessingEventArgs>? ProcessPartial;
        event EventHandler<ColorProcessingEventArgs>? Processed;

    }
}
