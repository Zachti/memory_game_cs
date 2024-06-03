namespace MemoryGame {
    internal struct Board(int i_width, int i_height)
    {
        public int Width { get; set; } = i_width;
        public int Height { get; set; } = i_height;
        public BoardLetter[,] Letters { get; set; } = new BoardLetter[i_height, i_width];

        public readonly ref BoardLetter this[Cell cell] { get => ref Letters[cell.Row, cell.Column]; }
        
        public readonly void InsertLetterToBoard(char i_Letter, Cell i_Cell) => Letters[i_Cell.Row, i_Cell.Column] = new BoardLetter(i_Letter);
    }
}