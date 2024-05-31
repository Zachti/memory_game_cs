public class Player {
    public string PlayerName { get; set; }
    public int PlayerScore { get; set; }
    public ePlayerTypes Type { get; set; }

    public Player(string i_playerName, ePlayerTypes i_type)
    {
        PlayerName = i_playerName;
        Type = i_type;
        PlayerScore = 0;
    }
}