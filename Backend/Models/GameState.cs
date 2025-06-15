namespace Backend.Models;

public class GameState
{
    public string GameId { get; set; }
    public string Status { get; set; }
    public List<string> CommunityCards { get; set; }
    public int Pot { get; set; }
    public int CurrentBet { get; set; }
    public Player Player { get; set; }
    public IEnumerable<PublicPlayer> OtherPlayers { get; set; }
    public string CurrentTurn { get; internal set; }
}