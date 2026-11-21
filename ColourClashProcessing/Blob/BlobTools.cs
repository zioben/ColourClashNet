using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Export and layout helpers for detected blobs.
    /// </summary>
    public static class BlobTools
    {
        static string sC = nameof(BlobTools);
        /// <summary>
        /// Saves every blob as a separate image file named <c>{prefix}Blob_{id}</c> in <paramref name="path"/>.
        /// </summary>
        /// <returns>True if all images were saved.</returns>
        public static bool ExportImages(Blob blob, string path, string prefix, ImageSaveFormat format, int imageQuality)
        {
            string sM = nameof(ExportImages);
            try
            {
                Directory.CreateDirectory(path);
                var imageList = blob.ToImageList();
                LogMan.Debug(sC, sM, $"exporting {imageList.Count} blob images to {path} as {format}");
                var result = true;
                foreach (var image in imageList)
                {
                    result &= ImageTools.SaveImage(image, Path.Combine(path, $"{prefix}Blob_{Convert.ToInt32(image.Name):D3}"), format, imageQuality);
                }
                if (!result)
                    LogMan.Error(sC, sM, $"some blob images could not be saved to {path}");
                return result;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"blob image export to {path} failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves every blob as a PNG file.
        /// </summary>
        public static bool ExportImages(Blob blob, string path, string prefix)
            => ExportImages(blob, path, prefix, ImageSaveFormat.Png, 80);


        /// <summary>
        /// Packs the blob images into a sprite sheet (not implemented yet).
        /// </summary>
        /// <returns>Always false.</returns>
        public static bool RepackImages(Blob blob, string path, string prefix, ImageSaveFormat format, int imageQuality)
        {
            string sM = nameof(RepackImages);
            LogMan.Error(sC, sM, "not implemented yet");
            return false;
        }

        /// <summary>
        /// Stacks the blob images vertically into a single image, with optional padding.
        /// </summary>
        /// <param name="blob">Detected blobs</param>
        /// <param name="maxWidth">Skips blobs wider than this value (0 = no limit)</param>
        /// <param name="xPaddingL">Left padding</param>
        /// <param name="xPaddingR">Right padding</param>
        /// <param name="yPaddingT">Top padding of each blob</param>
        /// <param name="yPaddingB">Bottom padding of each blob</param>
        /// <returns>The stacked image, or null on error.</returns>
        public static ImageData ReshapeVertical(Blob blob, int maxWidth, int xPaddingL, int xPaddingR, int yPaddingT,int yPaddingB)            
        {
            string sM = nameof(ReshapeVertical);
            try
            {
                if(blob == null) throw new ArgumentNullException(nameof(blob));
                // Get all blobs
                var list = blob.ToImageList();
                if( maxWidth > 0)
                {
                    list = list.Where(x => x != null && x.Width <= maxWidth).ToList();
                }
                var count = list.Count();
                // evaluates the maximum width of the images and the total height of the images
                var maxWidthFound = list.Max(x => x.Width);
                var imageW = maxWidthFound + xPaddingL + xPaddingR;
                var imageH = list.Sum(x => x.Height) + (yPaddingT + yPaddingB) * (count);
                var matrix= MatrixTools.CreateMatrix(imageW, imageH, blob.Options.BackgroundColor);
                // starting point for the first image
                int xStart = xPaddingL;
                int yStart = yPaddingT;
                foreach (var image in list)
                {
                    if (!MatrixTools.Blit(image, matrix, 0, 0, xStart, yStart, image.Width, image.Height))
                    {
                        LogMan.Error(sC, sM, $"Blit failed for image {image.Name} at position ({xStart},{yStart})");
                        return null;
                    }
                    yStart += image.Height + yPaddingT + yPaddingB;
                }
                var outImage = new ImageData().Create(matrix);
                return outImage;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "vertical reshape failed", ex);
                return null;
            }
        }
    }
}