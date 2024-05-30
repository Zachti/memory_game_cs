public class GameManager
{
    private static int Difficulty { get; } = 50;
    public static int MinBoardWidth { get; } = 4;
    public static int MaxBoardWidth { get; } = 6;
    public static int MinBoardHeight { get; } = 4;
    public static int MaxBoardHeight { get; } = 6;
    public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
    private bool FoundMatch { get; set; } = false;
    private bool IsFirstSelection { get; set; } = false;
    private bool AiHasMatches { get; set; } = false;
    public int BoardWidth => r_GameData.BoardWidth;
    public int BoardHeight => r_GameData.BoardHeight;
    public BoardLetter[,] Letters => r_GameData.Letters;
    public Player CurrentPlayer { get => r_GameData.CurrentPlayer; set => r_GameData.CurrentPlayer = value; }
    public bool SelectionNotMatching { get; set; } = false;

    private readonly eGameModes r_GameMode;
    private readonly GameData r_GameData;
    private readonly Dictionary<Cell, char> r_AiMemory;
    private Cell m_AiSelection;
    private Cell m_CurrentUserSelection;
    private Cell m_PreviousUserSelection;

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

    public string GetScoreboard()
    {
        return string.Format(
            "Score: {0} {1} - {2} {3}",
            r_GameData.PlayerOne.PlayerName,
            r_GameData.PlayerOne.PlayerScore,
            r_GameData.PlayerTwo.PlayerName,
            r_GameData.PlayerTwo.PlayerScore );
    }

    public void ChangeTurn() {
        CurrentPlayer = CurrentPlayer == r_GameData.PlayerOne ? r_GameData.PlayerTwo : r_GameData.PlayerOne;

        getBoardLetterAt(m_CurrentUserSelection).IsRevealed = false;
        getBoardLetterAt(m_PreviousUserSelection).IsRevealed = false;
        SelectionNotMatching = false;
    }

    public void Update(Cell i_UserSelection)
    {       
        if(!SelectionNotMatching) {
                UpdateNextTurn(i_UserSelection);
        }
        if ((r_GameData.PlayerOne.PlayerScore + r_GameData.PlayerTwo.PlayerScore) == (BoardWidth * BoardHeight) / 2)
        {
            CurrentGameState = eGameStates.GameOver;
        }
    }

    private void UpdateNextTurn(Cell i_UserSelection)
    {
        m_CurrentUserSelection = i_UserSelection;

        if (r_GameMode == eGameModes.singlePlayer)
        {
            if (GameData.GetRandomNumber(0, 100) <= Difficulty)
            {
                addToAiMemory(m_CurrentUserSelection);
            }
        }

        if (IsFirstSelection)
        {
            m_PreviousUserSelection = m_CurrentUserSelection;
            getBoardLetterAt(m_CurrentUserSelection).IsRevealed = true;
            IsFirstSelection = false;
        } else {
            BoardLetter firstSelectionLetter = getBoardLetterAt(m_PreviousUserSelection);
            BoardLetter secondSelectionLetter = getBoardLetterAt(m_CurrentUserSelection);

            secondSelectionLetter.IsRevealed = true;

            SelectionNotMatching = firstSelectionLetter.Letter != secondSelectionLetter.Letter;

            if (!SelectionNotMatching) {
                if (r_GameMode == eGameModes.singlePlayer) {
                    r_AiMemory.Remove(m_CurrentUserSelection);
                    r_AiMemory.Remove(m_PreviousUserSelection);
                }

                CurrentPlayer.PlayerScore++;
            }
            IsFirstSelection = true;
        }
    }

    private void addToAiMemory(Cell i_CellToBeAdded)
    {
        if(!r_AiMemory.ContainsKey(i_CellToBeAdded))
        {
            r_AiMemory.Add(i_CellToBeAdded, Letters[i_CellToBeAdded.Row, i_CellToBeAdded.Column].Letter);
        }
    }

    private BoardLetter getBoardLetterAt(Cell i_CellLocation)
    {
        return Letters[i_CellLocation.Row, i_CellLocation.Column];
    }

    public string GetAiInput()
     {
        string aiSelection;

        if(r_AiMemory.Count == 0)
        {
            AiHasMatches = false;
            aiSelection = GetRandomUnmemorizedCell();
        } else {
            aiSelection = IsFirstSelection ? GetFirstSelection() : GetSecondSelection();
        }
        return aiSelection;
    }
    
    private string GetFirstSelection()
    {
        string firstSelection = null;

        FoundMatch = findLetterMatch(ref firstSelection);

        if(FoundMatch)
        {
            AiHasMatches = true;
        } else {
                AiHasMatches = false;
                firstSelection = GetRandomUnmemorizedCell();
            }

        return firstSelection;
    }

    private string GetSecondSelection()
    {
        string secondSelection;

        if(FoundMatch)
        {
            secondSelection = m_AiSelection.ToString();
        } else {
            secondSelection = findLetterInMemory(m_AiSelection);
            if(secondSelection != null)
            {
                AiHasMatches = true;
            } else {
                AiHasMatches = false;
                secondSelection = GetRandomUnmemorizedCell();
            }
        }
        return secondSelection;
     }
    
    private string findLetterInMemory(Cell i_FirstSelectionCell)
    {
        string foundLetter = null;

        foreach(var memorizedLetter in r_AiMemory)
        {
            Cell currentKey = memorizedLetter.Key;
            char firstSelectionLetter = Letters[i_FirstSelectionCell.Row, i_FirstSelectionCell.Column].Letter;

            if(!currentKey.Equals(i_FirstSelectionCell) && memorizedLetter.Value == firstSelectionLetter) 
            {
                foundLetter = memorizedLetter.Key.ToString();
            }
        }
        return foundLetter;
    }

    private string GetRandomUnmemorizedCell()
    {
        int row = Letters.GetLength(0);
        int column = Letters.GetLength(1);
        Cell[] cellsNotInMemory = new Cell[(BoardHeight * BoardWidth) - r_AiMemory.Count];
        int indexOfCellNotInMemory = 0;

        for (int i = 0; i < row; i++)
        {
            for(int j = 0; j < column; j++)
            {
                if(!Letters[i, j].IsRevealed)
                {
                    if(!r_AiMemory.ContainsKey(new Cell(i, j)))
                    {
                        cellsNotInMemory[indexOfCellNotInMemory++] = new Cell(i, j);
                    }
                }
            }
         }
        indexOfCellNotInMemory = GameData.GetRandomNumber(0, indexOfCellNotInMemory);
        m_AiSelection = cellsNotInMemory[indexOfCellNotInMemory];
        return m_AiSelection.ToString();
    }

    private bool findLetterMatch(ref string i_MemorizedMatchingLetter)
    {
        bool foundMatch = false;

        foreach (var firstMemorizedLetter in r_AiMemory)
        {
            foreach (var secondMemorizedLetter in r_AiMemory)
            {
                if (!firstMemorizedLetter.Key.Equals(secondMemorizedLetter.Key))
                {
                     if (firstMemorizedLetter.Value == secondMemorizedLetter.Value)
                    {
                        i_MemorizedMatchingLetter = firstMemorizedLetter.Key.ToString();
                        m_AiSelection = secondMemorizedLetter.Key;
                        foundMatch = true;
                    }
                }
             }
         }
            return foundMatch;
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