using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace ColourClashProcessing.Blob
{
    public static class BlobTools
    {
        static string sC = nameof(BlobTools);
        public static bool ExportImages(Blob blob, string sPath, string sPrefix, ImageSaveFormat format)
        {
            string sM = nameof(ExportImages);
            try
            {
                Directory.CreateDirectory(sPath);
                var oImageOut = blob.GetShapeImage();
                bool res = true;
                foreach (var blobItem in blob.Blobs)
                {
                    var oImageBlob = blob.GetBlobImage(blobItem.BlobId);
                    res &= ImageTools.SaveImage(oImageBlob, $"{sPath}\\{sPrefix}Blob_{blobItem.BlobId}", format);
                }
                return res;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC,sM, ex);
                return false;
            };
        }
    }
}
