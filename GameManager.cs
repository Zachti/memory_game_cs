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

    public Player CurrentPlayer
    {
        get
        {
            return r_GameData.CurrentPlayer;
        }

        set
        {
            r_GameData.CurrentPlayer = value;
        }
    }
    
    public bool validatePlayerInput(string i_userInput) {
        bool isValid = false;
        if (i_userInput == null) {
            Console.WriteLine("Input must not be empty");
            return isValid;
        }
        i_userInput = i_userInput.ToUpper();
        return i_userInput != "Q" && validateCellSelection(i_userInput) && validateCellIsHidden(i_userInput);
    }

    private bool validateCellSelection(string i_userInput) {
        bool isValid = false;
        if(i_userInput.Length != 2)
        {
            Console.WriteLine("Input must have exactly 2 characters");
            return false;
        }
        return checkIfLetterInRange(i_userInput[0]) && checkIfDigitInRange(i_userInput[1]);
    }

    private bool checkIfLetterInRange(char i_Letter) {
        char lastLetter = (char)('A' + BoardWidth - 1);
        if (i_Letter < 'A' || i_Letter > lastLetter) {
            Console.WriteLine("Invalid input, letter must be between A and {0}", lastLetter);
            return false;
        }
        return true;
    }

    private bool checkIfDigitInRange(char i_Digit) {
        char lastDigit = (char)('0' + BoardHeight);
        if (i_Digit < '1' || i_Digit > lastDigit) {
            Console.WriteLine("Invalid input, digit must be between 1 and {0}", lastDigit);
            return false;
        }
        return true;
    }

    private bool validateCellIsHidden(string i_userInput) {
        int column = i_userInput[0] - 'A';
        int row = i_userInput[1] - '1';
        bool isValid = !r_GameData.Letters[row, column].IsRevealed;
        if (!isValid)
        {
           Console.WriteLine("Cell {0} is already revealed", i_userInput);
        }
        return isValid;
    }
}