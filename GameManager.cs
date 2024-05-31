public class GameManager
{
    private static int Difficulty { get; } = 50;
    public static int MinBoardWidth { get; } = 4;
    public static int MaxBoardWidth { get; } = 6;
    public static int MinBoardHeight { get; } = 4;
    public static int MaxBoardHeight { get; } = 6;
    public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
    private bool FoundMatch { get; set; } = false;
    private bool IsFirstSelection { get; set; } = true;
    public bool AiHasMatches { get; set; } = false;
    public int BoardWidth => r_GameData.BoardWidth;
    public int BoardHeight => r_GameData.BoardHeight;
    public BoardLetter[,] Letters => r_GameData.Letters;
    public Player CurrentPlayer { get => r_GameData.CurrentPlayer; set => r_GameData.CurrentPlayer = value; }
    public bool SelectionNotMatching { get; set; } = false;
    private Cell AiSelection { get; set; }
    private Cell CurrentUserSelection { get; set; }
    private Cell PreviousUserSelection{ get; set; }

    private readonly eGameModes r_GameMode;
    private readonly GameData r_GameData;
    private readonly  Dictionary<Cell, char>? r_AiMemory;


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

    public void ChangeTurn() {
        CurrentPlayer = CurrentPlayer == r_GameData.PlayerOne ? r_GameData.PlayerTwo : r_GameData.PlayerOne;

        GetBoardLetterAt(CurrentUserSelection).IsRevealed = false;
        GetBoardLetterAt(PreviousUserSelection).IsRevealed = false;
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
        CurrentUserSelection = i_UserSelection;

        if (r_GameMode == eGameModes.singlePlayer)
        {
            if (GameData.GetRandomNumber(0, 100) <= Difficulty)
            {
                AddToAiMemory(CurrentUserSelection);
            }
        }

        if (IsFirstSelection)
        {
            PreviousUserSelection = CurrentUserSelection;
            GetBoardLetterAt(CurrentUserSelection).IsRevealed = true;
            IsFirstSelection = false;
        } else {
            BoardLetter firstSelectionLetter = GetBoardLetterAt(PreviousUserSelection);
            BoardLetter secondSelectionLetter = GetBoardLetterAt(CurrentUserSelection);

            secondSelectionLetter.IsRevealed = true;

            SelectionNotMatching = firstSelectionLetter.Letter != secondSelectionLetter.Letter;

            if (!SelectionNotMatching) {
                if (r_GameMode == eGameModes.singlePlayer) {
                    r_AiMemory?.Remove(CurrentUserSelection);
                    r_AiMemory?.Remove(PreviousUserSelection);
                }
                CurrentPlayer.PlayerScore++;
            }
            IsFirstSelection = true;
        }
    }

    private void AddToAiMemory(Cell i_CellToBeAdded)
    {
        if(r_AiMemory != null && !r_AiMemory.ContainsKey(i_CellToBeAdded))
        {
            r_AiMemory.Add(i_CellToBeAdded, Letters[i_CellToBeAdded.Row, i_CellToBeAdded.Column].Letter);
        }
    }

    private BoardLetter GetBoardLetterAt(Cell i_CellLocation)
    {
        return Letters[i_CellLocation.Row, i_CellLocation.Column];
    }

    public string GetAiInput()
     {
        string aiSelection;

        if(r_AiMemory?.Count == 0)
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
        string firstSelection = string.Empty;

        FoundMatch = FindLetterMatch(ref firstSelection);

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
            secondSelection = AiSelection.ToString();
        } else {
            secondSelection = FindLetterInMemory(AiSelection);
            if(secondSelection != "")
            {
                AiHasMatches = true;
            } else {
                AiHasMatches = false;
                secondSelection = GetRandomUnmemorizedCell();
            }
        }
        return secondSelection;
     }
    
    private string FindLetterInMemory(Cell i_FirstSelectionCell)
    {
        string foundLetter = string.Empty;

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
        AiSelection = cellsNotInMemory[indexOfCellNotInMemory];
        return AiSelection.ToString();
    }

    private bool FindLetterMatch(ref string i_MemorizedMatchingLetter)
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
                        AiSelection = secondMemorizedLetter.Key;
                        foundMatch = true;
                    }
                }
             }
         }
        return foundMatch;
    }

    public bool ValidatePlayerInput(string? i_userInput) {
        bool isValid = false;
        if (i_userInput == null) {
            Console.WriteLine("Input must not be empty");
            return isValid;
        }
        i_userInput = i_userInput.ToUpper();
        return i_userInput != "Q" && ValidateCellSelection(i_userInput) && ValidateCellIsHidden(i_userInput);
    }

    private bool ValidateCellSelection(string i_userInput) {
        if(i_userInput.Length != 2)
        {
            Console.WriteLine("Input must have exactly 2 characters");
            return false;
        }
        return CheckIfLetterInRange(i_userInput[0]) && CheckIfDigitInRange(i_userInput[1]);
    }

    private bool CheckIfLetterInRange(char i_Letter) {
        char lastLetter = (char)('A' + BoardWidth - 1);
        if (i_Letter < 'A' || i_Letter > lastLetter) {
            Console.WriteLine("Invalid input, letter must be between A and {0}", lastLetter);
            return false;
        }
        return true;
    }

    private bool CheckIfDigitInRange(char i_Digit) {
        char lastDigit = (char)('0' + BoardHeight);
        if (i_Digit < '1' || i_Digit > lastDigit) {
            Console.WriteLine("Invalid input, digit must be between 1 and {0}", lastDigit);
            return false;
        }
        return true;
    }

    private bool ValidateCellIsHidden(string i_userInput) {
        int column = i_userInput[0] - 'A';
        int row = i_userInput[1] - '1';
        bool isValid = !r_GameData.Letters[row, column].IsRevealed;
        if (!isValid)
        {
           Console.WriteLine("Cell {0} is already revealed", i_userInput);
        }
        return isValid;
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

    public string GetGameOverStatus() {
        Player playerOne = r_GameData.PlayerOne;
        Player playerTwo = r_GameData.PlayerTwo;

        string gameResult = playerOne.PlayerScore.CompareTo(playerTwo.PlayerScore) switch
        {
            > 0 => GetGameResultText(playerOne.PlayerName),
            < 0 => GetGameResultText(playerTwo.PlayerName),
            _ => GetGameResultText(null),
        };

        return gameResult;
    }

    private string GetGameResultText(string? i_WinningPlayer)
    {
        string gameResult = i_WinningPlayer == null ? "It's a tie!" : $"{i_WinningPlayer} wins!";
        return $"{gameResult}\n{GetScoreboard()}";
    }

    public void ResetGame(int i_Height, int i_Width) {

        CurrentPlayer = r_GameData.PlayerOne.PlayerScore.CompareTo(r_GameData.PlayerTwo.PlayerScore) > 0
                        ? r_GameData.PlayerOne
                        : r_GameData.PlayerTwo;

        r_GameData.PlayerOne.PlayerScore = 0;
        r_GameData.PlayerTwo.PlayerScore = 0;

        r_GameData.BoardHeight = i_Height;
        r_GameData.BoardWidth = i_Width;

        r_GameData.Letters = new BoardLetter[i_Height, i_Width];
        r_GameData.InitializeBoard();

        InitializeLogic();

        CurrentGameState = eGameStates.Running;
    }

    private void InitializeLogic()
    {
        IsFirstSelection = true;
        SelectionNotMatching = false;
        AiHasMatches = false;
        FoundMatch = false;
        if (r_GameMode == eGameModes.singlePlayer)
        {
            r_AiMemory?.Clear();
        }
    }
    
}