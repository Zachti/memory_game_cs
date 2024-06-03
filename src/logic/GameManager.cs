using System.Text;

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
        public Player CurrentPlayer { get; set; } = i_Dto.i_GameData.Players.First();
        public bool IsSelectionNotMatching { get; set; }
        public bool IsCurrentPlayerHuman => CurrentPlayer.Type == ePlayerTypes.Human;
        private Cell CurrentUserSelection { get; set; }
        private Cell PreviousUserSelection{ get; set; }
        private eGameModes SelectedMode { get; set; } = i_Dto.i_GameMode;
        private IGameData IGameData { get; } = i_Dto.i_GameData;
        private AI? AI { get; set; }
        public bool IsAiHasMatches => AI!.HasMatches;


        public void Initialize(List<Player> i_players, Board i_Board, eGameModes i_GameMode, int? i_Difficulty)
        {
           Task.WaitAll([
                Task.Run(() => initializeMode(i_GameMode, i_Difficulty)),
                Task.Run(() => initializeGameData(i_players, i_Board))
            ]);
        }

        private void initializeMode(eGameModes i_GameMode, int? i_Difficulty) {
            CurrentGameState = eGameStates.OnGoing;
            SelectedMode = i_GameMode;
            Difficulty = i_Difficulty;
            AI = Difficulty != null ? new AI() : null;
        }
        
        private void initializeGameData(List<Player> i_players, Board i_Board) {
            IGameData.Players = i_players;
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
            Task.WaitAll([
                Task.Run(() => AI?.ForgetCell(CurrentUserSelection)),
                Task.Run(() => AI?.ForgetCell(PreviousUserSelection))
            ]);

            CurrentPlayer.Score++;
            Board.IncrementRevealedSquaresCounter();
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

            Player winner = IGameData.Players.MaxBy(player => player.Score)!;
            bool isTie = IGameData.Players.Any(player => player.Score == winner.Score && player != winner);


            return isTie ? getGameResultText(null) : getGameResultText(winner.Name);
        }

        private string getGameResultText(string? i_WinningPlayer)
        {
            string gameResult = i_WinningPlayer == null ? "It's a tie!" : $"{i_WinningPlayer} wins!";
            return $"{gameResult}\n{getScoreboard()}";
        }

        public void ResetGame(int i_Height, int i_Width) {

            IGameData.CreateNewTurnsOrder();
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
        
        private string getScoreboard()
        {
        StringBuilder scoreboard = new StringBuilder("Scoreboard:\n");

        foreach (var player in IGameData.Players)
        {
            scoreboard.AppendLine($"{player.Name}: {player.Score}");
        }

        return scoreboard.ToString();
        }
        
        public string GetAiInput() => AI!.MakeSelection(Board.Letters , IsFirstSelection);
    }
}