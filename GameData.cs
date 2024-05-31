public class GameData
    {
        private static readonly Random m_Random = new Random();
        public Player PlayerOne { get; }
        public Player PlayerTwo { get; }
        public Player CurrentPlayer { get; set; }
        public BoardLetter[,] Letters { get; set; }
        public int BoardHeight { get; set; }
        public int BoardWidth { get; set; }

        public GameData(Player i_PlayerOne, Player i_PlayerTwo, int i_BoardWidth, int i_BoardHeight)
        {
            PlayerOne = i_PlayerOne;
            PlayerTwo = i_PlayerTwo;
            CurrentPlayer = i_PlayerOne;
            BoardWidth = i_BoardWidth;
            BoardHeight = i_BoardHeight;
            Letters = new BoardLetter[BoardHeight, BoardWidth];
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
            List<Cell> randomCells = new List<Cell>(BoardHeight * BoardWidth);

            for (int i = 0; i < BoardHeight; i++)
            {
                for (int j = 0; j < BoardWidth; j++)
                {
                   randomCells.Add(new Cell(i, j));
                }
            }

            return randomCells;
        }
        
        private char[] initializeBoardLetters()
        {
            char[] boardLetters = new char[BoardHeight * BoardWidth / 2];

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