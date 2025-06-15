namespace Backend.Models;

public class SidePot
{
    public int Amount { get; set; }
    public List<Player> EligiblePlayers { get; set; } = [];
}

