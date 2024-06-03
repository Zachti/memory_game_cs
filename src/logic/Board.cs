namespace MemoryGame {
    internal class Board(int i_width, int i_height)
    {
        public int Width { get; set; } = i_width;
        public int Height { get; set; } = i_height;
        public Card[,] Cards { get; set; } = new Card[i_height, i_width];
        private int RevealedSquaresCounter { get; set; }
        public bool IsBoardFinished => Height * Width == RevealedSquaresCounter;


        public ref Card this[Cell cell] { get => ref Cards[cell.Row, cell.Column]; }

        public void InsertLetterToBoard(char i_Letter, Cell i_Cell) => Cards[i_Cell.Row, i_Cell.Column] = new Card(i_Letter);

        public void IncrementRevealedSquaresCounter() => RevealedSquaresCounter += 2;
    }
}