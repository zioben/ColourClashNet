using ColourClashNet.Color;
using ColourClashNet.Defaults;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging;


public partial class ImageData
{
    static string sC = nameof(ImageData);

    object locker = new object();

    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets the two-dimensional array of integer data associated with this instance.
    /// </summary>
    internal protected int[,]? matrix;

    /// <summary>
    /// Gets the color palette used for rendering visual elements.
    /// </summary>
    public Palette ColorPalette { get; private set; } = new Palette();


    /// <summary>
    /// Gets the Width (number of columns) in the data set as image.
    /// </summary>
    public int Width
    {
        get
        {
            if (matrix == null) return 0;
            return matrix.GetLength(1);
        }
    }

    public int Rows => Height;
    public int Columns => Width;

    /// <summary>
    /// gets the Height (number of rows) in the data set as image.
    /// </summary>
    public int Height
    {
        get
        {
            if (matrix == null) return 0;
            return matrix.GetLength(0);
        }
    }

    /// <summary>
    /// Gets the total number of pixels in the image.
    /// </summary>
    public int PixelCount
    {
        get
        {
            return Width * Height;
        }
    }

    /// <summary>
    /// Gets the number of colors in the current color palette.
    /// </summary>
    public int Colors => ColorPalette.Count;


   


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public int[,] GetMatrix()
    {
        AssertValid(this);
        return matrix.Clone() as int[,];
    }


    /// <summary>
    /// Resets the object's data and color palette to their default states.
    /// </summary>
    /// <remarks>This method is thread-safe. If an error occurs during the reset process, the method
    /// logs the exception and returns false.</remarks>
    /// <returns>The current <see cref="ImageData"/> instance with updated data and color palette if <paramref name="oData"/>
    /// is not null; otherwise, the instance remains unchanged.</returns>
    public ImageData Reset()
    {
        string sM = nameof(Reset);
        lock (locker)
        {
            matrix = null;
            ColorPalette = new Palette();
        }
        return this;
    }


   /// <summary>
   /// Initializes the image data and color palette using the specified two-dimensional array of pixel values.
   /// </summary>
   /// <param name="matrixSrc">A two-dimensional array of integers representing pixel data. If null, the existing image data is not
   /// modified.</param>
   /// <returns>The current <see cref="ImageData"/> instance with updated data and color palette if <paramref name="matrixSrc"/>
   /// is not null; otherwise, the instance remains unchanged.</returns>
    public ImageData Create(int[,] matrixSrc)
    {
        string sM = nameof(Create);
        lock (locker)
        {
            Reset();
            if (matrixSrc == null)
                throw new InvalidOperationException($"{sC}.{sM} : {nameof(matrixSrc)} is null" );
            this.matrix = matrixSrc.Clone() as int[,];
            ColorPalette = new Palette().Create(this);
        }
        return this;
    }


    /// <summary>
    /// Initializes the image data with the specified width and height, resetting any existing data.
    /// </summary>
    /// <remarks>This method resets any existing image data before initializing the new data. The
    /// color palette is also regenerated based on the new dimensions. This method is not thread-safe; external
    /// synchronization is required if accessed concurrently.</remarks>
    /// <param name="width">The number of columns for the image data. Must be greater than zero.</param>
    /// <param name="height">The number of rows for the image data. Must be greater than zero.</param>
    /// <returns>The current instance of <see cref="ImageData"/> with the data initialized to the specified dimensions.</returns>
    public ImageData Create(int width, int height )
    {
        lock (locker)
        {
            Reset();
            matrix = new int[height, width]; 
            ColorPalette = new Palette().Create(this);
        }
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public ImageData Extract(int xStart, int yStart, int width, int height )
    {
        string sM = nameof(Extract);
        lock (locker)
        {
            AssertValid(this);
            var matrixDst = new int[height, width];
            MatrixTools.Blit(matrix, matrixDst, xStart, yStart, 0, 0, width, height);
            return new ImageData().Create(matrixDst);
        }
    }


    /// <summary>
    /// Creates a new image based on the data contained in the specified <see cref="ImageData"/> instance.
    /// </summary>
    /// <param name="imageSrc">An <see cref="ImageData"/> object containing the source data for the new image. Can be null.</param>
    /// <returns>An <see cref="ImageData"/> instance representing the newly created image, or null if <paramref
    /// name="imageSrc"/> is null or contains no data.</returns>
    public ImageData Create(ImageData imageSrc)
    {
        string sM = nameof (Create);
        if (imageSrc == null)
            throw new InvalidOperationException($"{sC}.{sM} : {nameof(imageSrc)} is null");
        Create(imageSrc.matrix);   
        return this;
    }


    /// <summary>
    /// Returns a string that represents the current image, including its name, dimensions, and color count.
    /// </summary>
    /// <returns>A string containing the image name, width, height, and number of colors in the format: "ImageData: {Name},
    /// {Width}x{Height}, Colors: {Colors}".</returns>
    public override string ToString()
    {
        return $"ImageData: {Name} (Size: {Width}x{Height}, Colors: {Colors} )";
    }
}
