public class GameManager
{
    private static int? Difficulty { get; set; }
    public static int MinBoardWidth { get; } = 4;
    public static int MaxBoardWidth { get; } = 6;
    public static int MinBoardHeight { get; } = 4;
    public static int MaxBoardHeight { get; } = 6;
    public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
    private bool FoundMatch { get; set; } = false;
    private bool IsFirstSelection { get; set; } = true;
    public bool AiHasMatches { get; set; } = false;
    public int BoardWidth => IGameData.Board.Width;
    public int BoardHeight => IGameData.Board.Height;
    public BoardLetter[,] Letters => IGameData.Letters;
    public Player CurrentPlayer { get => IGameData.CurrentPlayer; set => IGameData.CurrentPlayer = value; }
    public bool SelectionNotMatching { get; set; } = false;
    private Cell AiSelection { get; set; }
    private Cell CurrentUserSelection { get; set; }
    private Cell PreviousUserSelection{ get; set; }
    private IGameMode IGameMode { get; }
    private IGameData IGameData { get; }
    private  Dictionary<Cell, char>? AiMemory { get; set; }


    public GameManager(IGameData i_GameData, IGameMode i_GameMode)
    {
        IGameData = i_GameData;
        IGameMode = i_GameMode;
    }

    public void Initializae(Player i_PlayerOne, Player i_PlayerTwo, Board i_Board, eGameModes i_GameMode, int? i_Difficulty)
    {
        IGameData.PlayerOne = i_PlayerOne;
        IGameData.PlayerTwo = i_PlayerTwo;
        IGameData.Board = i_Board;
        IGameData.CurrentPlayer = i_PlayerOne;
        IGameData.Letters = new BoardLetter[i_Board.Height, i_Board.Width];
        IGameData.InitializeBoard();
        CurrentGameState = eGameStates.Running;
        IGameMode.Mode = i_GameMode;
        if (IGameMode.Mode == eGameModes.singlePlayer)
        {
            Difficulty = i_Difficulty;
            AiMemory = new Dictionary<Cell, char>();
        }
    }

    public void ChangeTurn() {
        CurrentPlayer = CurrentPlayer == IGameData.PlayerOne ? IGameData.PlayerTwo : IGameData.PlayerOne;

        GetBoardLetterAt(CurrentUserSelection).IsRevealed = false;
        GetBoardLetterAt(PreviousUserSelection).IsRevealed = false;
        SelectionNotMatching = false;
    }

    public void Update(Cell i_UserSelection)
    {       
        if(!SelectionNotMatching) {
                UpdateNextTurn(i_UserSelection);
        }
        if ((IGameData.PlayerOne.PlayerScore + IGameData.PlayerTwo.PlayerScore) == (BoardWidth * BoardHeight) / 2)
        {
            CurrentGameState = eGameStates.GameOver;
        }
    }

    private void UpdateNextTurn(Cell i_UserSelection)
    {
        CurrentUserSelection = i_UserSelection;

        if (IGameMode.Mode == eGameModes.singlePlayer)
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
                if (IGameMode.Mode == eGameModes.singlePlayer) {
                    AiMemory?.Remove(CurrentUserSelection);
                    AiMemory?.Remove(PreviousUserSelection);
                }
                CurrentPlayer.PlayerScore++;
            }
            IsFirstSelection = true;
        }
    }

    private void AddToAiMemory(Cell i_CellToBeAdded)
    {
        if(AiMemory != null && !AiMemory.ContainsKey(i_CellToBeAdded))
        {
            AiMemory.Add(i_CellToBeAdded, Letters[i_CellToBeAdded.Row, i_CellToBeAdded.Column].Letter);
        }
    }

    private BoardLetter GetBoardLetterAt(Cell i_CellLocation)
    {
        return Letters[i_CellLocation.Row, i_CellLocation.Column];
    }

    public string GetAiInput()
     {
        string aiSelection;

        if(AiMemory?.Count == 0)
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

        foreach(var memorizedLetter in AiMemory!)
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
        Cell[] cellsNotInMemory = new Cell[(BoardHeight * BoardWidth) - AiMemory!.Count];
        int indexOfCellNotInMemory = 0;

        for (int i = 0; i < row; i++)
        {
            for(int j = 0; j < column; j++)
            {
                if(!Letters[i, j].IsRevealed)
                {
                    if(!AiMemory.ContainsKey(new Cell(i, j)))
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

        foreach (var firstMemorizedLetter in AiMemory!)
        {
            foreach (var secondMemorizedLetter in AiMemory)
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
        return i_userInput == "Q" || (ValidateCellSelection(i_userInput) && ValidateCellIsHidden(i_userInput));
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
        bool isValid = !IGameData.Letters[row, column].IsRevealed;
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
            IGameData.PlayerOne.PlayerName,
            IGameData.PlayerOne.PlayerScore,
            IGameData.PlayerTwo.PlayerName,
            IGameData.PlayerTwo.PlayerScore );
    }

    public string GetGameOverStatus() {
        Player playerOne = IGameData.PlayerOne;
        Player playerTwo = IGameData.PlayerTwo;

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

        CurrentPlayer = IGameData.PlayerOne.PlayerScore.CompareTo(IGameData.PlayerTwo.PlayerScore) > 0
                        ? IGameData.PlayerOne
                        : IGameData.PlayerTwo;

        IGameData.PlayerOne.PlayerScore = 0;
        IGameData.PlayerTwo.PlayerScore = 0;

        IGameData.Board.Height = i_Height;
        IGameData.Board.Width = i_Width;

        IGameData.Letters = new BoardLetter[i_Height, i_Width];
        IGameData.InitializeBoard();

        InitializeLogic();

        CurrentGameState = eGameStates.Running;
    }

    private void InitializeLogic()
    {
        IsFirstSelection = true;
        SelectionNotMatching = false;
        AiHasMatches = false;
        FoundMatch = false;
        if (IGameMode.Mode == eGameModes.singlePlayer)
        {
            AiMemory?.Clear();
        }
    }
    
}