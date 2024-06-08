namespace MemoryGame {
    internal class Player(string i_PlayerName, ePlayerTypes i_Type)
    {
        public string Name { get; } = i_PlayerName;
        public int Score { get; set; }
        public ePlayerTypes Type { get; } = i_Type;
    }
}