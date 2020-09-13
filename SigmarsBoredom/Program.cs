namespace SigmarsBoredom
{
    /// <summary>
    /// Sigmar's Boredom.
    /// A solver for Opus Magnum's mini-game: Sigmar's Garden.
    /// The program assumes that you have Opus Magnum up and running, that you have opened Sigmar's Garden,
    /// and that the board is currently in its initial position.
    /// It will capture Opus Magnum's window, and manipulate the mouse cursor to click on certain spots.
    /// It is designed to automatically solve Sigmar's Garden games in an infinite loop, starting new games automatically when it's done.
    /// The game must be run as a borderless window at a resolution of 1920x1080.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Name of the process that runs Opus Magnum.
        /// </summary>
        public const string OpusMagnumProcessName = "Lightning";

        static void Main(string[] args)
        {
            var solver = new Solver();
            solver.Run();
        }
    }
}
