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

        ImageData ImageReference { get; }   
        ImageData ImageSource { get; }
        ImageData ImageOutput { get; }

        //------------------------------------------------------------
        Palette ReferencePalette { get; }       
        Int32 ReferenceColors { get; }
        ColorTransformationMap TransformationMap { get; }

        //------------------------------------------------------------
        ColorDithering DitheringType { get; set; }
        public bool BypassDithering { get; set; }
        public double ProcessingTimeMilliseconds { get; }

        //------------------------------------------------------------

        //------------------------------------------------------------

        ColorTransformInterface SetProperties(ColorTransformConfig cfg);

        //------------------------------------------------------------

        ColorTransformInterface Create(ImageData sourceImage, ImageData referenceImage);
        ColorTransformInterface Create(ImageData sourceImage);

        ColorTransformResult ProcessColors(CancellationToken token = default);
        ColorTransformResult CreateAndProcessColors(ImageData sourceImage, ImageData referenceImage,  CancellationToken token = default);
        ColorTransformResult CreateAndProcessColors(ImageData sourceImage, CancellationToken token = default);
        void AbortProcessing(CancellationTokenSource token);

        Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData image, ImageData referenceImage = null, CancellationToken token = default);
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
