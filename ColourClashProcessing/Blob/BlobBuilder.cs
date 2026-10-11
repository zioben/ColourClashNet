using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Builds blobs (connected components) from run lengths.
    ///
    /// Algorithm (run-based connected component labeling):
    ///   1. sort the runs by (Y, XS);
    ///   2. for every pair of adjacent rows, walk the two lists of runs with two pointers
    ///      and union (disjoint-set) every pair of runs that <see cref="RunLength.Connected"/>
    ///      reports as touching - O(runs), not O(runs^2);
    ///   3. runs sharing the same set root belong to the same blob.
    ///
    /// Working on runs instead of pixels keeps the cost proportional to the number of runs,
    /// which is far smaller than the number of pixels on typical sprite/bitmap content.
    /// </summary>
    public static class BlobBuilder
    {
        /// <param name="runLengths">Run lengths of the foreground; any order is accepted.</param>
        /// <param name="mode">4 or 8 connectivity.</param>
        /// <returns>
        /// Blobs ordered by the scan position of their first run (top-most, then left-most),
        /// with BlobId numbered 1..N after filtering.
        /// </returns>
        public static List<BlobItem> Build(
            IEnumerable<RunLength> runLengths,
            BlobConnectionModes mode,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(runLengths);

            // 1. canonical order: row by row, left to right inside the row
            RunLength[] runs = runLengths
                .Where(r => r != null)
                .OrderBy(r => r.Y)
                .ThenBy(r => r.XS)
                .ToArray();

            var blobs = new List<BlobItem>();
            if (runs.Length == 0)
                return blobs;

            // 2. link runs of adjacent rows
            var sets = new DisjointSet(runs.Length);
            int slack = mode == BlobConnectionModes.EightConnected ? 1 : 0;

            int prevStart = -1, prevEnd = -1;   // previous row = runs[prevStart..prevEnd)
            int curStart = 0;
            while (curStart < runs.Length)
            {
                token.ThrowIfCancellationRequested();

                int y = runs[curStart].Y;
                int curEnd = curStart + 1;
                while (curEnd < runs.Length && runs[curEnd].Y == y)
                    curEnd++;

                // rows must be consecutive: an empty row in between breaks any connection
                if (prevStart >= 0 && runs[prevStart].Y == y - 1)
                    LinkRows(runs, prevStart, prevEnd, curStart, curEnd, mode, slack, sets);

                prevStart = curStart;
                prevEnd = curEnd;
                curStart = curEnd;
            }

            // 3. group runs by set root -> blobs
            var blobByRoot = new Dictionary<int, BlobItem>();
            for (int k = 0; k < runs.Length; k++)
            {
                int root = sets.Find(k);
                if (!blobByRoot.TryGetValue(root, out BlobItem? blob))
                {
                    blob = new BlobItem();
                    blobByRoot.Add(root, blob);
                    blobs.Add(blob);    // creation order == scan order of the first run
                }
                blob.Add(runs[k]);
            }

           
            for (int k = 0; k < blobs.Count; k++)
                blobs[k].BlobId = k + 1;

            return blobs;
        }

        /// <summary>
        /// Two-pointer sweep over the runs of two consecutive rows (both sorted by XS).
        /// After testing a pair, the run that ends first is advanced: it cannot touch
        /// anything further along the other row.
        /// For 8-connectivity the previous-row run is "virtually" one pixel wider on the
        /// right (slack), consistent with the +1 tolerance used by RunLength.Connected.
        /// </summary>
        static void LinkRows(
            RunLength[] runs,
            int prevStart, int prevEnd,
            int curStart, int curEnd,
            BlobConnectionModes mode, int slack,
            DisjointSet sets)
        {
            int i = prevStart;
            int j = curStart;
            while (i < prevEnd && j < curEnd)
            {
                RunLength p = runs[i];
                RunLength c = runs[j];

                if (RunLength.Connected(p, c, mode))
                    sets.Union(i, j);

                // on a tie advance the previous row: the current run may still touch the next one
                if (p.XE + slack <= c.XE)
                    i++;
                else
                    j++;
            }
        }

        /// <summary>Disjoint-set (union-find) with path halving and union by size.</summary>
        sealed class DisjointSet
        {
            readonly int[] _parent;
            readonly int[] _size;

            public DisjointSet(int count)
            {
                _parent = new int[count];
                _size = new int[count];
                for (int i = 0; i < count; i++)
                {
                    _parent[i] = i;
                    _size[i] = 1;
                }
            }

            public int Find(int x)
            {
                while (_parent[x] != x)
                {
                    _parent[x] = _parent[_parent[x]];   // path halving
                    x = _parent[x];
                }
                return x;
            }

            public void Union(int a, int b)
            {
                int ra = Find(a);
                int rb = Find(b);
                if (ra == rb) return;

                if (_size[ra] < _size[rb])
                    (ra, rb) = (rb, ra);

                _parent[rb] = ra;
                _size[ra] += _size[rb];
            }
        }
    }
}
