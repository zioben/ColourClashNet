using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Imaging;

public partial class ImageData
{
    /// <summary>
    /// Throws if the image is null or has no pixel data.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AssertValid(ImageData image)
    {
        string sM = nameof(AssertValid);
        if (image == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(image)} is null");
        if (image.matrix == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrix)} is null");
    }

    /// <summary>
    /// Throws if either image is invalid, empty, or the two images have different sizes.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AssertValidAndDimension(ImageData imageA, ImageData imageB)
    {
        string sM = nameof(AssertValidAndDimension);
        ImageData.AssertValid(imageA);
        ImageData.AssertValid(imageB);
        if (imageA.Rows != imageB.Rows || imageA.Columns != imageB.Columns || imageA.Rows == 0 || imageA.Columns == 0)
            throw new ArgumentNullException($"{sC}.{sM} : Size does not match : {imageA} ; {imageB}");
    }
}
