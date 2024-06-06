namespace MemoryGame {
    internal struct Card<T>(T i_Symbol)
    {
        public T Symbol { get;} = i_Symbol;
        public bool IsRevealed { get; set; }

        public void Flip() => IsRevealed = !IsRevealed;
    }           
}