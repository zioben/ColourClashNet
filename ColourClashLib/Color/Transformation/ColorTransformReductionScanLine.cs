using ColourClashNet.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color.Dithering;
using System.Globalization;
using ColourClashNet.Color.Tile;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionScanLine : ColorTransformBase
    {
        static string sC = nameof(ColorTransformReductionScanLine);

        public enum ScanlineReductionMode
        {
            IndependentPalettePerLine,
            SharedPaletteForAllLines
        }

        public ColorTransformReductionScanLine()
        {
            Type = ColorTransformType.ColorReductionScanline;
            Description = "Raster line color reduction";
        }

        // -------------------------------------------------------------
        // Ideas:
        // Input image is divided in horizontal chunks of N lines (default 1 line)
        // Without shared palette:
        // Each chunk is processed to reduce the number of colors to a maximum of indipendent MaxColorsPerChunk colors (default 8)
        // With shared palette
        // Image is color reduced to MaxColorsWanted colors (default 16) with a single palette for all chunks 
        // Every chunk is processed to reduce the number of colors to a maximum MaxColorsPerChunk (default 8) with palette selected from the shared palette

        public int ChunkHeight
        {
            get => config.ChunkHeight;
            set => config.ChunkHeight = value;
        }
        public ScanlineReductionMode ReductionModePalette
        {
            get => config.ScanlineReductionMode;
            set => config.ScanlineReductionMode = value;
        }
        public int MaxColorsWanted
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        public int MaxColorsPerChunk
        {
            get => config.MaxColorsPerChunk;
            set => config.MaxColorsPerChunk = value;
        }
        public ColorTransformType InternalTransformationModel 
        { 
            get => config.InternalTransformationModel;
            set => config.InternalTransformationModel = value;
        }
        public bool UseColorMean 
        { 
            get => config.UseColorMean; 
            set => config.UseColorMean = value;
        }

        public List<List<int>> ColorListRow { get; private set; } = new List<List<int>>();
        public List<UInt16> ColorListMask { get; private set; } = new List<UInt16>();

        public ColorTransformReductionScanLine WithProcessingParams(int chunkHeight, ScanlineReductionMode scanlineReductionMode, int maxColorsWanted, int maxColorsPerChunk, ColorTransformType internalTransformationModel, bool useColorMean)
        {
            ChunkHeight=chunkHeight;
            ReductionModePalette = scanlineReductionMode;
            MaxColorsWanted = maxColorsWanted;
            MaxColorsPerChunk = maxColorsPerChunk;
            InternalTransformationModel = internalTransformationModel;
            UseColorMean = useColorMean;
            return this;
        }


        public ColorTransformReductionScanLine WithReductionScanLine(ColorTransformConfig cfg) 
            => WithProcessingParams(cfg.ChunkHeight, cfg.ScanlineReductionMode, cfg.MaxColorsWanted, cfg.MaxColorsPerChunk, cfg.InternalTransformationModel, cfg.UseColorMean);


        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken = default)
        {
            String sM = nameof(ExecuteTransform);
            BypassDithering = true;
            ColorListRow.Clear();

            var oRet = new int[ImageSource.Rows, ImageSource.Columns];
            //var oCols = new int[1, C];
            var oSourceNew = new Imaging.ImageData().Create(ImageSource);

            var oLineFixedPalette = ReferencePalette;
            // Step 1 : Reducing to target palette colors -> 128 to 16 colors 
            // MainPaletteUsed = false;
            switch (ReductionModePalette)
            {
                case ScanlineReductionMode.IndependentPalettePerLine:
                    return ExecuteTransformIndipendentPalette(oToken);
                case ScanlineReductionMode.SharedPaletteForAllLines:
                    return ExecuteTransformSharedPalette(oToken);
                default:
                    throw new NotImplementedException($"{sC}.{sM} : ReductionModePalette {ReductionModePalette} not implemented");
            }
        }

        ColorTransformResult ExecuteTransformIndipendentPalette(CancellationToken oToken = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);
            TileManager tileManager = new TileManager();
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk).WithReferencePalette(new Palette());
            tileManager.Create(ImageSource.Width, ChunkHeight, ImageSource, 1.0, cfg.InternalTransformationModel, cfg, oToken);
            var tileResult = tileManager.ProcessColors(oToken);
            if (tileResult.IsSuccess)
            {
                ImageOutput = TileManager.MergeToImage(new List<TileManager?>() { tileManager });
                return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult(ImageSource, ImageSource, $"{sC}.{sM} : Tile processing error : {tileResult.Message}");
            }
        }

        ColorTransformResult ExecuteTransformSharedPalette(CancellationToken oToken = default)
        {
            string sM = nameof(ExecuteTransformSharedPalette);  
            var oMainImage = ImageSource;
            var oMainPalette = oMainImage.ColorPalette;
            if (oMainPalette.Count > MaxColorsWanted)
            {
                var transf = ColorTransformInternal.Alloc(config).Create(ImageSource);
                var result = transf.ProcessColors(oToken);
                oMainImage = result.DataOut;
            }
            ImageData.AssertValid(oMainImage);
            WithReferencePalette(oMainImage.ColorPalette);
            TileManager tileManager = new TileManager();
            var cfg = config.Clone().WithMaxColorWanted(MaxColorsPerChunk);
            tileManager.Create(oMainImage.Width, ChunkHeight, oMainImage, 1.0, cfg.InternalTransformationModel, cfg, oToken);
            var tileResult = tileManager.ProcessColors(oToken);
            if (tileResult.IsSuccess)
            { 
                ImageOutput = TileManager.MergeToImage(new List<TileManager?>() { tileManager });
                return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult(ImageSource, ImageSource, $"{sC}.{sM} : Tile processing error : {tileResult.Message}");
            }
        }
        //if (UseSharedPalette)
        //{
        //    var oMainHist = new HistogramRGB().Create(ImageSource);
        //    var oMainPalette = oMainHist.ToPalette();
        //    if (oMainPalette.Count <= MaxColorsWanted)
        //    {
        //        oLineFixedPalette = oMainPalette;
        //    }
        //    else
        //    {
        //        //ColorTransformConfig cfg = new ColorTransformConfig()
        //        //{
        //        //    MaxColorsPerChunk = MaxColorsPerChunk,
        //        //    UseColorMean = UseColorMean,
        //        //    InternalTransformationModel = InternalTransformationModel,
        //        ////    DitheringConfig = DitheringConfig.Clone() as DitherConfig,
        //        //};
        //        //ColorTransformInterface oLineTrasf = ColorTransformInternal.Create(InternalTransformationModel);
        //        //if (InternalTransformationMode)
        //        //{
        //        //    var transf2 = new ColorTransformReductionCluster()
        //        //    .WithProcessingParams(MaxColorsPerChunk, 30, UseColorMean)
        //        //    .WithReferencePalette(oLineFixedPalette)
        //        //  //  .WithDithering(DitheringType);
        //        //    oLineTrasf = transf2;
        //         //}
        //        //else
        //        //{
        //        //    var transf2 = new ColorTransformReductionFast()
        //        //    .WithProcessingParams(MaxColorsPerChunk)
        //        //    .WithReferencePalette(oLineFixedPalette)
        //        //    .WithDithering(DitheringType);
        //        //    oLineTrasf = transf2;
        //        //}

        //        //oLineTrasf.Create(ImageSource);

        //        //var oMainRet = oLineTrasf.ProcessColors(oToken);
        //        //oSourceNew = oMainRet.DataOut;
        //        //var oHistNew = new HistogramRGB().Create(ImageSource);
        //        //oLineFixedPalette = oHistNew.ToPalette();
        //    }
        //}
        //oToken.ThrowIfCancellationRequested();


        //    // Select the fixed-most used in histogram

        //    var oRowColors = new List<int>();
        //    //Parallel.For(0, SourceData.Rows, r  =>
        //    Parallel.For(0, ImageSource.Rows, r =>
        //    {
        //        oToken.ThrowIfCancellationRequested();
        //        var oCols = new int[1, ImageSource.Columns];
        //        for (int c = 0; c < ImageSource.Columns; c++)
        //        {
        //            oCols[0, c] = oSourceNew.matrix[r, c];
        //        }
        //        // Create row histogram and take the most used colors
        //        var oHist = new HistogramRGB().Create(new ImageData().Create(oCols));//.SortColorsDescending();
        //        var oNewPal = oHist.SortColorsDescending().ToPalette(MaxColorsPerChunk);
        //        // Create 
        //        //--------------------------------------------------------------
        //        //    Trace.WriteLine($"Row - {r}");
        //        if (DitheringConfig.DitheringType == ColorDithering.None)
        //        {
        //            for (int c = 0; c < ImageSource.Columns; c++)
        //            {
        //                oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
        //            }
        //        }
        //        else
        //        {
        //            var oTras = new ColorTransformReductionPalette()
        //            .WithReferencePalette(oNewPal)
        //            .WithDithering(DitheringConfig)   
        //            .Create(new ImageData().Create(oCols));
        //            var oColRes = oTras.ProcessColors(oToken);
        //            for (int c = 0; c < ImageSource.Columns; c++)
        //            {
        //                oRet[r, c] = oColRes.DataOut.matrix[0, c];
        //            }
        //        }
        //        //--------------------------------------------------------------
        //        // Acceptable
        //        //for (int c = 0; c < C; c++)
        //        //{
        //        //    oRet[r, c] = ColorIntExt.GetNearestColor(oCols[0, c], oNewPal, ColorDistanceEvaluationMode);
        //        //}
        //    });
        //    if (oRet != null)
        //    {
        //        return ColorTransformResult.CreateValidResult(ImageSource, new ImageData().Create(oRet));
        //    }
        //    return new();
        //
        //}
    }
}
