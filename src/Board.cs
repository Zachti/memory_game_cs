namespace MemoryGame {
    internal struct Board(int i_width, int i_height)
    {
        public int Width { get; set; } = i_width;
        public int Height { get; set; } = i_height;
        public BoardLetter[,] Letters { get; set; } = new BoardLetter[i_height, i_width];

        public ref BoardLetter GetLetterAt(Cell i_Cell) => ref Letters[i_Cell.Row, i_Cell.Column];

        public void InsertLetterToBoard(char i_Letter, Cell i_Cell) => Letters[i_Cell.Row, i_Cell.Column] = new BoardLetter(i_Letter);

    }
}