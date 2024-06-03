namespace MemoryGame {
    internal class Player(string i_PlayerName, ePlayerTypes i_Type)
    {
        public string Name { get; set; } = i_PlayerName;
        public int Score { get; set; }
        public ePlayerTypes Type { get; set; } = i_Type;
    }
}