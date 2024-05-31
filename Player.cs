public class Player(string i_playerName, ePlayerTypes i_type)
{
    public string PlayerName { get; set; } = i_playerName;
    public int PlayerScore { get; set; } = 0;
    public ePlayerTypes Type { get; set; } = i_type;
}