using System.Text.Json.Serialization;

namespace Backend.Models;

public class Game(int anteAmount)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int AnteAmount { get; } = anteAmount;
    [JsonIgnore]
    internal DeckHelper Deck { get; set; }
	public List<Player> Players { get; set; } = [];
    public List<string> CommunityCards { get; set; } = [];
    public string Status { get; set; } = "waiting";
    public int Pot { get; set; }
    public int CurrentTurn { get; set; }
    public int CurrentBet { get; set; }
    public Dictionary<string, int> PlayerBets { get; set; } = [];
    public int BettingRound { get; set; } = 1;
    public List<SidePot> SidePots { get; set; } = [];
    public DateTime InsertionDate = DateTime.Now;
    internal bool IsWaiting => Status == "waiting";
	public HashSet<string> PlayersActed = [];
	public RoundResult LastRoundResult { get; set; }
	public int RoundNumber { get; set; } = 0;
}

