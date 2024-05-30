public class BoardLetter
{
    public char Letter { get; set; }
    public bool IsRevealed { get; set; }

    public BoardLetter(char i_Letter)
    {
        Letter = i_Letter;
        IsRevealed = false;
    }
}