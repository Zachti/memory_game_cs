namespace MemoryGame {
    internal struct Board(int i_width, int i_height)
    {
        public int Width { get; set; } = i_width;
        public int Height { get; set; } = i_height;
        public BoardLetter[,] Letters { get; set; } = new BoardLetter[i_height, i_width];
    }
}