using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

public class GameHub : Hub
{
	private static readonly Dictionary<string, string> _connectionToPlayer = new();
	private static readonly Dictionary<string, string> _playerToConnection = new();

	public async Task JoinGameRoom(string gameId, string playerId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
		
		// Map connection to player
		_connectionToPlayer[Context.ConnectionId] = playerId;
		_playerToConnection[playerId] = Context.ConnectionId;
	}

	public async Task LeaveGameRoom(string gameId, string playerId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game-{gameId}");
		
		// Remove mappings
		_connectionToPlayer.Remove(Context.ConnectionId);
		_playerToConnection.Remove(playerId);
	}

	public async Task JoinLobby()
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, "lobby");
	}

	public async Task LeaveLobby()
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, "lobby");
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		// Clean up mappings when client disconnects
		if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
		{
			_connectionToPlayer.Remove(Context.ConnectionId);
			_playerToConnection.Remove(playerId);
		}
		
		await base.OnDisconnectedAsync(exception);
	}

	public static string GetConnectionId(string playerId)
	{
		_playerToConnection.TryGetValue(playerId, out var connectionId);
		return connectionId;
	}
}
