namespace MemoryGame {
    internal struct Board(int i_Width, int i_Height)
    {
        public int Width { get; set; } = i_Width;
        public int Height { get; set; } = i_Height;
        public Card[,] Cards { get; set; } = new Card[i_Height, i_Width];

        
        public ref Card this[Cell i_Cell] { get => ref Cards[i_Cell.Row, i_Cell.Column]; }

        public void InsertSymbolToBoard(char i_Symbol, Cell i_Cell) => Cards[i_Cell.Row, i_Cell.Column] = new Card(i_Symbol);
    }
}