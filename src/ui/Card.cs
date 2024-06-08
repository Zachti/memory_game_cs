namespace MemoryGame {
    internal struct Card(char i_Symbol)
    {
        public char Symbol { get;} = i_Symbol;
        public bool IsRevealed { get; set; }

        public void Flip() => IsRevealed = !IsRevealed;
    }           
}