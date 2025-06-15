namespace Backend.Extensions;

public static class GameExtensions
{
	public static void DealHands(this Game game)
	{
		foreach (var player in game.Players)
		{
			player.Hand.Clear();
			player.Hand.Add(game.Deck.DrawCard());
			player.Hand.Add(game.Deck.DrawCard());
		}
	}

	public static void AdvanceTurn(this Game game)
	{
		if (game.AllPlayersActed() || game.Players.Count(p => !p.IsFolded) <= 1)
		{
			if (game.BettingRound >= 3 || game.Players.Count(p => !p.IsFolded) <= 1)
			{
				game.ResolveWinner();
				return;
			}
			game.AdvanceBettingRound();

			var eligiblePlayers = game.Players
				.Select((p, i) => new { Player = p, Index = i })
				.FirstOrDefault(x => !x.Player.IsFolded && !x.Player.IsAllIn);

			if (eligiblePlayers != null)
				game.CurrentTurn = eligiblePlayers.Index;
			else
				game.ResolveWinner();

			return;
		}

		var nextTurn = (game.CurrentTurn + 1) % game.Players.Count;

		var startTurn = game.CurrentTurn;

		for (var i = 0; i < game.Players.Count; i++)
		{
			var player = game.Players[nextTurn];

			if (!player.IsFolded && !player.IsAllIn)
			{
				game.CurrentTurn = nextTurn;
				return;
			}

			nextTurn = (nextTurn + 1) % game.Players.Count;
		}

		if (game.BettingRound >= 3 || game.Players.Count(p => !p.IsFolded) <= 1)
		{
			game.ResolveWinner();
			return;
		}

		game.AdvanceBettingRound();

		var eligiblePlayer = game.Players
			.Select((p, i) => new { Player = p, Index = i })
			.FirstOrDefault(x => !x.Player.IsFolded && !x.Player.IsAllIn);

		if (eligiblePlayer != null)
			game.CurrentTurn = eligiblePlayer.Index;
		else
			game.ResolveWinner();
	}

	public static bool AllPlayersActed(this Game game)
	{
		var eligible = game.Players.Where(p => !p.IsFolded && !p.IsAllIn).ToList();

		if (eligible.Count == 0)
			return true;

		var maxBet = eligible
			.Select(p => game.PlayerBets.GetValueOrDefault(p.Id, 0))
			.Max();

		return eligible.All(p =>
			game.PlayersActed.Contains(p.Id) &&
			game.PlayerBets.GetValueOrDefault(p.Id, 0) == maxBet
		);
	}

	public static void AdvanceBettingRound(this Game game)
	{
		game.PlayersActed.Clear();
		game.PlayerBets.Clear();

		game.CurrentBet = 0;

		if (game.BettingRound < 3)
			game.CommunityCards.AddRange(game.DrawCards(1));
		else
			game.ResolveWinner();

		game.BettingRound++;
	}

	public static List<string> DrawCards(this Game game, int count)
	{
		game.Deck.RemoveCards(game.CommunityCards);

		foreach (var p in game.Players)
			game.Deck.RemoveCards(p.Hand);

		List<string> cards = [];

		for (var i = 0; i < count; i++)
			cards.Add(game.Deck.DrawCard());

		return cards;
	}

	public static void ResolveWinner(this Game game)
	{
		try
		{
			game.Status = "waiting";

			var eligiblePlayers = game.Players.Where(p => !p.IsFolded).ToList();

			var totalBets = game.PlayerBets.Values.Sum();

			if (game.SidePots.Count == 0 && eligiblePlayers.Count > 0)
			{
				var winners = eligiblePlayers
					.OrderByDescending(p => PokerHandEvaluator.Evaluate([.. p.Hand, .. game.CommunityCards]))
					.ToList();

				var bestScore = PokerHandEvaluator.Evaluate([.. winners.First().Hand, .. game.CommunityCards]);
				var finalWinners = winners
					.Where(w => PokerHandEvaluator.Evaluate(w.Hand.Concat(game.CommunityCards).ToList()) == bestScore)
					.ToList();

				int share = game.Pot / finalWinners.Count;
				foreach (var winner in finalWinners)
					winner.Balance += share;

				return;
			}

			var sidePots = game.SidePots.Count > 0 ? game.SidePots : game.CalculateSidePots();

			foreach (var pot in sidePots)
			{
				var contenders = pot.EligiblePlayers
					.Where(p => !p.IsFolded)
					.ToList();

				if (contenders.Count == 0)
					continue;

				var sorted = contenders
					.OrderByDescending(p => PokerHandEvaluator.Evaluate([.. p.Hand, .. game.CommunityCards]))
					.ToList();

				var bestScore = PokerHandEvaluator.Evaluate([.. sorted.First().Hand, .. game.CommunityCards]);

				var finalWinners = sorted
					.Where(w => PokerHandEvaluator.Evaluate([.. w.Hand, .. game.CommunityCards]) == bestScore)
					.ToList();

				var share = pot.Amount / finalWinners.Count;
				foreach (var winner in finalWinners)
					winner.Balance += share;
			}
		}
		finally
		{
			game.Pot = 0;
			game.CurrentBet = 0;
			game.CurrentTurn = 0;
			game.BettingRound = 1;
			game.SidePots.Clear();
			game.PlayerBets.Clear();
			game.Players.ForEach(player =>
			{
				player.IsReady = player.Balance == 0;
				player.IsAllIn = false;
			});
			game.Status = "waiting";
		}
	}

	private static List<SidePot> CalculateSidePots(this Game game)
	{
		List<SidePot> pots = [];

		var bets = game.Players.ToDictionary(p => p, p => game.PlayerBets.GetValueOrDefault(p.Id, 0));

		while (bets.Values.Any(b => b > 0))
		{
			var min = bets.Values.Where(b => b > 0).Min();
			var eligible = bets.Where(b => b.Value > 0).Select(b => b.Key).ToList();
			pots.Add(new()
			{
				Amount = min * eligible.Count,
				EligiblePlayers = eligible
			});
			foreach (var p in eligible)
				bets[p] -= min;
		}

		return pots;
	}
}
