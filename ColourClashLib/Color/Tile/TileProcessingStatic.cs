using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileProcessing
{
    /// <summary>
    /// Throws if the tile processing is null or its tile is invalid.
    /// </summary>
    public static void AssertValid(TileProcessing tileProcessing)
    {
        if (tileProcessing == null)
            throw new ArgumentNullException($"{nameof(tileProcessing) is null}");
        TileItem.AssertValid(tileProcessing.tileItem);
    }
  
    /// <summary>
    /// Copies the better of two tiles (lower transformation error) into the destination matrix.
    /// <para/>
    /// If one tile is null, the other one is copied.
    /// </summary>
    /// <param name="destinationData">Destination matrix</param>
    /// <param name="tileA">First candidate</param>
    /// <param name="tileB">Second candidate</param>
    /// <returns>True if a tile was copied.</returns>
    public static bool MergeData(int[,] destinationData, TileProcessing tileA, TileProcessing tileB)
    {
        if (destinationData == null)
            throw new ArgumentNullException(nameof(destinationData));
        if (tileA == null && tileB == null)
            throw new ArgumentNullException($"{nameof(tileA)} or {nameof(tileB)}");
        if (tileA != null && tileB == null)
        {
            return tileA.MergeData(destinationData);
        }
        if (tileA == null && tileB != null)
        {
            return tileB.MergeData(destinationData);
        }
        double errorA = tileA.TransformationError;
        double errorB = tileB.TransformationError;
        if (errorA <= errorB)
        {
            return tileA.MergeData(destinationData);
        }
        else
        {
            return tileB.MergeData(destinationData);
        }
    }

    /// <summary>
    /// Copies the tile with the lowest transformation error into the destination matrix.
    /// </summary>
    /// <param name="destinationData">Destination matrix</param>
    /// <param name="tiles">Candidate tiles covering the same area</param>
    /// <returns>True if a tile was copied.</returns>
    public static bool MergeData(int[,]? destinationData, IEnumerable<TileProcessing> tiles)
    {
        if (destinationData == null)
        {
            return false;
        }
        if (tiles == null)
        {
            return false;
        }
        var tile = tiles.Where(x => x.TransformationError == tiles.Min(y => y.TransformationError)).FirstOrDefault();
        if (tile == null)
        {
            return false;
        }
        return tile.MergeData(destinationData);
    }
}