using Backend.Extensions;

namespace Backend;

public class GameService
{
	public IEnumerable<Game> GetRooms() => _games.Select(_ => _.Value).Where(game => (DateTime.Now - game.InsertionDate).TotalHours < 1);

	private readonly Dictionary<string, Game> _games = [];

	public string CreateGame(int ante)
	{
		Game game = new(ante);
		_games[game.Id] = game;
		return game.Id;
	}

	public bool JoinGame(string gameId, Player player)
	{
		if (!_games.TryGetValue(gameId, out var game) || !game.IsWaiting)
			return false;

		game.Players.Add(player);

		return true;
	}

	public bool StartGame(string gameId)
	{
		if (!_games.TryGetValue(gameId, out var game) || !game.IsWaiting)
			return false;

		game.Players.RemoveAll(_ => _.Balance == 0);

		game.CommunityCards.Clear();

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

		game.Deck = DeckHelper.CreateShuffledDeck(gameId);

		game.DealHands();

		game.CommunityCards.AddRange(game.DrawCards(3));

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

		var currentPlayerBet = game.PlayerBets.GetValueOrDefault(playerId, 0);

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
					game.PlayerBets[player.Id] += player.Balance;
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
				if (!betAmount.HasValue || betAmount.Value <= game.CurrentBet || betAmount.Value > player.Balance)
					return false;

				int previousBet = game.CurrentBet;
				game.CurrentBet = betAmount.Value;

				if (player.Balance <= betAmount.Value)
				{
					if (game.PlayerBets.ContainsKey(playerId))
						game.PlayerBets[player.Id] += player.Balance;
					else
						game.PlayerBets[player.Id] = player.Balance;

					game.Pot += player.Balance;
					player.Balance = 0;
					player.IsAllIn = true;
				}
				else
				{
					player.Balance -= betAmount.Value;
					game.PlayerBets[player.Id] = betAmount.Value;
					game.Pot += betAmount.Value;
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
			return true;
		}

		if (!game.AllPlayersActed())
		{
			game.AdvanceTurn();
			return true;
		}

		if (game.BettingRound >= 3 || game.Players.Count(p => !p.IsFolded) <= 1)
		{
			game.ResolveWinner();
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
			CurrentTurn = game.Players[game.CurrentTurn].Id,
			Player = player,
			OtherPlayers = game.Players
				.Where(p => p.Id != player.Id)
				.Select(p => PublicPlayer.Create(p, game.Status))
		};
	}

	public bool ConfirmPlayerReady(string gameId, string playerId)
	{
		var game = _games[gameId];

		game.Players.Find(_ => _.SecretId == playerId).IsReady = true;

		return game.Players.All(_ => _.IsReady) && StartGame(gameId);
	}
}
