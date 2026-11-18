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
    public static void AssertValid(TileProcessing tileProc)
    {
        if (tileProc == null)
            throw new ArgumentNullException($"{nameof(tileProc) is null}");
        TileItem.AssertValid(tileProc.tileItem);
    }
  
    /// <summary>
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="destinationData"></param>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    /// <returns>true if data</returns>
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
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="destinationData"></param>
    /// <param name="tiles">List to tiles to compare</param>
    /// <returns></returns>
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