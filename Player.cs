public class Player {
     public string PlayerName { get; set; }
    public int PlayerScore { get; set; }
    public ePlayerTypes Type { get; set; }

    public Player(string playerName, ePlayerTypes type)
    {
        PlayerName = playerName;
        Type = type;
        PlayerScore = 0;
    }
}