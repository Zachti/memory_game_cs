namespace MemoryGame {
    internal record GameDataInput(Player PlayerOne, Player PlayerTwo, Board Board);

    internal interface IGameData
    {
        public Board Board { get; set; }
        public Player PlayerOne { get; set;}
        public Player PlayerTwo { get; set;}
        public Player CurrentPlayer { get; set; }
        public BoardLetter[,] Letters { get; set; }
        void InitializeBoard();
    }

    internal class GameData(GameDataInput i_Dto) : IGameData
    {
        private static readonly Random m_Random = new();
        public Player PlayerOne { get; set; } = i_Dto.PlayerOne;
        public Player PlayerTwo { get; set; } = i_Dto.PlayerTwo;
        public Player CurrentPlayer { get; set; } = i_Dto.PlayerOne;
        public BoardLetter[,] Letters { get; set; } = new BoardLetter[i_Dto.Board.Height, i_Dto.Board.Width];
        public Board Board { get; set; } = i_Dto.Board;

        public void InitializeBoard()
        {
            char[] boardLetters = initializeBoardLetters();
            List<Cell> randomCells = getRandomCellsList();

            foreach(char letter in boardLetters)
            {
                getRandomCell(randomCells, out Cell firstCell);
                getRandomCell(randomCells, out Cell secondCell);

                insertLetterToBoard(letter, firstCell);
                insertLetterToBoard(letter, secondCell);
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
    
        private void insertLetterToBoard(char i_Letter, Cell i_Cell) => Letters[i_Cell.Row, i_Cell.Column] = new BoardLetter(i_Letter);

        public static int GetRandomNumber(int i_RangeStart, int i_RangeEnd) => m_Random.Next(i_RangeStart, i_RangeEnd);
    }
}