namespace Backend.Models;

public class RoundResult
{
	public List<WinnerInfo> Winners { get; set; } = [];
	public int TotalPot { get; set; }
	public string WinningHandName { get; set; }
}

public class WinnerInfo
{
	public string PlayerId { get; set; }
	public string PlayerName { get; set; }
	public int AmountWon { get; set; }
	public int HandScore { get; set; }
	public string HandName { get; set; }
}
