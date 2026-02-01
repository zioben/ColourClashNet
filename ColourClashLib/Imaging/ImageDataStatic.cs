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
    /// Gets a value indicating whether the current object contains valid data.
    /// </summary>
    public static void AssertValid(ImageData image)
    {
        string sM = nameof(AssertValid);
        if (image == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(image)} is null");
        if (image.matrix == null)
            throw new ArgumentNullException($"{sC}.{sM} : {nameof(matrix)} is null");
    }

    public static void AssertValidAndDimension(ImageData imageA, ImageData imageB)
    {
        string sM = nameof(AssertValidAndDimension);
        ImageData.AssertValid(imageA);
        ImageData.AssertValid(imageB);
        if (imageA.Rows != imageB.Rows || imageA.Columns != imageB.Columns || imageA.Rows == 0 || imageA.Columns == 0)
            throw new ArgumentNullException($"{sC}.{sM} : Size does not match : {imageA} ; {imageB}");
    }
}
