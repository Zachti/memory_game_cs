namespace MemoryGame {
    internal record GameDataInput(List<Player> Players, Board Board);

    internal interface IGameData
    {
        public Board Board { get; set; }
        public List<Player> Players { get; set;}
        public Queue<Player> TurnsOrder { get; set; }   
        void InitializeBoard();
        Player GetNextPlayer();
        void CreateNewTurnsOrder();
    }

    internal class GameData(GameDataInput i_Dto) : IGameData
    {
        private static readonly Random m_Random = new Random();
        public List<Player> Players { get; set; } = i_Dto.Players;
        public Queue<Player> TurnsOrder { get; set; } = new Queue<Player>(i_Dto.Players);
        public Board Board { get; set; } = i_Dto.Board;

        public void InitializeBoard()
        {
            char[] boardLetters = initializeBoardLetters();
            List<Cell> randomCells = getRandomCellsList();

            foreach(char letter in boardLetters)
            {
                getRandomCell(randomCells, out Cell firstCell);
                getRandomCell(randomCells, out Cell secondCell);

                Board.InsertLetterToBoard(letter, firstCell);
                Board.InsertLetterToBoard(letter, secondCell);
            }
        }

        private void getRandomCell(List<Cell> i_RandomCells, out Cell o_Cell)
        {
            int randomSelection = GetRandomNumber(0, i_RandomCells.Count);
            o_Cell = i_RandomCells[randomSelection];
            i_RandomCells.RemoveAt(randomSelection);
        }
            
        private List<Cell> getRandomCellsList()
        {
            return Enumerable.Range(0, Board.Height)
                    .SelectMany(row => Enumerable.Range(0, Board.Width), (row, col) => new Cell(row, col))
                    .ToList();
        }
            
        private char[] initializeBoardLetters()
        {
            return Enumerable.Range(0, Board.Height * Board.Width / 2)
                    .Select(i => (char)('A' + i))
                    .ToArray();
        }

        public static int GetRandomNumber(int i_RangeStart, int i_RangeEnd) => m_Random.Next(i_RangeStart, i_RangeEnd);
    
        public Player GetNextPlayer()
        {
            TurnsOrder.Enqueue(TurnsOrder.Dequeue());
            return TurnsOrder.Peek();
        }

        public void CreateNewTurnsOrder()
        {
            Player Winner = Players.MaxBy(player => player.Score)!;
            int winnerIndex = Players.FindIndex(player => player == Winner);

            TurnsOrder = new Queue<Player>(Players
                .Take(winnerIndex)
                .Concat(Players.Skip(winnerIndex))
                .ToList());
            TurnsOrder.Enqueue(TurnsOrder.Dequeue());
        }
    }
}