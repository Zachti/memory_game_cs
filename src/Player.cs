namespace MemoryGame {
    internal class Player(string i_playerName, ePlayerTypes i_type)
    {
        public string Name { get; set; } = i_playerName;
        public int Score { get; set; } = 0;
        public ePlayerTypes Type { get; set; } = i_type;
    }
}