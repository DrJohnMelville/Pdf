// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

namespace CoreJ2K.Util    
{
    using System;

    using j2k;
    using j2k.image;

    public class PortableImageSource : BlkImgDataSrc
    {
        #region FIELDS

        private readonly int rb;

        private readonly bool[] sgnd;

        private readonly int[][] comps;

        #endregion

        #region CONSTRUCTORS

        public PortableImageSource(int w, int h, int nc, int rb, bool[] sgnd, int[][] comps)
        {
            this.TileWidth = w;
            this.TileHeight = h;
            this.NumComps = nc;
            this.rb = rb;
            this.sgnd = sgnd;
            this.comps = comps;
        }

        #endregion

        #region PROPERTIES

        public int TileWidth { get; }

        public int TileHeight { get; }

        public int NomTileWidth => TileWidth;

        public int NomTileHeight => TileHeight;

        public int ImgWidth => TileWidth;

        public int ImgHeight => TileHeight;

        public int NumComps { get; }

        public int TileIdx => 0;

        public int TilePartULX => 0;

        public int TilePartULY => 0;

        public int ImgULX => 0;

        public int ImgULY => 0;

        #endregion

        #region METHODS

        public int getCompSubsX(int c)
        {
            return 1;
        }

        public int getCompSubsY(int c)
        {
            return 1;
        }

        public int getTileCompWidth(int t, int c)
        {
            if (t != 0)
            {
                throw new InvalidOperationException("Asking a tile-component width for a tile index" + " greater than 0 whereas there is only one tile");
            }
            return TileWidth;
        }

        public int getTileCompHeight(int t, int c)
        {
            if (t != 0)
            {
                throw new InvalidOperationException("Asking a tile-component width for a tile index" + " greater than 0 whereas there is only one tile");
            }
            return TileHeight;
        }

        public int getCompImgWidth(int c)
        {
            return TileWidth;
        }

        public int getCompImgHeight(int c)
        {
            return TileHeight;
        }

        public int getNomRangeBits(int compIndex)
        {
            return rb;
        }

        public void setTile(int x, int y)
        {
            if (x != 0 || y != 0)
            {
                throw new ArgumentException();
            }
        }

        public void nextTile()
        {
            throw new NoNextElementException();
        }

        public Coord getTile(Coord co)
        {
            if (co != null)
            {
                co.x = 0;
                co.y = 0;
                return co;
            }

            return new Coord(0, 0);
        }

        public int getCompULX(int c)
        {
            return 0;
        }

        public int getCompULY(int c)
        {
            return 0;
        }

        public Coord getNumTiles(Coord co)
        {
            if (co != null)
            {
                co.x = 1;
                co.y = 1;
                return co;
            }

            return new Coord(1, 1);
        }

        public int getNumTiles()
        {
            return 1;
        }

        public int GetFixedPoint(int compIndex)
        {
            return 0;
        }

        public DataBlk GetInternCompData(DataBlk blk, int compIndex)
        {
            if (compIndex < 0 || compIndex >= NumComps)
            {
                throw new ArgumentOutOfRangeException(nameof(compIndex));
            }

            var data = new int[blk.w * blk.h];
            for (int y = blk.uly, k = 0; y < blk.uly + blk.h; ++y)
            {
                for (int x = blk.ulx, xy = blk.uly * TileWidth + blk.ulx; x < blk.ulx + blk.w; ++x, ++k, ++xy)
                {
                    data[k] = comps[compIndex][xy];
                }
            }

            blk.offset = 0;
            blk.scanw = blk.w;
            blk.progressive = false;
            blk.Data = data;

            return blk;
        }

        public DataBlk GetCompData(DataBlk blk, int c)
        {
            var newBlk = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
            return GetInternCompData(newBlk, c);
        }

        public void Close()
        {
            // Do nothing.
        }

        public bool IsOrigSigned(int compIndex)
        {
            if (compIndex < 0 || compIndex >= NumComps)
            {
                throw new ArgumentOutOfRangeException(nameof(compIndex));
            }

            return sgnd[compIndex];
        }

        #endregion
    }
}
