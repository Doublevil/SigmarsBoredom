using System.Drawing;

namespace SigmarsBoredom
{
    /// <summary>
    /// Represents a move that can be made in a certain board configuration.
    /// </summary>
    public readonly struct Play
    {
        /// <summary>
        /// Gets the board coordinates of the first tile to be played.
        /// </summary>
        public Point FirstTile { get; }

        /// <summary>
        /// Gets the board coordinates of the other tile to be played.
        /// </summary>
        public Point SecondTile { get; }

        /// <summary>
        /// Gets the priority score of the play. A higher score means the play has less priority.
        /// </summary>
        public int PriorityScore { get; }

        /// <summary>
        /// Builds a play based on two tiles to be played.
        /// </summary>
        /// <param name="firstTile">Board coordinates of the first tile to be played.</param>
        /// <param name="secondTile">Board coordinates of the other tile to be played.</param>
        /// <param name="priorityScore">Priority score of the play. A higher score means the play has less priority.</param>
        public Play(Point firstTile, Point secondTile, int priorityScore)
        {
            FirstTile = firstTile;
            SecondTile = secondTile;
            PriorityScore = priorityScore;
        }
    }
}
