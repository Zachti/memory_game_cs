namespace MemoryGame {
    internal class GameManager(IGameData i_GameData, IGameMode i_GameMode)
    {
        private static int? Difficulty { get; set; }
        public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
        private bool IsFoundMatch { get; set; } = false;
        private bool IsFirstSelection { get; set; } = true;
        public bool IsAiHasMatches { get; set; } = false;
        public int BoardWidth => IGameData.Board.Width;
        public int BoardHeight => IGameData.Board.Height;
        public BoardLetter[,] Letters => IGameData.Letters;
        public Player CurrentPlayer { get => IGameData.CurrentPlayer; set => IGameData.CurrentPlayer = value; }
        public bool IsSelectionNotMatching { get; set; } = false;
        public bool IsCurrentPlayerHuman => CurrentPlayer.Type == ePlayerTypes.Human;
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

            IsSelectionNotMatching = getBoardLetterAt(CurrentUserSelection).IsRevealed = getBoardLetterAt(PreviousUserSelection).IsRevealed =false;
        }

        public void Update(Cell i_UserSelection)
        {       
            if(!IsSelectionNotMatching) {
                updateNextTurn(i_UserSelection);
            }
            if (IGameData.PlayerOne.Score + IGameData.PlayerTwo.Score == BoardWidth * BoardHeight / 2)
            {
                CurrentGameState = eGameStates.Ended;
            }
        }

        private void updateNextTurn(Cell i_UserSelection)
        {
            CurrentUserSelection = i_UserSelection;

            updateAiMemoryIfNeeded();

            revealCurrentSelection();

            if (!IsFirstSelection)
            {
                checkAndHandleMatch();
            }
            
            IsFirstSelection = !IsFirstSelection;
        }

        private void updateAiMemoryIfNeeded()
        {
            if (IGameMode.Mode == eGameModes.singlePlayer && GameData.GetRandomNumber(0, 100) <= Difficulty)
            {
                addToAiMemory(CurrentUserSelection);
            }
        }

        private void revealCurrentSelection()
        {
            getBoardLetterAt(CurrentUserSelection).IsRevealed = true;
            if (IsFirstSelection)
            {
                PreviousUserSelection = CurrentUserSelection;
            }
        }

        private void checkAndHandleMatch()
        {
            BoardLetter firstSelectionLetter = getBoardLetterAt(PreviousUserSelection);
            BoardLetter secondSelectionLetter = getBoardLetterAt(CurrentUserSelection);

            IsSelectionNotMatching = firstSelectionLetter.Letter != secondSelectionLetter.Letter;

            if (!IsSelectionNotMatching)
            {
                handleMatchFound();
            }
        }

        private void handleMatchFound()
        {
            if (IGameMode.Mode == eGameModes.singlePlayer)
            {
                AiMemory?.Remove(CurrentUserSelection);
                AiMemory?.Remove(PreviousUserSelection);
            }
            CurrentPlayer.Score++;
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

            IsAiHasMatches = !isMemoryEmpty && IsAiHasMatches;
            return isMemoryEmpty
                ? getRandomUnmemorizedCell()
                : (IsFirstSelection ? getFirstSelection() : getSecondSelection());
        }
        
        private string getFirstSelection()
        {
            string firstSelection = string.Empty;

            IsAiHasMatches = IsFoundMatch = findLetterMatch(ref firstSelection);
            return IsFoundMatch ? firstSelection : getRandomUnmemorizedCell();
        }

        private string getSecondSelection()
        {
            string secondSelection = IsFoundMatch ? AiSelection.ToString() : findLetterInMemory(AiSelection);
            IsAiHasMatches = secondSelection != "";
            return IsAiHasMatches ? secondSelection : getRandomUnmemorizedCell();
        }
        
        private string findLetterInMemory(Cell i_FirstSelectionCell)
        {
            char firstSelectionLetter = Letters[i_FirstSelectionCell.Row, i_FirstSelectionCell.Column].Letter;

            return AiMemory?
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

        public bool ValidatePlayerInput(string i_userInput) {
            bool isInvalid = i_userInput == string.Empty;
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

            return playerOne.Score.CompareTo(playerTwo.Score) switch
            {
                > 0 => getGameResultText(playerOne.Name),
                < 0 => getGameResultText(playerTwo.Name),
                _ => getGameResultText(null),
            };
        }

        private string getGameResultText(string? i_WinningPlayer)
        {
            string gameResult = i_WinningPlayer == null ? "It's a tie!" : $"{i_WinningPlayer} wins!";
            return $"{gameResult}\n{getScoreboard()}";
        }

        public void ResetGame(int i_Height, int i_Width) {

            CurrentPlayer = IGameData.PlayerOne.Score.CompareTo(IGameData.PlayerTwo.Score) > 0
                            ? IGameData.PlayerOne
                            : IGameData.PlayerTwo;

            IGameData.PlayerOne.Score = IGameData.PlayerTwo.Score = 0;

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
            IsSelectionNotMatching = IsAiHasMatches = IsFoundMatch = false;
            if (IGameMode.Mode == eGameModes.singlePlayer)
            {
                AiMemory?.Clear();
            }
        }
        
        private BoardLetter getBoardLetterAt(Cell i_CellLocation) => Letters[i_CellLocation.Row, i_CellLocation.Column];

        private string getScoreboard() =>
            $"Score: {IGameData.PlayerOne.Name} {IGameData.PlayerOne.Score} - {IGameData.PlayerTwo.Name} {IGameData.PlayerTwo.Score}";
    }
}