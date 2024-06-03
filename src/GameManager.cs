namespace MemoryGame {
    internal record GameManagerInput(IGameData i_GameData, eGameModes i_GameMode);
    
    internal class GameManager(GameManagerInput i_Dto)
    {
        private static int? Difficulty { get; set; }
        public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
        private bool IsFirstSelection { get; set; } = true;
        public int BoardWidth => IGameData.Board.Width;
        public int BoardHeight => IGameData.Board.Height;
        public Board Board => IGameData.Board;
        public Player CurrentPlayer { get => IGameData.CurrentPlayer; set => IGameData.CurrentPlayer = value; }
        public bool IsSelectionNotMatching { get; set; }
        public bool IsCurrentPlayerHuman => CurrentPlayer.Type == ePlayerTypes.Human;
        private Cell CurrentUserSelection { get; set; }
        private Cell PreviousUserSelection{ get; set; }
        private eGameModes SelectedMode { get; set; } = i_Dto.i_GameMode;
        private IGameData IGameData { get; } = i_Dto.i_GameData;
        private AI? AI { get; set; }
        public bool IsAiHasMatches => AI!.HasMatches;


        public void Initialize(Player i_PlayerOne, Player i_PlayerTwo, Board i_Board, eGameModes i_GameMode, int? i_Difficulty)
        {
            List<Task> tasks = [
                Task.Run(() => initializeMode(i_GameMode, i_Difficulty)),
                Task.Run(() => initializeGameData(i_PlayerOne, i_PlayerTwo, i_Board))
            ];
            Task.WaitAll([.. tasks]);
        }

        private void initializeMode(eGameModes i_GameMode, int? i_Difficulty) {
            CurrentGameState = eGameStates.OnGoing;
            SelectedMode = i_GameMode;
            Difficulty = i_Difficulty;
            AI = Difficulty != null ? new AI() : null;
        }
        
        private void initializeGameData(Player i_PlayerOne, Player i_PlayerTwo, Board i_Board) {
            IGameData.PlayerOne = IGameData.CurrentPlayer = i_PlayerOne;
            IGameData.PlayerTwo = i_PlayerTwo;
            IGameData.Board = i_Board;
            IGameData.InitializeBoard();
        }
        
        public void ChangeTurn() 
        {
            CurrentPlayer = CurrentPlayer == IGameData.PlayerOne ? IGameData.PlayerTwo : IGameData.PlayerOne;

            IsSelectionNotMatching = false;
            Board[CurrentUserSelection].Flip();
            Board[PreviousUserSelection].Flip();
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

            List<Task> tasks = [
                Task.Run(updateAiMemoryIfNeeded),
                Task.Run(revealCurrentSelection),
                Task.Run(() => {
                    if (!IsFirstSelection)
                    {
                        checkAndHandleMatch();
                    }
                })
            ];

            Task.WaitAll([.. tasks]);

            IsFirstSelection = !IsFirstSelection;
        }

        private void updateAiMemoryIfNeeded()
        {
            if (SelectedMode == eGameModes.singlePlayer && GameData.GetRandomNumber(0, 100) <= Difficulty)
            {
                char letter = Board[CurrentUserSelection].Letter;
                AI?.RememberCell(CurrentUserSelection, letter);
            }
        }

        private void revealCurrentSelection()
        {
            Board[CurrentUserSelection].Flip();
            if (IsFirstSelection)
            {
                PreviousUserSelection = CurrentUserSelection;
            }
        }

        private void checkAndHandleMatch()
        {
            IsSelectionNotMatching = Board[PreviousUserSelection].Letter != Board[CurrentUserSelection].Letter;

            if (!IsSelectionNotMatching)
            {
                handleMatchFound();
            }
        }

        private void handleMatchFound()
        {
            List<Task> tasks = [
                Task.Run(() => AI?.ForgetCell(CurrentUserSelection)),
                Task.Run(() => AI?.ForgetCell(PreviousUserSelection))
            ];

            Task.WaitAll([.. tasks]);
            CurrentPlayer.Score++;
        }
        
        public bool ValidateCellIsHidden(string i_userInput) {
            int column = i_userInput[0] - 'A';
            int row = i_userInput[1] - '1';
            bool isInvalid = Board.Letters[row, column].IsRevealed;
            if (isInvalid)
            {
            Console.WriteLine($"Cell {i_userInput} is already revealed");
            }
            return !isInvalid;
        }

        public string GetGameOverStatus() {

            return IGameData.PlayerOne.Score.CompareTo(IGameData.PlayerTwo.Score) switch
            {
                > 0 => getGameResultText(IGameData.PlayerOne.Name),
                < 0 => getGameResultText(IGameData.PlayerTwo.Name),
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
            IGameData.Board = new Board(i_Width, i_Height);

            List<Task> tasks = [
                Task.Run(() => AI?.ResetMemory()),
                Task.Run(initializeLogic),
                Task.Run(IGameData.InitializeBoard)
            ];

            Task.WaitAll([.. tasks]);
        }

        private void initializeLogic()
        {
            IsFirstSelection = true;
            IsSelectionNotMatching = false;
            CurrentGameState = eGameStates.OnGoing;
        }
        
        private string getScoreboard() =>
            $"Score: {IGameData.PlayerOne.Name} {IGameData.PlayerOne.Score} - {IGameData.PlayerTwo.Name} {IGameData.PlayerTwo.Score}";
    
        public string GetAiInput() => AI!.MakeSelection(Board.Letters , IsFirstSelection);
    }
}