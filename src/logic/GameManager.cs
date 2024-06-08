namespace MemoryGame {
    internal class GameManager()
    {
        public static eGameStates CurrentGameState { get; set; } = eGameStates.Menu;
        public bool IsCurrentPlayerHuman => CurrentPlayer.Type == ePlayerTypes.Human;
        private Queue<Player> TurnsOrder { get; set; } =  new Queue<Player>();
        public Player CurrentPlayer { get; set; } = new Player("AI", ePlayerTypes.AI);
        private AI? AI { get; set; }
        public bool IsAiHasMatches => AI!.HasMatches;
        public List<Cell> Choices { get;} = [];
        private bool IsBoardFinished => Choices.Count == 0;
        private List<Player> Players { get;} = [];
        private static readonly Random m_Random = new Random();

        public void Initialize(List<Player> i_Players,int i_BoardHeight, int i_BoardWidth, int? i_Difficulty)
        {
            Players.AddRange(i_Players);
            CurrentGameState = eGameStates.OnGoing;
            CurrentPlayer = Players[0];
            TurnsOrder =  new Queue<Player>(Players);
            AI = i_Difficulty != null ? new AI((int)i_Difficulty) : null;
            initializeBoard(i_BoardHeight, i_BoardWidth);
        }
        
        public void Update(Cell i_UserSelection, bool i_IsAMatch)
        {       
            updateAiMemoryIfNeeded(i_UserSelection, i_IsAMatch);
            if (IsBoardFinished)
            {
                CurrentGameState = eGameStates.Ended;
            }
        }

        private void updateAiMemoryIfNeeded(Cell i_UserSelection, bool i_IsAMatch)
        {
            Cell cell = Choices.FirstOrDefault(cell => cell.Row == i_UserSelection.Row && cell.Column == i_UserSelection.Column)!;
            if (i_IsAMatch)
            {
                Choices.Remove(cell);
                AI?.ForgetCell(cell);
            }
            else if (GetRandomNumber(0, 100) <= AI?.DifficultyLevel)
            {
                AI?.RememberCell(cell);
            }
        }

        private Player getNextPlayer()
        {
            TurnsOrder.Enqueue(TurnsOrder.Dequeue());
            return TurnsOrder.Peek();
        }
        
        public void ResetGame(int i_BoardHeight, int i_BoardWidth) {

            createNewTurnsOrder();
            CurrentPlayer = TurnsOrder.Peek();
            Players.ForEach(player => player.Score = 0);
            CurrentGameState = eGameStates.OnGoing;

            AI?.ResetMemory();
            initializeBoard(i_BoardHeight, i_BoardWidth);
        }

        private void createNewTurnsOrder()
        {
            Player Winner = Players.MaxBy(player => player.Score)!;
            int winnerIndex = Players.FindIndex(player => player == Winner);

            TurnsOrder =  new Queue<Player>([Winner, ..Players.Skip(winnerIndex+1).Concat(Players.Take(winnerIndex)).ToList()]);
        }
 
        private void getRandomCell(List<Cell> i_RandomCells, out Cell o_Cell)
        {
            int randomSelection = GetRandomNumber(0, i_RandomCells.Count);
            o_Cell = i_RandomCells[randomSelection];
            i_RandomCells.RemoveAt(randomSelection);
        }
            
        private List<Cell> getRandomCellsList(int i_Width, int i_Height)
        {
            return Enumerable.Range(0, i_Height)
                    .SelectMany(row => Enumerable.Range(0, i_Width), (row, col) => new Cell(row, col))
                    .ToList();
        }
            
        private void initializeBoard(int i_BoardHeight, int i_BoardWidth)
        {
            List<Cell> randomCells = getRandomCellsList(i_BoardWidth, i_BoardHeight);

            while (randomCells.Count > 0) {

                getRandomCell(randomCells, out Cell firstCell);
                getRandomCell(randomCells, out Cell secondCell);

                firstCell.MatchCell = secondCell;
                secondCell.MatchCell = firstCell;

                Choices.AddRange([firstCell, secondCell]);
            }
        }
    
        public bool IsBoardValid(int i_Width, int i_Height) => i_Width * i_Height % 2 == 0;

        public List<Player> GetPlayersOrderByScore() => [.. Players.OrderByDescending( player => player.Score)];
    
        public Cell GetAiInput(bool i_IsFirstSelection) => AI!.MakeSelection(Choices , i_IsFirstSelection);
   
        public void ChangeTurn() => CurrentPlayer = getNextPlayer();
   
        public static int GetRandomNumber(int i_RangeStart, int i_RangeEnd) => m_Random.Next(i_RangeStart, i_RangeEnd);

    }
}