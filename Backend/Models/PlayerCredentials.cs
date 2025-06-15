namespace Backend.Models;

public class PlayerCredentials(Player player)
{
	public string Id => player.Id;
	public string SecretId => player.SecretId;
}
