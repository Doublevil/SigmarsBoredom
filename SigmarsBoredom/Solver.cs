using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NLog;

namespace SigmarsBoredom
{
    /// <summary>
    /// Solves Sigmar's Garden.
    /// </summary>
    public class Solver
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Reads and solves board in an infinite loop.
        /// </summary>
        public void Run()
        {
            var boardReader = new BoardReader();
            while (true)
            {
                // Read the board
                var board = boardReader.ReadBoard();

                // Solve the board
                _logger.Info("Attempting to solve the board...");
                var solution = SolveBoard(board);

                // Play the solution
                if (solution != null)
                {
                    _logger.Info("A solution has been found. Attempting to play it...");
                    PlaySolution(solution);
                }
                else
                {
                    _logger.Warn("No solution was found for this board! This is alarming! Anyway, moving on to the next game.");
                }

                // Start a new round
                StartNewGame();
            }
        }

        /// <summary>
        /// Runs a single round. Stops when the round has been cleared.
        /// </summary>
        private List<Play> SolveBoard(Board board)
        {    
            // List the potential plays
            var playableMarbles = GetPlayableMarbles(board).ToList();
            var potentialPlays = GetPlays(playableMarbles).OrderBy(p => p.PriorityScore).ToList();

            foreach (var play in potentialPlays)
            {
                // Build a new board where the play has been played
                Board newBoard = new Board {Tiles = (Tile[,]) board.Tiles.Clone()};
                newBoard.Tiles[play.FirstTile.X, play.FirstTile.Y] = Tile.Empty;
                newBoard.Tiles[play.SecondTile.X, play.SecondTile.Y] = Tile.Empty;

                var currentSolution = new List<Play> {play};

                // If there are no marbles left: we found a solution. Return a sequence containing only the current move.
                if (newBoard.GetRemainingMarbleCount() == 0)
                    return currentSolution;

                // Check if the new board is a dead-end. If so, return null.
                if (IsDeadEnd(newBoard))
                    return null;

                // Otherwise, recursively solve the board where this move has been played.
                var nextSolution = SolveBoard(newBoard);
                if (nextSolution != null)
                {
                    // If the method returned a non-null result, we have a solution. Append it to the current play and return it.
                    currentSolution.AddRange(nextSolution);
                    return currentSolution;
                }

                // If the recursive called returned null, the play led to a dead-end. Just go on to the next play.
            }

            // If we reach this point, we couldn't find any solution anywhere.
            // So either there were no potential plays at all, or there were some but they all led to a dead-end.
            // In any case, return null.
            return null;
        }

        /// <summary>
        /// Browses the board and returns marbles that are considered playable: they have at least 3 empty tiles around them,
        /// and for metals, they are the first remaining in the sequence.
        /// </summary>
        /// <param name="board">Board to browse.</param>
        private IEnumerable<Marble> GetPlayableMarbles(Board board)
        {
            var firstMetalTileInSequence = board.GetFirstMetalRemainingInSequence();
            for (int x = 0; x < board.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < board.Tiles.GetLength(1); y++)
                {
                    var tile = board.Tiles[x, y];
                    if (tile == Tile.Empty)
                        continue;

                    if (tile >= Tile.Lead && tile <= Tile.Gold && tile != firstMetalTileInSequence)
                    {
                        // We are on a metal tile that cannot be played yet because there is another metal coming before it in the sequence.
                        continue;
                    }

                    // We are looking for 3 contiguous neighbors that are free
                    int lastFreeContiguousNeighbors = 0;
                    int? firstFreeSequence = null;
                    foreach (var neighbor in GetNeighbors(new Point(x, y)))
                    {
                        // A neighbor is considered free if it's out of the board or if it's an empty tile
                        if (!SigmarCoordinateHelper.IsValidBoardCoordinate(neighbor.X, neighbor.Y)
                            || board.Tiles[neighbor.X, neighbor.Y] == Tile.Empty)
                        {
                            // This neighbor is free. Increment the sequence.
                            lastFreeContiguousNeighbors++;

                            // If we already found 3 contiguous neighbors, we're good. Stop searching.
                            if (lastFreeContiguousNeighbors == 3)
                                break;
                        }
                        else
                        {
                            // This neighbor isn't free.
                            // We have to reset the sequence of contiguous free neighbors.
                            // We just keep the first sequence in memory so that it can be added to the last sequence.
                            if (firstFreeSequence == null)
                                firstFreeSequence = lastFreeContiguousNeighbors;
                            lastFreeContiguousNeighbors = 0;
                        }
                    }

                    // We have finished browsing the neighbors in a clockwise fashion (so that every last one is contiguous from the previous one).
                    // To determine if the spot at (x,y) is free, we have to check if there are 3 contiguous free neighbors.
                    // We sum the last free contiguous neighbors number with the first free sequence, so that our starting point doesn't impact the result.
                    // I hope that makes sense.
                    if (lastFreeContiguousNeighbors + firstFreeSequence.GetValueOrDefault() >= 3)
                    {
                        // The tile at (x,y) is free. So we return it.
                        yield return new Marble(x, y, tile);
                    }
                }
            }
        }

        /// <summary>
        /// Get all 6 neighbors of a given point. Includes points that might be off the board.
        /// </summary>
        /// <param name="p">Target point on the board.</param>
        private Point[] GetNeighbors(Point p)
        {
            if (p.Y % 2 == 0)
                return new[] { new Point(p.X-1,p.Y), new Point(p.X-1,p.Y-1), new Point(p.X,p.Y-1), new Point(p.X+1,p.Y), new Point(p.X,p.Y+1), new Point(p.X-1,p.Y+1) };
            return new[] { new Point(p.X-1,p.Y), new Point(p.X,p.Y-1), new Point(p.X+1,p.Y-1), new Point(p.X+1,p.Y), new Point(p.X+1,p.Y+1), new Point(p.X,p.Y+1) };
        }

        /// <summary>
        /// Assuming that the given marbles are free to be played, returns all the possible moves that can be made with them.
        /// </summary>
        private IEnumerable<Play> GetPlays(IEnumerable<Marble> playableMarbles)
        {
            var marbleList = playableMarbles.ToList();
            for (int i = 0; i < marbleList.Count; i++)
            {
                for (int j = i; j < marbleList.Count; j++)
                {
                    var marbleA = marbleList[i];
                    var marbleB = marbleList[j];

                    int? playPriority = null;
                    // First of all, check gold play (it's the only one that can be played alone)
                    if (marbleA.TileType == Tile.Gold && marbleA.Equals(marbleB))
                    {
                        playPriority = 0; // Gold should be played first as it's completely free
                    }
                    else if (marbleA.Equals(marbleB))
                    {
                        // Continue if A and B are the same but not gold.
                        continue;
                    }

                    // Check elementary particles and salt plays
                    if (marbleA.IsElementaryCompatible() && marbleB.IsElementaryCompatible() 
                        && (marbleA.TileType == Tile.Salt || marbleB.TileType == Tile.Salt || marbleA.TileType == marbleB.TileType))
                    {
                        // Not sure about the priority, maybe this should be 2.
                        playPriority = 3;

                        // Using salts might be necessary in some cases but I think it's often better to leave them for later moves.
                        if (marbleA.TileType == Tile.Salt) playPriority++;
                        if (marbleB.TileType == Tile.Salt) playPriority++;
                    }
                    // Check mors/vitae plays
                    else if ((marbleA.TileType == Tile.Vitae && marbleB.TileType == Tile.Mors)
                             || (marbleA.TileType == Tile.Mors && marbleB.TileType == Tile.Vitae))
                    {
                        playPriority = 2;
                    }
                    // Check Quicksilver plays
                    else if ((marbleA.TileType == Tile.Quicksilver && marbleB.IsMetal())
                             || (marbleA.IsMetal() && marbleB.TileType == Tile.Quicksilver))
                    {
                        // It seems to usually be a good idea to get rid early of quicksilvers and metals, so this is a high priority move.
                        playPriority = 1;
                    }

                    // Return the play if it's available.
                    if (playPriority != null)
                    {
                        yield return new Play(new Point(marbleA.BoardX, marbleA.BoardY), new Point(marbleB.BoardX, marbleB.BoardY), playPriority.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Figures out whether the given board is a dead-end.
        /// </summary>
        /// <param name="board">Target board.</param>
        private bool IsDeadEnd(Board board)
        {
            // Fill an array containing a counter for every type of tile.
            int[] counters = new int[Enum.GetValues(typeof(Tile)).Length];
            for (int x = 0; x < board.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < board.Tiles.GetLength(1); y++)
                {
                    Tile currentTile = board.Tiles[x, y];
                    counters[(int)currentTile]++;
                }
            }

            // If there are more than 2 odd numbers of elemental marbles remaining, the game is over
            int oddElementalCounts = 0;
            if (counters[(int)Tile.Air] % 2 == 1) oddElementalCounts++;
            if (counters[(int)Tile.Fire] % 2 == 1) oddElementalCounts++;
            if (counters[(int)Tile.Water] % 2 == 1) oddElementalCounts++;
            if (counters[(int)Tile.Earth] % 2 == 1) oddElementalCounts++;
            if (oddElementalCounts > counters[(int)Tile.Salt])
                return true;

            return false;
        }

        /// <summary>
        /// Plays the given solution.
        /// </summary>
        /// <param name="solution">Sequence of plays to solve the board.</param>
        private void PlaySolution(List<Play> solution)
        {
            var inputService = new InputService(Program.OpusMagnumProcessName);
            foreach (var play in solution)
            {
                // Click the first tile
                Point windowFirstTilePoint = SigmarCoordinateHelper.GetWindowCoordinateOfTileCenterOnBoard(play.FirstTile);
                inputService.SendMouseLeftButtonClick(windowFirstTilePoint.X, windowFirstTilePoint.Y);

                Thread.Sleep(50); // Leave the game some time to react.

                // Click the second tile
                Point windowSecondTilePoint = SigmarCoordinateHelper.GetWindowCoordinateOfTileCenterOnBoard(play.SecondTile);
                inputService.SendMouseLeftButtonClick(windowSecondTilePoint.X, windowSecondTilePoint.Y);

                Thread.Sleep(50); // Leave the game some time to react.
            }
        }

        /// <summary>
        /// Starts a new round.
        /// </summary>
        private void StartNewGame()
        {
            _logger.Info("Starting a new game.");
            var inputService = new InputService(Program.OpusMagnumProcessName);
            inputService.SendMouseLeftButtonClick(SigmarCoordinateHelper.NewGameButtonPosition.X, SigmarCoordinateHelper.NewGameButtonPosition.Y);
            _logger.Info("Waiting for the new game animation to clear...");
            Thread.Sleep(5200); // Wait for the new game animation to finish
        }
    }
}
