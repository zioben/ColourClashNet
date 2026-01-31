using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
        Palette PriorityPalette { get; }       
        Int32 PriorityColors { get; }
        ColorTransformationMap TransformationMap { get; }

        //------------------------------------------------------------
        ColorDithering DitheringType { get; set; }
        public bool BypassDithering { get; set; }
        public double ProcessingTimeMilliseconds { get; }

        //------------------------------------------------------------

        //------------------------------------------------------------
        //ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue);

        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, int value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, double value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Palette value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, List<int> value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, IEnumerable<int> value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ColorDistanceEvaluationMode value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, string value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, decimal value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ColorDithering value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Boolean value);
        ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Enum value);

        //------------------------------------------------------------

        ColorTransformInterface Create(ImageData image);
      
        ColorTransformResult ProcessColors(CancellationToken token = default);
        ColorTransformResult CreateAndProcessColors(ImageData image, CancellationToken token = default);
        void AbortProcessing(CancellationTokenSource token);



       // Task<ColorTransformInterface> CreateAsync(ImageData image);
       // Task<ColorTransformResults> ProcessColorsAsync(CancellationToken token = default);
        Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData image, CancellationToken token = default);
        Task AbortProcessingAsync(CancellationTokenSource token);


        ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; }
        double TransformationError { get; }

        //------------------------------------------------------------

        event EventHandler? Creating;
        event EventHandler? Created;
        event EventHandler<ColorProcessingEventArgs>? Processing;
        event EventHandler<ColorProcessingEventArgs>? ProcessAdvance;
        event EventHandler<ColorProcessingEventArgs>? ProcessPartial;
        event EventHandler<ColorProcessingEventArgs>? Processed;

    }
}
