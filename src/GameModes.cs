namespace MemoryGame {
    internal enum eGameModes 
    {
        singlePlayer,
        multiPlayer
    }

    internal interface IGameMode
    {
        eGameModes SelectedMode { get; set; }
    }

    internal class GameMode : IGameMode
    {
        public eGameModes SelectedMode { get; set; } = eGameModes.singlePlayer;
    }
}