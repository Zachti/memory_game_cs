public interface IGameData
{
    public Board Board { get; set; }
    public Player PlayerOne { get; set;}
    public Player PlayerTwo { get; set;}
    Player CurrentPlayer { get; set; }
    public BoardLetter[,] Letters { get; set; }
    void InitializeBoard();
}

public class GameData: IGameData
    {
        private static readonly Random m_Random = new Random();
        public Player PlayerOne { get; set; }
        public Player PlayerTwo { get; set; }
        public Player CurrentPlayer { get; set; }
        public BoardLetter[,] Letters { get; set; }
        public Board Board { get; set; }

        public GameData(Player i_PlayerOne, Player i_PlayerTwo, Board i_Board)
        {
            PlayerOne = i_PlayerOne;
            PlayerTwo = i_PlayerTwo;
            CurrentPlayer = i_PlayerOne;
            Board = i_Board;
            Letters = new BoardLetter[Board.Height, Board.Width];
        }
        
        public void InitializeBoard()
        {
            char[] boardLetters = initializeBoardLetters();
            List<Cell> randomCells = getRandomCellsList();

            foreach(char letter in boardLetters)
            {
                int randomSelection = GetRandomNumber(0, randomCells.Count);
                Cell firstCell = randomCells[randomSelection];
                randomCells.Remove(firstCell);

                randomSelection = GetRandomNumber(0, randomCells.Count);
                Cell secondCell = randomCells[randomSelection];
                randomCells.Remove(secondCell);

                Letters[firstCell.Row, firstCell.Column] = new BoardLetter(letter);
                Letters[secondCell.Row, secondCell.Column] = new BoardLetter(letter);
            }
        }
        
        private List<Cell> getRandomCellsList()
        {
            List<Cell> randomCells = new List<Cell>(Board.Height * Board.Width);

            for (int i = 0; i < Board.Height; i++)
            {
                for (int j = 0; j < Board.Width; j++)
                {
                   randomCells.Add(new Cell(i, j));
                }
            }

            return randomCells;
        }
        
        private char[] initializeBoardLetters()
        {
            char[] boardLetters = new char[Board.Height * Board.Width / 2];

            for(int i = 0; i < boardLetters.Length; i++)
            {
                boardLetters[i] = (char)('A' + i);
            }

            return boardLetters;
        }
  
        public static int GetRandomNumber(int i_RangeStart, int i_RangeEnd)
        { 
            return m_Random.Next(i_RangeStart, i_RangeEnd);
        } 
    }