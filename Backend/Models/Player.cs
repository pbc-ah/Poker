using System.Text.Json.Serialization;

namespace Backend.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonIgnore]
    public string SecretId { get; set; } = Guid.NewGuid().ToString();
	public string Name { get; set; }
    public int Balance { get; set; }
    public List<string> Hand { get; set; } = [];
    public bool IsFolded { get; set; }
    public bool IsAllIn { get; set; }
    public bool IsReady { get; set; }
}

