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

        Palette FixedPalette { get; }
        Int32 FixedColors { get; }

        Palette SourcePalette { get; }
        Histogram SourceHistogram { get; }
        Int32 SourceColors { get; }

        Histogram OutputHistogram { get; }
        Palette OutputPalette { get; }
        Int32 OutputColors { get; }
        ColorTransformationMap TransformationMap { get; }

        public bool BypassDithering { get; set; }    

        //------------------------------------------------------------
        ColorTransformType Type { get; }
        String Name { get; }
        String Description { get; }
        ColorDithering DitheringType { get; set; }

        event EventHandler Creating;
        event EventHandler Created;
        event EventHandler<ColorTransformEventArgs> Processing;
        event EventHandler<ColorTransformEventArgs> ProcessAdvance;
        event EventHandler<ColorTransformEventArgs> ProcessPartial;
        event EventHandler<ColorTransformEventArgs> Processed;
        ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue);
        Task<ColorTransformInterface> CreateAsync(int[,]? oDataSource, CancellationToken? oToken );
        Task<ColorTransformInterface> CreateAsync(Histogram? oColorHistogramSource, CancellationToken? oToken);
        Task<ColorTransformInterface> CreateAsync(Palette? oColorPaletteSource, CancellationToken? oToken);
       // ColorTransformInterface Create(int[,]? oDataSource);
       // ColorTransformInterface Create(Histogram? oColorHistogramSource);
       // ColorTransformInterface Create(Palette? oColorPaletteSource);
        Task<ColorTransformResults> ProcessColorsAsync( CancellationToken? oTokenSource);
       // ColorTransformResults ProcessColors(int[,]? oSource );
        Task AbortProcessingAsync(CancellationTokenSource oToken);
        void AbortProcessing(CancellationTokenSource oToken);
    }
}
