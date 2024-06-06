namespace MemoryGame {
    internal class Board(int i_Width, int i_Height)
    {
        public int Width { get; set; } = i_Width;
        public int Height { get; set; } = i_Height;
        public Card<char>[,] Cards { get; set; } = new Card<char>[i_Height, i_Width];
        private int RevealedSquaresCounter { get; set; }
        public bool IsBoardFinished => Height * Width == RevealedSquaresCounter;


        public ref Card<char> this[Cell i_Cell] { get => ref Cards[i_Cell.Row, i_Cell.Column]; }

        public void InsertSymbolToBoard(char i_Symbol, Cell i_Cell) => Cards[i_Cell.Row, i_Cell.Column] = new Card<char>(i_Symbol);

        public void IncrementRevealedSquaresCounter() => RevealedSquaresCounter += 2;

        public static bool IsValid(int i_Width, int i_Height) => i_Width * i_Height % 2 == 0;
    }
}