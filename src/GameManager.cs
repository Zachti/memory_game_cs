namespace MemoryGame {
    internal class GameManager(IGameData i_GameData, IGameMode i_GameMode)
    {
        private static int? Difficulty { get; set; }
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
        private IGameMode IGameMode { get; } = i_GameMode;
        private IGameData IGameData { get; } = i_GameData;
        private  Dictionary<Cell, char>? AiMemory { get; set; } = [];

        public void Initialize(Player i_PlayerOne, Player i_PlayerTwo, Board i_Board, eGameModes i_GameMode, int? i_Difficulty)
        {
            IGameData.PlayerOne = IGameData.CurrentPlayer = i_PlayerOne;
            IGameData.PlayerTwo = i_PlayerTwo;
            IGameData.Board = i_Board;
            IGameData.Letters = new BoardLetter[i_Board.Height, i_Board.Width];
            IGameData.InitializeBoard();
            CurrentGameState = eGameStates.OnGoing;
            IGameMode.Mode = i_GameMode;
            if (IGameMode.Mode == eGameModes.singlePlayer)
            {
                Difficulty = i_Difficulty;
            }
        }

        public void ChangeTurn() {
            CurrentPlayer = CurrentPlayer == IGameData.PlayerOne ? IGameData.PlayerTwo : IGameData.PlayerOne;

            SelectionNotMatching = getBoardLetterAt(CurrentUserSelection).IsRevealed = getBoardLetterAt(PreviousUserSelection).IsRevealed =false;
        }

        public void Update(Cell i_UserSelection)
        {       
            if(!SelectionNotMatching) {
                updateNextTurn(i_UserSelection);
            }
            if (IGameData.PlayerOne.PlayerScore + IGameData.PlayerTwo.PlayerScore == BoardWidth * BoardHeight / 2)
            {
                CurrentGameState = eGameStates.Ended;
            }
        }

        private void updateNextTurn(Cell i_UserSelection)
        {
            CurrentUserSelection = i_UserSelection;

            if (IGameMode.Mode == eGameModes.singlePlayer && GameData.GetRandomNumber(0, 100) <= Difficulty)
            {
                    addToAiMemory(CurrentUserSelection);
            }

            if (IsFirstSelection)
            {
                PreviousUserSelection = CurrentUserSelection;
                getBoardLetterAt(CurrentUserSelection).IsRevealed = true;
            } 
            else 
            {
                BoardLetter firstSelectionLetter = getBoardLetterAt(PreviousUserSelection);
                BoardLetter secondSelectionLetter = getBoardLetterAt(CurrentUserSelection);

                secondSelectionLetter.IsRevealed = true;

                SelectionNotMatching = firstSelectionLetter.Letter != secondSelectionLetter.Letter;

                if (!SelectionNotMatching) {
                    if (IGameMode.Mode == eGameModes.singlePlayer) {
                        AiMemory?.Remove(CurrentUserSelection);
                        AiMemory?.Remove(PreviousUserSelection);
                    }
                    CurrentPlayer.PlayerScore++;
                }
            }
            IsFirstSelection = !IsFirstSelection;
        }

        private void addToAiMemory(Cell i_CellToBeAdded)
        {
            if(AiMemory != null && !AiMemory.ContainsKey(i_CellToBeAdded))
            {
                AiMemory.Add(i_CellToBeAdded, Letters[i_CellToBeAdded.Row, i_CellToBeAdded.Column].Letter);
            }
        }

        public string GetAiInput()
        {
            bool isMemoryEmpty = AiMemory?.Count == 0;

            AiHasMatches = isMemoryEmpty ? false : AiHasMatches;
            return isMemoryEmpty
                ? getRandomUnmemorizedCell()
                : (IsFirstSelection ? getFirstSelection() : getSecondSelection());
        }
        
        private string getFirstSelection()
        {
            string firstSelection = string.Empty;

            AiHasMatches = FoundMatch = findLetterMatch(ref firstSelection);
            return FoundMatch ? firstSelection : getRandomUnmemorizedCell();
        }

        private string getSecondSelection()
        {
            string secondSelection = FoundMatch ? AiSelection.ToString() : findLetterInMemory(AiSelection);
            AiHasMatches = secondSelection != "";
            return AiHasMatches ? secondSelection : getRandomUnmemorizedCell();
        }
        
        private string findLetterInMemory(Cell i_FirstSelectionCell)
        {
            char firstSelectionLetter = Letters[i_FirstSelectionCell.Row, i_FirstSelectionCell.Column].Letter;

            return AiMemory!
                .Where(memorizedLetter => 
                    !memorizedLetter.Key.Equals(i_FirstSelectionCell) &&
                    memorizedLetter.Value == firstSelectionLetter)
                .Select(memorizedLetter => memorizedLetter.Key.ToString())
                .FirstOrDefault() 
                ?? string.Empty;
        }

        private string getRandomUnmemorizedCell()
        {
            List<Cell> cellsNotInMemory = Enumerable.Range(0, Letters.GetLength(0))
                .SelectMany(row => Enumerable.Range(0, Letters.GetLength(1)), (row, column) => new { row, column })
                .Where(cell => !(Letters[cell.row, cell.column].IsRevealed || AiMemory!.ContainsKey(new Cell(cell.row, cell.column))))
                .Select(cell => new Cell(cell.row, cell.column))
                .ToList();
            int randomIndex = GameData.GetRandomNumber(0, cellsNotInMemory.Count);
            AiSelection = cellsNotInMemory[randomIndex];
            return AiSelection.ToString();
        }

        private bool findLetterMatch(ref string i_MemorizedMatchingLetter)
        {
            bool foundMatch = false;

            foreach (var firstMemorizedLetter in AiMemory!)
            {
                KeyValuePair<Cell, char> matchingLetter = AiMemory
                    .FirstOrDefault(secondMemorizedLetter =>
                        !firstMemorizedLetter.Key.Equals(secondMemorizedLetter.Key) &&
                        firstMemorizedLetter.Value == secondMemorizedLetter.Value);

                if (!matchingLetter.Equals(default(KeyValuePair<Cell, char>)))
                {
                    i_MemorizedMatchingLetter = firstMemorizedLetter.Key.ToString();
                    AiSelection = matchingLetter.Key;
                    foundMatch = true;
                    break;
                }
            }
            return foundMatch;
        }

        public bool ValidatePlayerInput(string? i_userInput) {
            bool isInvalid = i_userInput == null;
            if (isInvalid) {
                Console.WriteLine("Input must not be empty");
            }
            i_userInput = i_userInput.ToUpper();
            return !isInvalid && ( i_userInput == "Q" || ( validateCellSelection(i_userInput) && validateCellIsHidden(i_userInput) ) );
        }

        private bool validateCellSelection(string i_userInput) {
            bool isInvalid = i_userInput.Length != 2;
            if(isInvalid)
            {
                Console.WriteLine("Input must have exactly 2 characters");
            }
            return !isInvalid && checkIfLetterInRange(i_userInput[0]) && checkIfDigitInRange(i_userInput[1]);
        }

        private bool checkIfLetterInRange(char i_Letter) {
            char lastLetter = (char)('A' + BoardWidth - 1);
            bool isInvalid = i_Letter < 'A' || i_Letter > lastLetter;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, letter must be between A and {lastLetter}");
            }
            return !isInvalid;
        }

        private bool checkIfDigitInRange(char i_Digit) {
            char lastDigit = (char)('0' + BoardHeight);
            bool isInvalid = i_Digit < '1' || i_Digit > lastDigit;
            if (isInvalid) {
                Console.WriteLine($"Invalid input, digit must be between 1 and {lastDigit}");
            }
            return !isInvalid;
        }

        private bool validateCellIsHidden(string i_userInput) {
            int column = i_userInput[0] - 'A';
            int row = i_userInput[1] - '1';
            bool isInvalid = IGameData.Letters[row, column].IsRevealed;
            if (isInvalid)
            {
            Console.WriteLine($"Cell {i_userInput} is already revealed");
            }
            return !isInvalid;
        }

        public string GetGameOverStatus() {
            Player playerOne = IGameData.PlayerOne;
            Player playerTwo = IGameData.PlayerTwo;

            string gameResult = playerOne.PlayerScore.CompareTo(playerTwo.PlayerScore) switch
            {
                > 0 => getGameResultText(playerOne.PlayerName),
                < 0 => getGameResultText(playerTwo.PlayerName),
                _ => getGameResultText(null),
            };

            return gameResult;
        }

        private string getGameResultText(string? i_WinningPlayer)
        {
            string gameResult = i_WinningPlayer == null ? "It's a tie!" : $"{i_WinningPlayer} wins!";
            return $"{gameResult}\n{getScoreboard()}";
        }

        public void ResetGame(int i_Height, int i_Width) {

            CurrentPlayer = IGameData.PlayerOne.PlayerScore.CompareTo(IGameData.PlayerTwo.PlayerScore) > 0
                            ? IGameData.PlayerOne
                            : IGameData.PlayerTwo;

            IGameData.PlayerOne.PlayerScore = IGameData.PlayerTwo.PlayerScore = 0;

            IGameData.Board.Height = i_Height;
            IGameData.Board.Width = i_Width;

            IGameData.Letters = new BoardLetter[i_Height, i_Width];
            IGameData.InitializeBoard();

            initializeLogic();

            CurrentGameState = eGameStates.OnGoing;
        }

        private void initializeLogic()
        {
            IsFirstSelection = true;
            SelectionNotMatching = AiHasMatches = FoundMatch = false;
            if (IGameMode.Mode == eGameModes.singlePlayer)
            {
                AiMemory?.Clear();
            }
        }
        
        private BoardLetter getBoardLetterAt(Cell i_CellLocation) => Letters[i_CellLocation.Row, i_CellLocation.Column];

        private string getScoreboard() =>
            $"Score: {IGameData.PlayerOne.PlayerName} {IGameData.PlayerOne.PlayerScore} - {IGameData.PlayerTwo.PlayerName} {IGameData.PlayerTwo.PlayerScore}";
    }
}