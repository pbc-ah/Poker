using System.Text.Json.Serialization;

namespace Backend.Models;

public class PublicPlayer
{
	public static PublicPlayer Create(Player player, string status, int totalPlayers, int foldedPlayers) 
		=> new()
		{
			Id = player.Id,
			Name = player.Name,
			Balance = player.Balance,
			IsFolded = player.IsFolded,
			IsReady = player.IsReady,
			Hand = status == "waiting" && player.Hand?.Count > 0 && totalPlayers - 1 != foldedPlayers ? player.Hand : null
		};

	public string Id { get; init; }
	public string Name { get; init; }
	public int Balance { get; init; }
	public bool IsFolded { get; init; }
	public bool IsReady { get; set; }
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<string> Hand { get; set; }
}
