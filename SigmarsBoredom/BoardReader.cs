using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using NLog;

namespace SigmarsBoredom
{
    /// <summary>
    /// Captures images from the game to read the board.
    /// </summary>
    public class BoardReader
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Captures and reads the board in its current state.
        /// </summary>
        public Board ReadBoard()
        {
            _logger.Info("Reading the board...");

            var board = new Board();
            var captureService = new CaptureService(Program.OpusMagnumProcessName);
            var inputService = new InputService(Program.OpusMagnumProcessName);

            // Scan for most tile types using their highlight button.
            foreach (var tileToScan in new[] {
                Tile.Salt, Tile.Air, Tile.Fire, Tile.Water, Tile.Earth, Tile.Quicksilver, Tile.Lead, Tile.Tin,
                Tile.Iron, Tile.Copper, Tile.Silver, Tile.Gold
            })
            {
                // Click on the highlight button for that tile
                Point highlightButtonCoordinates = SigmarCoordinateHelper.MarbleHintCoordinates[(int)tileToScan - 1];
                inputService.SendMouseLeftButtonDown(highlightButtonCoordinates.X, highlightButtonCoordinates.Y);
                Thread.Sleep(100); // Sleep a while so the game gets some time to draw the highlight circles

                // Capture the image
                using (var boardBmp = captureService.GetWindowImage(SigmarCoordinateHelper.BoardRectangle))
                {
                    // Release the mouse click
                    inputService.SendMouseLeftButtonUp(highlightButtonCoordinates.X, highlightButtonCoordinates.Y);
                    boardBmp.Save($"{tileToScan}.bmp");

                    // Scan for highlighted marbles on the board image - all highlighted marbles are guaranteed to be of that tile type.
                    foreach (var highlightedMarble in ScanBoardForHighlightedMarbles(boardBmp))
                    {
                        _logger.Info($"Read a {tileToScan} at ({highlightedMarble.X}, {highlightedMarble.Y})");
                        board.Tiles[highlightedMarble.X, highlightedMarble.Y] = tileToScan;
                    }
                }
            }

            // Scan for Mors and Vitae (they don't have a highlight button for some reason).
            using (var boardBmp = captureService.GetWindowImage(SigmarCoordinateHelper.BoardRectangle))
            {
                foreach (var morsMarble in ScanBoardForMors(boardBmp, board.FindTilesOfType(Tile.Empty)))
                {
                    _logger.Info($"Read a Mors at ({morsMarble.X}, {morsMarble.Y})");
                    board.Tiles[morsMarble.X, morsMarble.Y] = Tile.Mors;
                }

                foreach (var vitaeMarble in ScanBoardForVitae(boardBmp, board.FindTilesOfType(Tile.Empty)))
                {
                    _logger.Info($"Read a Vitae at ({vitaeMarble.X}, {vitaeMarble.Y})");
                    board.Tiles[vitaeMarble.X, vitaeMarble.Y] = Tile.Vitae;
                }
            }

            // Check if the board is valid before returning it
            return ThrowOnInvalidBoard(board);
        }

        /// <summary>
        /// Checks the given board to see if it is a valid starting board.
        /// </summary>
        /// <param name="board">Board to check.</param>
        private Board ThrowOnInvalidBoard(Board board)
        {
            int[] counters = new int[Enum.GetValues(typeof(Tile)).Length];
            for (int x = 0; x < board.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < board.Tiles.GetLength(1); y++)
                {
                    Tile currentTile = board.Tiles[x, y];
                    counters[(int)currentTile]++;
                }
            }


            // if incorrect number of tiles are found, return null
            if (counters[(int)Tile.Salt] != 4) return null;
            if (counters[(int)Tile.Air] != 8) return null;
            if (counters[(int)Tile.Fire] != 8) return null;
            if (counters[(int)Tile.Water] != 8) return null;
            if (counters[(int)Tile.Earth] != 8) return null;
            if (counters[(int)Tile.Quicksilver] != 5) return null;
            if (counters[(int)Tile.Lead] != 1) return null;
            if (counters[(int)Tile.Tin] != 1) return null;
            if (counters[(int)Tile.Iron] != 1) return null;
            if (counters[(int)Tile.Copper] != 1) return null;
            if (counters[(int)Tile.Silver] != 1) return null;
            if (counters[(int)Tile.Gold] != 1) return null;
            if (counters[(int)Tile.Vitae] != 4) return null;
            if (counters[(int)Tile.Mors] != 4) return null;

            // if its a valid board, return the board
            _logger.Info("Successfully read the board.");
            return board;
        }

        /// <summary>
        /// Scans the board bitmap for highlighted marbles and returns their positions.
        /// </summary>
        /// <param name="boardBmp">Bitmap of the game board.</param>
        private List<Point> ScanBoardForHighlightedMarbles(Bitmap boardBmp)
        {
            var highlightedPoints = new List<Point>();

            // Browse the board
            for (int boardX = 0; boardX < SigmarCoordinateHelper.BoardSize; boardX++)
            {
                for (int boardY = 0; boardY < SigmarCoordinateHelper.BoardSize; boardY++)
                {
                    if (!SigmarCoordinateHelper.IsValidBoardCoordinate(boardX, boardY))
                    {
                        // Invalid coordinate. Don't bother.
                        continue;
                    }

                    // Get the rectangle on the image matching the marble to recognize at the current board position
                    var targetRectangle = SigmarCoordinateHelper.GetMarbleRectangle(boardX, boardY);

                    // Get some pixels around the marble and figure out if they are bright enough to be highlights
                    var targetColorBottom = boardBmp.GetPixel(targetRectangle.X + targetRectangle.Width / 2, targetRectangle.Bottom - 2);
                    var targetColorTop = boardBmp.GetPixel(targetRectangle.X + targetRectangle.Width / 2 - 8, targetRectangle.Top);
                    if (targetColorBottom.GetBrightness() > 0.85f || targetColorTop.GetBrightness() > 0.85f)
                    {
                        highlightedPoints.Add(new Point(boardX, boardY));
                    }
                }
            }

            return highlightedPoints;
        }

        /// <summary>
        /// Scans the board bitmap for Mors marbles (there is no highlight button for Mors and Vitae).
        /// </summary>
        /// <param name="boardBmp">Bitmap of the game board.</param>
        /// <param name="potentialPoints">Points with board coordinates to look for on the board.</param>
        private List<Point> ScanBoardForMors(Bitmap boardBmp, IEnumerable<Point> potentialPoints)
        {
            var highlightedPoints = new List<Point>();

            // Browse the board
            foreach (var point in potentialPoints)
            {
                if (!SigmarCoordinateHelper.IsValidBoardCoordinate(point.X, point.Y))
                {
                    // Invalid coordinate. Don't bother.
                    continue;
                }

                // Get the rectangle on the image matching the marble to recognize at the current board position
                var targetRectangle = SigmarCoordinateHelper.GetMarbleRectangle(point.X, point.Y);

                // Get some pixel in the center of the first pixel row and try to approximate it as the highlight color
                var someMarblePixel = boardBmp.GetPixel(targetRectangle.Left + 18, targetRectangle.Top + 8);
                var someBgPixel = boardBmp.GetPixel(targetRectangle.Left + 18, targetRectangle.Top);
                if (someMarblePixel.GetBrightness() < someBgPixel.GetBrightness())
                {
                    highlightedPoints.Add(point);
                }
            }

            return highlightedPoints;
        }

        /// <summary>
        /// Scans the board bitmap for Vitae marbles (there is no highlight button for Mors and Vitae).
        /// </summary>
        /// <param name="boardBmp">Bitmap of the game board.</param>
        /// <param name="potentialPoints">Points with board coordinates to look for on the board.</param>
        private List<Point> ScanBoardForVitae(Bitmap boardBmp, IEnumerable<Point> potentialPoints)
        {
            var highlightedPoints = new List<Point>();

            // Browse the board
            foreach (var point in potentialPoints)
            {
                if (!SigmarCoordinateHelper.IsValidBoardCoordinate(point.X, point.Y))
                {
                    // Invalid coordinate. Don't bother.
                    continue;
                }

                // Get the rectangle on the image matching the marble to recognize at the current board position
                var targetRectangle = SigmarCoordinateHelper.GetMarbleRectangle(point.X, point.Y);

                // Get some pixel in the center of the first pixel row and try to approximate it as the highlight color
                var arrowPixel = boardBmp.GetPixel(targetRectangle.Left + 26, targetRectangle.Top + 26);
                var marblePixel = boardBmp.GetPixel(targetRectangle.Left + 26, targetRectangle.Top + 41);
                if (arrowPixel.GetBrightness() > marblePixel.GetBrightness())
                {
                    highlightedPoints.Add(point);
                }
            }

            return highlightedPoints;
        }
    }
}
