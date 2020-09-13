using System.Collections.Generic;
using System.Drawing;

namespace SigmarsBoredom
{
    /// <summary>
    /// Represents the game board. Contains a certain number of <see cref="Tile"/> items.
    /// </summary>
    public class Board
    {
        /// <summary>
        /// Gets or sets the table of tiles composing the board.
        /// </summary>
        public Tile[,] Tiles { get; set; }

        /// <summary>
        /// Builds an empty board.
        /// </summary>
        public Board()
        {
            Tiles = new Tile[SigmarCoordinateHelper.BoardSize, SigmarCoordinateHelper.BoardSize];
        }

        /// <summary>
        /// Gets the coordinates of tiles of a given type.
        /// </summary>
        /// <param name="tile">Target tile type.</param>
        public IEnumerable<Point> FindTilesOfType(Tile tile)
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    if (Tiles[x, y] == tile)
                        yield return new Point(x, y);
                }
            }
        }

        /// <summary>
        /// Gets the first metal tile in the metal sequence is currently on the board, if any.
        /// </summary>
        public Tile? GetFirstMetalRemainingInSequence()
        {
            Tile? bestFoundYet = null;
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile >= Tile.Lead && tile <= Tile.Gold && (bestFoundYet == null || tile < bestFoundYet.GetValueOrDefault()))
                        bestFoundYet = tile;
                }
            }

            return bestFoundYet;
        }

        /// <summary>
        /// Gets the number of marbles remaining on the board.
        /// </summary>
        public int GetRemainingMarbleCount()
        {
            int count = 0;
            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                    if (Tiles[x, y] != Tile.Empty)
                        count++;

            return count;
        }
    }
}
