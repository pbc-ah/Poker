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

		// Check if all eligible players have acted
		if (!eligible.All(p => game.PlayersActed.Contains(p.Id)))
			return false;

		// Check if all players have matched the current bet
		var maxBet = game.CurrentBet;
		return eligible.All(p => game.PlayerBets.GetValueOrDefault(p.Id, 0) >= maxBet);
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
		var originalPot = game.Pot;
		var roundResult = new RoundResult
		{
			TotalPot = originalPot,
			Winners = []
		};

		try
		{
			game.Status = "waiting";

			var eligiblePlayers = game.Players.Where(p => !p.IsFolded).ToList();

			// Complete the board if not all cards are dealt (happens when everyone folds)
			while (game.CommunityCards.Count < 5)
			{
				game.CommunityCards.AddRange(game.DrawCards(1));
			}

			if (eligiblePlayers.Count == 0)
			{
				// Everyone folded - shouldn't happen but handle it
				game.LastRoundResult = roundResult;
				return;
			}

			if (eligiblePlayers.Count == 1)
			{
				// Only one player left - they win by fold
				var winner = eligiblePlayers.First();
				winner.Balance += game.Pot;

				roundResult.WinningHandName = "Win by Fold";
				roundResult.Winners.Add(new WinnerInfo
				{
					PlayerId = winner.Id,
					PlayerName = winner.Name,
					AmountWon = game.Pot,
					HandScore = 0,
					HandName = "Win by Fold"
				});

				game.LastRoundResult = roundResult;
				return;
			}

			if (game.SidePots.Count == 0)
			{
				var playersWithScores = eligiblePlayers
					.Select(p => new
					{
						Player = p,
						Score = PokerHandEvaluator.Evaluate([.. p.Hand, .. game.CommunityCards])
					})
					.OrderByDescending(x => x.Score)
					.ToList();

				var bestScore = playersWithScores.First().Score;
				var finalWinners = playersWithScores
					.Where(x => x.Score == bestScore)
					.ToList();

				int share = game.Pot / finalWinners.Count;
				
				var handName = PokerHandEvaluator.GetHandName(bestScore);
				roundResult.WinningHandName = handName;

				foreach (var winner in finalWinners)
				{
					winner.Player.Balance += share;
					roundResult.Winners.Add(new WinnerInfo
					{
						PlayerId = winner.Player.Id,
						PlayerName = winner.Player.Name,
						AmountWon = share,
						HandScore = winner.Score,
						HandName = handName
					});
				}

				game.LastRoundResult = roundResult;
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

				if (contenders.Count == 1)
				{
					// Only one player eligible for this side pot
					var winner = contenders.First();
					winner.Balance += pot.Amount;

					var existingWinner = roundResult.Winners.FirstOrDefault(w => w.PlayerId == winner.Id);
					if (existingWinner != null)
					{
						existingWinner.AmountWon += pot.Amount;
					}
					else
					{
						roundResult.Winners.Add(new WinnerInfo
						{
							PlayerId = winner.Id,
							PlayerName = winner.Name,
							AmountWon = pot.Amount,
							HandScore = 0,
							HandName = "Win by Fold"
						});
					}
					continue;
				}

				var playersWithScores = contenders
					.Select(p => new
					{
						Player = p,
						Score = PokerHandEvaluator.Evaluate([.. p.Hand, .. game.CommunityCards])
					})
					.OrderByDescending(x => x.Score)
					.ToList();

				var bestScore = playersWithScores.First().Score;
				var finalWinners = playersWithScores
					.Where(x => x.Score == bestScore)
					.ToList();

				var share = pot.Amount / finalWinners.Count;
				var handName = PokerHandEvaluator.GetHandName(bestScore);

				if (string.IsNullOrEmpty(roundResult.WinningHandName))
					roundResult.WinningHandName = handName;

				foreach (var winner in finalWinners)
				{
					winner.Player.Balance += share;
					
					var existingWinner = roundResult.Winners.FirstOrDefault(w => w.PlayerId == winner.Player.Id);
					if (existingWinner != null)
					{
						existingWinner.AmountWon += share;
					}
					else
					{
						roundResult.Winners.Add(new WinnerInfo
						{
							PlayerId = winner.Player.Id,
							PlayerName = winner.Player.Name,
							AmountWon = share,
							HandScore = winner.Score,
							HandName = handName
						});
					}
				}
			}

			game.LastRoundResult = roundResult;
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
