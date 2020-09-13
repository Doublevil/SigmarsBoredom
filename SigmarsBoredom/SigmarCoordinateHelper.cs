using System.Drawing;
using System.Linq;

namespace SigmarsBoredom
{
    /// <summary>
    /// Provides constants and methods that help with the coordinates of the items to identify in Opus Magnum.
    /// </summary>
    public static class SigmarCoordinateHelper
    {
        /// <summary>
        /// X coordinate of the board in the Opus Magnum main window.
        /// </summary>
        public const int BoardStartX = 861;

        /// <summary>
        /// Y coordinate of the board in the Opus Magnum main window.
        /// </summary>
        public const int BoardStartY = 195;

        /// <summary>
        /// Width of the board in pixels.
        /// </summary>
        public const int BoardWidth = 712;

        /// <summary>
        /// Height of the board in pixels.
        /// </summary>
        public const int BoardHeight = 622;

        /// <summary>
        /// Rectangle of the board in the Opus Magnum main window.
        /// </summary>
        public static readonly Rectangle BoardRectangle = new Rectangle(BoardStartX, BoardStartY, BoardWidth, BoardHeight);

        /// <summary>
        /// Size (in both width and height) of a marble in a tile.
        /// </summary>
        public const int MarbleSize = 52;

        /// <summary>
        /// Number of tiles in both width and height on the board.
        /// </summary>
        public const int BoardSize = 11;

        /// <summary>
        /// Offset in X coordinates between each potential marble.
        /// </summary>
        public const int MarbleOffsetX = 66;

        /// <summary>
        /// Offset in Y coordinates between each potential marble.
        /// </summary>
        public const int MarbleOffsetY = 57;

        /// <summary>
        /// Coordinates of tile spots on the board that are not tiles because they're outside of the board.
        /// </summary>
        public static readonly Point[] DeadTileSpots = {
            // Top-left corner
            new Point(00, 00), new Point(01, 00), new Point(02, 00), new Point(00, 01), new Point(01, 01), new Point(00, 02), new Point(01, 02), new Point(00, 03), new Point(00, 04),
            // Bottom-left corner
            new Point(00, 06), new Point(00, 07), new Point(00, 08), new Point(01, 08), new Point(00, 09), new Point(01, 09), new Point(00, 10), new Point(01, 10), new Point(02, 10),
            // Top-right corner
            new Point(09, 00), new Point(10, 00), new Point(09, 01), new Point(10, 01), new Point(10, 02), new Point(10, 03),
            // Bottom-right corner
            new Point(10, 07), new Point(10, 08), new Point(09, 09), new Point(10, 09), new Point(09, 10), new Point(10, 10)
        };

        /// <summary>
        /// Coordinates of buttons that highlight marbles of the same type.
        /// </summary>
        public static readonly Point[] MarbleHintCoordinates =
        {
            new Point(970, 884), // Salt
            new Point(1023, 884), // Air
            new Point(1065, 884), // Fire
            new Point(1107, 884), // Water
            new Point(1149, 884), // Earth
            new Point(1209, 884), // Quicksilver
            new Point(1264, 884), // Lead
            new Point(1304, 884), // Tin
            new Point(1344, 884), // Iron
            new Point(1384, 884), // Copper
            new Point(1424, 884), // Silver
            new Point(1464, 884) // Gold
        };

        /// <summary>
        /// Coordinates of the "New game" button.
        /// </summary>
        public static readonly Point NewGameButtonPosition = new Point(870, 885);

        /// <summary>
        /// Gets the coordinates in the Opus Magnum window of the tile at the specified board coordinates.
        /// </summary>
        /// <param name="boardCoordinates">Board coordinates of the target tile.</param>
        public static Point GetWindowCoordinateOfTileCenterOnBoard(Point boardCoordinates)
        {
            var boardImagePos = GetImageCoordinateOfTileOnBoard(boardCoordinates.X, boardCoordinates.Y);
            return new Point(boardImagePos.X + BoardStartX + MarbleSize / 2, boardImagePos.Y + BoardStartY + MarbleSize / 2);
        }

        /// <summary>
        /// Gets the coordinates on the board image of the tile at the specified board coordinates.
        /// </summary>
        /// <param name="boardX">X coordinate of the target tile on the board.</param>
        /// <param name="boardY">Y coordinate of the target tile on the board.</param>
        public static Point GetImageCoordinateOfTileOnBoard(int boardX, int boardY)
        {
            int offsetX = (boardY % 2 - 1) * MarbleOffsetX / 2;
            return new Point(MarbleOffsetX * boardX + offsetX, boardY * MarbleOffsetY);
        }

        /// <summary>
        /// Gets the rectangle in image coordinates that contains the marble (or center of the empty tile) at the given board coordinates.
        /// </summary>
        /// <param name="boardX">X coordinate of the target tile on the board.</param>
        /// <param name="boardY">Y coordinate of the target tile on the board.</param>
        public static Rectangle GetMarbleRectangle(int boardX, int boardY)
        {
            return new Rectangle(GetImageCoordinateOfTileOnBoard(boardX, boardY), new Size(MarbleSize, MarbleSize));
        }

        /// <summary>
        /// Determines whether the given coordinates match a valid tile on the board.
        /// </summary>
        /// <param name="boardX">Target X coordinate on the board.</param>
        /// <param name="boardY">Target Y coordinate on the board.</param>
        public static bool IsValidBoardCoordinate(int boardX, int boardY)
        {
            return boardX >= 0 && boardY >= 0 && boardX < BoardSize && boardY < BoardSize && !DeadTileSpots.Contains(new Point(boardX, boardY));
        }
    }
}
