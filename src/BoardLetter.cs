namespace MemoryGame {
    internal struct BoardLetter(char i_Letter)
    {
        public char Letter { get; set; } = i_Letter;
        public bool IsRevealed { get; set; } = false;

        public void Flip() => IsRevealed = !IsRevealed;
    }           
}