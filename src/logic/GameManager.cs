namespace MemoryGame {
    internal class GameManager(IGameData i_GameData)
    {
        public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
        private bool IsFirstSelection { get; set; } = true;
        public int BoardWidth => IGameData.Board.Width;
        public int BoardHeight => IGameData.Board.Height;
        public Board Board => IGameData.Board;
        public Player CurrentPlayer { get; set; } = i_GameData.Players.First();
        public bool IsSelectionNotMatching { get; set; }
        public bool IsCurrentPlayerHuman => CurrentPlayer.Type == ePlayerTypes.Human;
        private Cell CurrentUserSelection { get; set; }
        private Cell PreviousUserSelection{ get; set; }
        private IGameData IGameData { get; } = i_GameData;
        private AI? AI { get; set; }
        public bool IsAiHasMatches => AI!.HasMatches;

        public void Initialize(List<Player> i_Players, Board i_Board, int? i_Difficulty)
        {
           Task.WaitAll([
                Task.Run(() => initializeMode(i_Difficulty)),
                Task.Run(() => initializeGameData(i_Players, i_Board))
            ]);
        }

        private void initializeMode(int? i_Difficulty) {
            CurrentGameState = eGameStates.OnGoing;
            AI = i_Difficulty != null ? new AI((int)i_Difficulty) : null;
        }
        
        private void initializeGameData(List<Player> i_Players, Board i_Board) {
            IGameData.Players = i_Players;
            IGameData.InitializeBoard(i_Board);
            IGameData.TurnsOrder = new Queue<Player>(IGameData.Players);
            CurrentPlayer = IGameData.TurnsOrder.Peek();
        }
        
        public void ChangeTurn() 
        {
            CurrentPlayer = IGameData.GetNextPlayer();

            IsSelectionNotMatching = false;
            Board[CurrentUserSelection].Flip();
            Board[PreviousUserSelection].Flip();
        }

        public void Update(Cell i_UserSelection)
        {       
            if(!IsSelectionNotMatching) {
                updateNextTurn(i_UserSelection);
            }
            if (Board.IsBoardFinished)
            {
                CurrentGameState = eGameStates.Ended;
            }
        }

        private void updateNextTurn(Cell i_UserSelection)
        {
            CurrentUserSelection = i_UserSelection;

            Task.WaitAll([
                Task.Run(updateAiMemoryIfNeeded),
                Task.Run(revealCurrentSelection),
                Task.Run(() => {
                    if (!IsFirstSelection)
                    {
                        checkAndHandleMatch();
                    }
                })
            ]);

            IsFirstSelection = !IsFirstSelection;
        }

        private void updateAiMemoryIfNeeded()
        {
            if (GameData.GetRandomNumber(0, 100) <= AI?.DifficultyLevel)
            {
                char symbol = Board[CurrentUserSelection].Symbol;
                AI?.RememberCell(CurrentUserSelection, symbol);
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
            IsSelectionNotMatching = Board[PreviousUserSelection].Symbol != Board[CurrentUserSelection].Symbol;

            if (!IsSelectionNotMatching)
            {
                handleMatchFound();
            }
        }

        private void handleMatchFound()
        {
            Task.WaitAll([
                Task.Run(() => AI?.ForgetCell(CurrentUserSelection)),
                Task.Run(() => AI?.ForgetCell(PreviousUserSelection))
            ]);

            CurrentPlayer.Score++;
            Board.IncrementRevealedSquaresCounter();
        }

        public void ResetGame(int i_Height, int i_Width) {

            IGameData.CreateNewTurnsOrder();
            CurrentPlayer = IGameData.TurnsOrder.Peek();
            IGameData.Players.ForEach(player => player.Score = 0);

            Task.WaitAll([
                Task.Run(() => AI?.ResetMemory()),
                Task.Run(initializeLogic),
                Task.Run(() => IGameData.InitializeBoard(new Board(i_Width, i_Height)))
            ]);
        }

        private void initializeLogic()
        {
            IsFirstSelection = true;
            IsSelectionNotMatching = false;
            CurrentGameState = eGameStates.OnGoing;
        }

        public List<Player> GetPlayersOrderByScore() => [.. IGameData.Players.OrderByDescending( player => player.Score)];
    
        public string GetAiInput() => AI!.MakeSelection(Board.Cards , IsFirstSelection);

        public bool IsCellRevealed(Cell i_Cell) => Board[i_Cell].IsRevealed;
    }
}