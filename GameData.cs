 public class GameData
    {
        public Player PlayerOne { get; }
        public Player PlayerTwo { get; }
        public Player CurrentPlayer { get; set; }
        public BoardLetter[,] Letters { get; set; }
        public int BoardHeight { get; set; }
        public int BoardWidth { get; set; }

        public GameData(Player playerOne, Player playerTwo, int boardWidth, int boardHeight)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
            CurrentPlayer = playerOne;
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            Letters = new BoardLetter[BoardHeight, BoardWidth];
        }
    }