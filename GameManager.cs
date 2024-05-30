public class GameManager
{
    private const int k_DifficultyOdds = 80;
    public static int MinBoardWidth { get; } = 4;
    public static int MaxBoardWidth { get; } = 6;
    public static int MinBoardHeight { get; } = 4;
    public static int MaxBoardHeight { get; } = 6;
    public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
    private readonly eGameModes r_GameMode;
    private readonly GameData r_GameData;
    private readonly Dictionary<Cell, char> r_AiMemory;
    private Cell m_AiSelection;
    private Cell m_CurrentUserSelection;
    private Cell m_PreviousUserSelection;
    private bool m_FoundMatch = false;
    private bool m_IsFirstSelection = false;
    private bool m_SelectionNotMatching = false;
    private bool m_AiHasMatches = false;

    public GameManager(Player i_Player1, Player i_Player2, int i_Height, int i_Width, eGameModes i_GameMode)
    {
        r_GameData = new GameData(i_Player1, i_Player2, i_Width, i_Height);
        r_GameData.InitializeBoard();
        r_GameMode = i_GameMode;
        CurrentGameState = eGameStates.Running;
        if (r_GameMode == eGameModes.singlePlayer)
        {
            r_AiMemory = new Dictionary<Cell, char>();
        }
    }

    public int BoardWidth
    {
        get { return r_GameData.BoardWidth; }
    }

    public int BoardHeight
    {
        get { return r_GameData.BoardHeight; }
    }
}