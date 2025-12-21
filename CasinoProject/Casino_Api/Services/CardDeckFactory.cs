using Casino_Api.Models;

namespace Casino_Api.Services;

public interface ICardDeckFactory
{
    List<Card> CreateDeck();
    void Shuffle(List<Card> deck);
}

public class StandardCardDeckFactory : ICardDeckFactory
{
    private readonly IRandomNumberGenerator _rng;

    public StandardCardDeckFactory(IRandomNumberGenerator rng)
    {
        _rng = rng;
    }

    public List<Card> CreateDeck()
    {
        var suits = new[] { "♠️", "♥️", "♦️", "♣️" };
        var values = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        
        var deck = new List<Card>();
        foreach (var suit in suits)
        {
            foreach (var value in values)
            {
                deck.Add(new Card { Suit = suit, Value = value });
            }
        }
        
        return deck;
    }

    public void Shuffle(List<Card> deck)
    {
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(0, n + 1);
            var temp = deck[k];
            deck[k] = deck[n];
            deck[n] = temp;
        }
    }
}
