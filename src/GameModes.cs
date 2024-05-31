namespace MemoryGame {
    internal enum eGameModes 
    {
        singlePlayer,
        multiPlayer
    }

    internal interface IGameMode
    {
        eGameModes Mode { get; set; }
    }

    internal class GameMode : IGameMode
    {
        public eGameModes Mode { get; set; } = eGameModes.singlePlayer;
    }
}