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
        ImageData InputData { get; }
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
        Task<ColorTransformInterface> CreateAsync(ImageData? oDataSource, CancellationToken? oToken);
        Task<ColorTransformInterface> CreateAsync(int[,]? oDataSource, CancellationToken? oToken);
        
        Task<ColorTransformInterface> CreateAsync(Histogram? oColorHistogramSource, CancellationToken? oToken);
        
        //Task<ColorTransformInterface> CreateAsync(Palette? oColorPaletteSource, CancellationToken? oToken);
        Task<ColorTransformResults> ProcessColorsAsync(CancellationToken? oTokenSource);
        Task AbortProcessingAsync(CancellationTokenSource oToken);
        void AbortProcessing(CancellationTokenSource oToken);

        //------------------------------------------------------------

        event EventHandler Creating;
        event EventHandler Created;
        event EventHandler<ColorTransformEventArgs> Processing;
        event EventHandler<ColorTransformEventArgs> ProcessAdvance;
        event EventHandler<ColorTransformEventArgs> ProcessPartial;
        event EventHandler<ColorTransformEventArgs> Processed;

    }
}
