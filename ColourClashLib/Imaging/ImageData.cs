using ColourClashNet.Color;
using ColourClashNet.Log;
using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging;


/// <summary>
/// Image stored as a matrix of packed integer colors [rows, columns], with its color palette.
/// <para/>
/// The palette is recomputed every time the matrix is replaced; <see cref="GetMatrix"/> returns a copy,
/// so the image is effectively immutable from the outside.
/// </summary>
public partial class ImageData
{
    static string sC = nameof(ImageData);

    object locker = new object();

    /// <summary>
    /// Gets or sets a descriptive name of the image.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Pixel matrix [row, column]; each value is a packed color (see <see cref="ColorIntExt"/>).
    /// </summary>
    internal protected int[,]? matrix;

    /// <summary>
    /// Gets the set of distinct colors used by the image.
    /// </summary>
    public Palette ColorPalette { get; private set; } = new Palette();


    /// <summary>
    /// Gets the width (number of columns) of the image.
    /// </summary>
    public int Width
    {
        get
        {
            if (matrix == null) return 0;
            return matrix.GetLength(1);
        }
    }

    /// <summary>Number of rows (same as <see cref="Height"/>).</summary>
    public int Rows => Height;

    /// <summary>Number of columns (same as <see cref="Width"/>).</summary>
    public int Columns => Width;

    /// <summary>
    /// Gets the height (number of rows) of the image.
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
    /// Gets the number of distinct colors in the image.
    /// </summary>
    public int Colors => ColorPalette.Count;

    /// <summary>
    /// Returns a copy of the pixel matrix.
    /// </summary>
    /// <returns>A clone of the matrix; changes do not affect this image.</returns>
    /// <exception cref="ArgumentNullException">The image has no data.</exception>
    public int[,] GetMatrix()
    {
        AssertValid(this);
        return matrix.Clone() as int[,];
    }


    /// <summary>
    /// Clears the pixel data and the palette (thread-safe).
    /// </summary>
    /// <returns>This instance.</returns>
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
    /// Initializes the image with a copy of <paramref name="matrixSrc"/> and computes its palette.
    /// </summary>
    /// <param name="matrixSrc">Pixel matrix [rows, columns]</param>
    /// <returns>This instance.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="matrixSrc"/> is null.</exception>
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
    /// Initializes a filled color image of the given size.
    /// </summary>
    /// <param name="width">Number of columns</param>
    /// <param name="height">Number of rows</param>
    /// <param name="colorRGB">The RGB color value for all pixels</param>
    /// <returns>This instance.</returns>
    public ImageData Create(int width, int height, int colorRGB )
    {
        lock (locker)
        {
            Reset();
            matrix = MatrixTools.CreateMatrix(height, width, colorRGB);
            ColorPalette = new Palette().Create(this);
        }
        return this;
    }
    /// <summary>
    /// Initializes a black (all zero) image of the given size.
    /// </summary>
    /// <param name="width">Number of columns</param>
    /// <param name="height">Number of rows</param>
    /// <returns>This instance.</returns>
    public ImageData Create(int width, int height)
        => Create(width, height, 0);

    /// <summary>
    /// Copies a rectangular area into a new image. Parts outside this image are left black.
    /// </summary>
    /// <param name="xStart">Left coordinate</param>
    /// <param name="yStart">Top coordinate</param>
    /// <param name="width">Area width</param>
    /// <param name="height">Area height</param>
    /// <returns>A new image of <paramref name="width"/> x <paramref name="height"/>.</returns>
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
    /// Initializes the image as a deep copy of <paramref name="imageSrc"/> (the name is not copied).
    /// </summary>
    /// <param name="imageSrc">Source image</param>
    /// <returns>This instance.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="imageSrc"/> or its matrix is null.</exception>
    public ImageData Create(ImageData imageSrc)
    {
        string sM = nameof (Create);
        if (imageSrc == null)
            throw new InvalidOperationException($"{sC}.{sM} : {nameof(imageSrc)} is null");
        Create(imageSrc.matrix);   
        return this;
    }


    /// <summary>
    /// Returns the image name, size and number of colors.
    /// </summary>
    public override string ToString()
    {
        return $"ImageData: {Name} (Size: {Width}x{Height}, Colors: {Colors} )";
    }
}
