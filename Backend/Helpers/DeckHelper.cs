namespace Backend.Helpers;

public class DeckHelper
{
	private readonly List<string> Cards;

	public DeckHelper(string seed)
	{
		Cards = CreateDeck();

		Shuffle(seed);
	}

	public static DeckHelper CreateShuffledDeck(string seed) 
		=> new(seed);

	public string DrawCard()
	{
		if (Cards.Count == 0)
			throw new InvalidOperationException("empty deck - should never happen");

		var card = Cards[0];

		Cards.RemoveAt(0);

		return card;
	}

	public void RemoveCards(IEnumerable<string> cards)
	{
		foreach (var card in cards)
			Cards.Remove(card);
	}

	private void Shuffle(string seed)
	{
		Random random = new(seed.GetHashCode());

		for (var i = Cards.Count - 1; i > 0; i--)
		{
			var j = random.Next(i + 1);
			(Cards[j], Cards[i]) = (Cards[i], Cards[j]);
		}
	}

	private static List<string> CreateDeck()
	{
		List<char> suits = ['C', 'D', 'H', 'S'],
			ranks = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];

		List<string> deck = [];

		foreach (var s in suits)
			foreach (var r in ranks)
				deck.Add($"{s}{r}");

		return deck;
	}
}

