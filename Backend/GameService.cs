using Backend.Extensions;
using Microsoft.AspNetCore.SignalR;
using Backend.Hubs;

namespace Backend;

public class GameService
{
	private readonly IHubContext<GameHub> _hubContext;
	
	public IEnumerable<Game> GetRooms() => _games.Select(_ => _.Value).Where(game => (DateTime.Now - game.InsertionDate).TotalHours < 1);

	private readonly Dictionary<string, Game> _games = [];

	public GameService(IHubContext<GameHub> hubContext)
	{
		_hubContext = hubContext;
	}

	public async Task<string> CreateGame(int ante)
	{
		Game game = new(ante);
		_games[game.Id] = game;
		await BroadcastLobbyUpdate();
		return game.Id;
	}

	public async Task<bool> JoinGame(string gameId, Player player)
	{
		if (!_games.TryGetValue(gameId, out var game) || !game.IsWaiting)
			return false;

		game.Players.Add(player);
		await BroadcastLobbyUpdate();
		await BroadcastGameUpdate(gameId);

		return true;
	}

	public bool StartGame(string gameId)
	{
		if (!_games.TryGetValue(gameId, out var game) || !game.IsWaiting)
			return false;

		game.Players.RemoveAll(_ => _.Balance == 0);

		game.CommunityCards.Clear();
		game.PlayersActed.Clear();

		game.Players.ForEach(player => player.IsFolded = false);

		if (game.Players.Count < 2)
			return false;

		foreach (var player in game.Players)
		{
			if (player.Balance < game.AnteAmount)
				return false;

			player.Balance -= game.AnteAmount;
			game.Pot += game.AnteAmount;
			game.PlayerBets[player.Id] = 0;
		}

		game.Status = "started";
		game.LastRoundResult = null;
		game.RoundNumber++;

		game.Deck = DeckHelper.CreateShuffledDeck($"{gameId}-{game.RoundNumber}-{DateTime.UtcNow.Ticks}");

		game.DealHands();

		game.CommunityCards.AddRange(game.DrawCards(3));

		_ = BroadcastGameUpdate(gameId);

		return true;
	}

	public Game GetGame(string gameId)
		=> _games.TryGetValue(gameId, out var g) ? g : null;

	public Player GetPlayer(string gameId, string playerId)
		=> GetGame(gameId)?.Players.FirstOrDefault(p => p.SecretId == playerId);

	public bool SubmitAction(string gameId, string playerId, string actionType, int? betAmount = null)
	{
		var game = GetGame(gameId);

		if (game?.Status != "started")
			return false;

		var player = game.Players.FirstOrDefault(p => p.SecretId == playerId);

		if (player == null || player.IsFolded || player.IsAllIn)
			return false;

		var currentPlayer = game.Players[game.CurrentTurn];

		if (currentPlayer.SecretId != playerId)
			return false;

		var currentPlayerBet = game.PlayerBets.GetValueOrDefault(player.Id, 0);

		switch (actionType.ToLower())
		{
			case "fold":
				game.PlayersActed.Add(player.Id);
				player.IsFolded = true;
				break;

			case "check":
				if (currentPlayerBet < game.CurrentBet)
					return false;
				game.PlayersActed.Add(player.Id);
				break;

			case "call":
				int callAmount = game.CurrentBet - currentPlayerBet;
				if (player.Balance <= callAmount)
				{
					game.PlayerBets[player.Id] = game.PlayerBets.GetValueOrDefault(player.Id, 0) + player.Balance;
					game.Pot += player.Balance;
					player.Balance = 0;
					player.IsAllIn = true;
				}
				else
				{
					player.Balance -= callAmount;
					game.PlayerBets[player.Id] = game.CurrentBet;
					game.Pot += callAmount;
				}
				game.PlayersActed.Add(player.Id);
				break;

			case "bet":
				if (!betAmount.HasValue || betAmount.Value <= game.CurrentBet || betAmount.Value > player.Balance + currentPlayerBet)
					return false;

				int additionalAmount = betAmount.Value - currentPlayerBet;
				int previousBet = game.CurrentBet;
				game.CurrentBet = betAmount.Value;

				if (player.Balance <= additionalAmount)
				{
					game.PlayerBets[player.Id] = game.PlayerBets.GetValueOrDefault(player.Id, 0) + player.Balance;
					game.Pot += player.Balance;
					player.Balance = 0;
					player.IsAllIn = true;
				}
				else
				{
					player.Balance -= additionalAmount;
					game.PlayerBets[player.Id] = betAmount.Value;
					game.Pot += additionalAmount;
				}

				if (betAmount.Value > previousBet)
					game.PlayersActed.Clear();

				game.PlayersActed.Add(player.Id);
				break;

			default:
				return false;
		}

		if (game.Players.Count(p => !p.IsFolded) == 1)
		{
			game.ResolveWinner();
			_ = BroadcastGameUpdate(gameId);
			return true;
		}

		if (!game.AllPlayersActed())
		{
			game.AdvanceTurn();
			_ = BroadcastGameUpdate(gameId);
			return true;
		}

		if (game.BettingRound >= 3 || game.Players.Count(p => !p.IsFolded) <= 1)
		{
			game.ResolveWinner();
			_ = BroadcastGameUpdate(gameId);
			return true;
		}

		game.AdvanceBettingRound();

		var next = game.Players
			.Select((p, i) => new { Player = p, Index = i })
			.FirstOrDefault(x => !x.Player.IsFolded && !x.Player.IsAllIn);

		if (next != null)
			game.CurrentTurn = next.Index;
		else
			game.ResolveWinner();

		_ = BroadcastGameUpdate(gameId);
		return true;
	}

	public GameState GetPlayerView(string gameId, string playerId)
	{
		var game = GetGame(gameId);

		if (game == null)
			return null;

		var player = GetPlayer(gameId, playerId);

		if (player == null)
			return null;

		return new()
		{
			GameId = game.Id,
			Status = game.Status,
			CommunityCards = game.CommunityCards,
			Pot = game.Pot,
			CurrentBet = game.CurrentBet,
			PlayerCurrentBet = game.PlayerBets.GetValueOrDefault(player.Id, 0),
			CurrentTurn = game.Players[game.CurrentTurn].Id,
			Player = player,
			OtherPlayers = game.Players
				.Where(p => p.Id != player.Id)
				.Select(p => PublicPlayer.Create(p, game.Status, game.Players.Count, game.Players.Where(_=>_.IsFolded).Count())),
			LastRoundResult = game.LastRoundResult
		};
	}

	public bool ConfirmPlayerReady(string gameId, string playerId)
	{
		var game = _games[gameId];

		game.Players.Find(_ => _.SecretId == playerId).IsReady = true;

		var allReady = game.Players.All(_ => _.IsReady) && StartGame(gameId);
		
		if (!allReady)
			_ = BroadcastGameUpdate(gameId);
			
		return allReady;
	}

	private async Task BroadcastGameUpdate(string gameId)
	{
		var game = GetGame(gameId);
		if (game == null) return;

		// Send personalized state to each player individually
		foreach (var player in game.Players)
		{
			var state = GetPlayerView(gameId, player.SecretId);
			var connectionId = GameHub.GetConnectionId(player.SecretId);
			
			if (!string.IsNullOrEmpty(connectionId))
			{
				await _hubContext.Clients.Client(connectionId).SendAsync("GameStateUpdate", state);
			}
		}
	}

	private async Task BroadcastLobbyUpdate()
	{
		var rooms = GetRooms();
		await _hubContext.Clients.Group("lobby").SendAsync("LobbyUpdate", rooms);
	}
}
