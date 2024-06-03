namespace MemoryGame {
    internal struct Cell(int i_Row, int i_Column)
    {
        public int Row { get; set; } = i_Row;
        public int Column { get; set; } = i_Column;

        public static Cell Parse(string i_ToParse)
        {
            int column = i_ToParse[0] - 'A';
            int row = i_ToParse[1] - '1';

            return new Cell(row, column);
        }
        
        public override readonly string ToString()
        {
            return string.Format("{0}{1}", (char)(Column + 'A'), (char)(Row + '1'));
        }
    }
}