using Casino_Api.Infrastructure;
using Casino_Api.Models;
using Casino_Api.Repositories.Interfaces;
using Casino_Api.Services.Interfaces;
using System.Text.Json;

namespace Casino_Api.Services.Implementations;

public class BlackjackEngine : IBlackjackEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICardDeckFactory _deckFactory;
    private readonly IWalletService _walletService;

    public BlackjackEngine(IUnitOfWork unitOfWork, ICardDeckFactory deckFactory, IWalletService walletService)
    {
        _unitOfWork = unitOfWork;
        _deckFactory = deckFactory;
        _walletService = walletService;
    }

    public async Task<BlackjackGame> InitializeGame(int userId, decimal betAmount)
    {
        // Validate and deduct bet
        var deductResult = await _walletService.DeductBet(userId, betAmount);
        if (!deductResult.Success)
            throw new InvalidOperationException(deductResult.Message);

        // Create and shuffle deck
        var deck = _deckFactory.CreateDeck();
        _deckFactory.Shuffle(deck);

        // Deal initial cards
        var playerHand = new List<Card> { deck[0], deck[2] };
        var dealerHand = new List<Card> { deck[1] };

        var game = new BlackjackGame
        {
            UserId = userId,
            BetAmount = betAmount,
            PlayerHandJson = JsonSerializer.Serialize(playerHand),
            DealerHandJson = JsonSerializer.Serialize(dealerHand),
            PlayerTotal = CalculateHandValue(playerHand),
            DealerTotal = CalculateHandValue(dealerHand),
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        // Check for player blackjack
        if (game.PlayerTotal == 21)
        {
            game.Status = "PlayerBlackjack";
            game.Payout = betAmount * 2.5m; // Blackjack pays 3:2
            game.CompletedAt = DateTime.UtcNow;
            await _walletService.ProcessPayout(userId, betAmount, game.Payout.Value);
        }

        await _unitOfWork.BlackjackGames.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return game;
    }

    public async Task<BlackjackGame> Hit(int gameId, int userId)
    {
        var game = await _unitOfWork.BlackjackGames.GetByIdAsync(gameId);
        if (game == null || game.UserId != userId)
            throw new InvalidOperationException("Game not found");

        if (game.Status != "Active")
            throw new InvalidOperationException("Game is not active");

        var playerHand = JsonSerializer.Deserialize<List<Card>>(game.PlayerHandJson) ?? new();
        var deck = _deckFactory.CreateDeck();
        _deckFactory.Shuffle(deck);

        // Add a card
        playerHand.Add(deck[0]);
        game.PlayerHandJson = JsonSerializer.Serialize(playerHand);
        game.PlayerTotal = CalculateHandValue(playerHand);

        // Check for bust
        if (game.PlayerTotal > 21)
        {
            game.Status = "PlayerBust";
            game.Payout = 0;
            game.CompletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    public async Task<BlackjackGame> Stand(int gameId, int userId)
    {
        var game = await _unitOfWork.BlackjackGames.GetByIdAsync(gameId);
        if (game == null || game.UserId != userId)
            throw new InvalidOperationException("Game not found");

        if (game.Status != "Active")
            throw new InvalidOperationException("Game is not active");

        var dealerHand = JsonSerializer.Deserialize<List<Card>>(game.DealerHandJson) ?? new();
        var deck = _deckFactory.CreateDeck();
        _deckFactory.Shuffle(deck);

        // Dealer draws until 17 or higher
        while (CalculateHandValue(dealerHand) < 17)
        {
            dealerHand.Add(deck[dealerHand.Count]);
        }

        game.DealerHandJson = JsonSerializer.Serialize(dealerHand);
        game.DealerTotal = CalculateHandValue(dealerHand);

        // Determine winner
        if (game.DealerTotal > 21)
        {
            game.Status = "DealerBust";
            game.Payout = game.BetAmount * 2;
        }
        else if (game.PlayerTotal > game.DealerTotal)
        {
            game.Status = "PlayerWin";
            game.Payout = game.BetAmount * 2;
        }
        else if (game.PlayerTotal < game.DealerTotal)
        {
            game.Status = "DealerWin";
            game.Payout = 0;
        }
        else
        {
            game.Status = "Push";
            game.Payout = game.BetAmount; // Return bet
        }

        game.CompletedAt = DateTime.UtcNow;
        
        if (game.Payout > 0)
            await _walletService.ProcessPayout(userId, game.BetAmount, game.Payout.Value);

        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    public async Task<BlackjackGame> DoubleDown(int gameId, int userId)
    {
        var game = await _unitOfWork.BlackjackGames.GetByIdAsync(gameId);
        if (game == null || game.UserId != userId)
            throw new InvalidOperationException("Game not found");

        // Deduct additional bet
        var deductResult = await _walletService.DeductBet(userId, game.BetAmount);
        if (!deductResult.Success)
            throw new InvalidOperationException(deductResult.Message);

        game.BetAmount *= 2;

        // Hit once, then stand
        game = await Hit(gameId, userId);
        if (game.Status == "Active")
            game = await Stand(gameId, userId);

        return game;
    }

    public async Task<BlackjackGame> Split(int gameId, int userId)
    {
        // Simplified - not fully implemented
        throw new NotImplementedException("Split feature coming soon");
    }

    public async Task<decimal> CalculatePayout(BlackjackGame game)
    {
        return game.Payout ?? 0;
    }

    private int CalculateHandValue(List<Card> hand)
    {
        int value = 0;
        int aces = 0;

        foreach (var card in hand)
        {
            if (card.IsAce())
            {
                aces++;
                value += 11;
            }
            else
            {
                value += card.GetNumericValue();
            }
        }

        // Adjust for aces
        while (value > 21 && aces > 0)
        {
            value -= 10;
            aces--;
        }

        return value;
    }
}
