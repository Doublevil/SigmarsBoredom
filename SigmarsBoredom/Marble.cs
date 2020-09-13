namespace SigmarsBoredom
{
    /// <summary>
    /// Represents a marble at a given spot on the board.
    /// </summary>
    public readonly struct Marble
    {
        /// <summary>
        /// Gets the X coordinate of the marble on the board.
        /// </summary>
        public int BoardX { get; }

        /// <summary>
        /// Gets the Y coordinate of the marble on the board.
        /// </summary>
        public int BoardY { get; }

        /// <summary>
        /// Gets the type of tile of the marble.
        /// </summary>
        public Tile TileType { get; }

        /// <summary>
        /// Builds a new marble.
        /// </summary>
        /// <param name="boardX">X coordinate of the marble on the board.</param>
        /// <param name="boardY">Y coordinate of the marble on the board.</param>
        /// <param name="tileType">Type of tile of the marble.</param>
        public Marble(int boardX, int boardY, Tile tileType)
        {
            BoardX = boardX;
            BoardY = boardY;
            TileType = tileType;
        }

        /// <summary>
        /// Gets a value indicating if the marble is a Fire, Earth, Air, Water or Salt.
        /// </summary>
        public bool IsElementaryCompatible()
        {
            return TileType == Tile.Air
                   || TileType == Tile.Earth
                   || TileType == Tile.Fire
                   || TileType == Tile.Salt
                   || TileType == Tile.Water;
        }

        /// <summary>
        /// Gets a value indicating if the marble is a metal.
        /// </summary>
        public bool IsMetal()
        {
            return TileType >= Tile.Lead && TileType <= Tile.Gold;
        }

        /// <summary>
        /// Determines the equality between this instance and the given object.
        /// </summary>
        /// <param name="obj">Target object.</param>
        public override bool Equals(object obj)
        {
            return obj is Marble other && Equals(other);
        }

        /// <summary>
        /// Determines the equality between this instance and the given marble.
        /// </summary>
        /// <param name="other">Target marble.</param>
        public bool Equals(Marble other)
        {
            return BoardX == other.BoardX && BoardY == other.BoardY && TileType == other.TileType;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BoardX;
                hashCode = (hashCode * 397) ^ BoardY;
                hashCode = (hashCode * 397) ^ (int) TileType;
                return hashCode;
            }
        }
    }
}
