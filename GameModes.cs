public enum eGameModes {
    singlePlayer,
    multiPlayer
}

public interface IGameMode
{
    eGameModes Mode { get; set; }
}

public class GameMode : IGameMode
{
    public eGameModes Mode { get; set; }

    public GameMode(eGameModes i_mode)
    {
        Mode = i_mode;
    }
}