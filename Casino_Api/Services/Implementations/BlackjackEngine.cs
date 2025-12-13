using Casino.Backend.Data;
using Casino.Backend.Infrastructure;
using Casino.Backend.Models;
using Casino.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Casino.Backend.Services.Implementations
{
    /// <summary>
    /// Blackjack game engine implementation
  /// Rules: Dealer hits on soft 17, Blackjack pays 3:2, Standard doubling and splitting
    /// </summary>
    public class BlackjackEngine : IBlackjackEngine
    {
        private readonly AppDbContext _db;
        private readonly ICardDeckFactory _deckFactory;
        private readonly ILogger<BlackjackEngine> _logger;

      public BlackjackEngine(AppDbContext db, ICardDeckFactory deckFactory, ILogger<BlackjackEngine> logger)
        {
            _db = db;
      _deckFactory = deckFactory;
         _logger = logger;
     }

        /// <summary>
        /// Initialize a new Blackjack game
        /// </summary>
        public async Task<BlackjackGameState> InitializeGame(int userId, decimal betAmount)
        {
            _logger.LogInformation("InitializeGame - UserId: {UserId}, Bet: {Bet}", userId, betAmount);

    // Create and shuffle deck (use 6 decks for realistic casino simulation)
            var deck = _deckFactory.CreateMultipleDecks(6);
            _deckFactory.Shuffle(deck);

            // Deal initial cards
   var playerHand = new List<Card> { deck[0], deck[2] };
            var dealerHand = new List<Card> { deck[1], deck[3] };
            int deckIndex = 4;

     // Calculate totals
        var playerTotal = CalculateHandTotal(playerHand);
   var dealerTotal = CalculateHandTotal(new List<Card> { dealerHand[0] }); // Only show dealer's first card

            // Check for immediate blackjack
            var status = playerTotal == 21 ? GameStatus.PlayerBlackjack : GameStatus.Active;

   // Create game entity
            var game = new BlackjackGame
       {
          UserId = userId,
           BetAmount = betAmount,
      PlayerCards = JsonSerializer.Serialize(playerHand),
       DealerCards = JsonSerializer.Serialize(dealerHand),
      PlayerTotal = playerTotal,
      DealerTotal = dealerTotal,
     Status = status.ToString(),
                CreatedAt = DateTime.UtcNow
          };

            // If player has blackjack, dealer plays immediately
   if (status == GameStatus.PlayerBlackjack)
            {
           var dealerFinalTotal = CalculateHandTotal(dealerHand);
       if (dealerFinalTotal == 21)
            {
          game.Status = GameStatus.Push.ToString();
game.Payout = betAmount; // Return original bet
                }
                else
       {
           game.Payout = betAmount * 2.5m; // Blackjack pays 3:2 (bet + 1.5x bet)
 }
        game.CompletedAt = DateTime.UtcNow;
 }

      _db.BlackjackGames.Add(game);
  await _db.SaveChangesAsync();

    _logger.LogInformation("InitializeGame complete - GameId: {GameId}, PlayerTotal: {PlayerTotal}", 
            game.Id, playerTotal);

      return MapToGameState(game, playerHand, dealerHand, status == GameStatus.PlayerBlackjack);
        }

     /// <summary>
        /// Player hits (draws a card)
        /// </summary>
        public async Task<BlackjackGameState> Hit(int gameId, int userId)
      {
         _logger.LogInformation("Hit - GameId: {GameId}, UserId: {UserId}", gameId, userId);

      var game = await _db.BlackjackGames.FindAsync(gameId);
        if (game == null || game.UserId != userId)
    throw new InvalidOperationException("Game not found or access denied");

            if (game.Status != "Active")
             throw new InvalidOperationException("Game is not active");

            // Deserialize hands
        var playerHand = JsonSerializer.Deserialize<List<Card>>(game.PlayerCards) ?? new List<Card>();
  var dealerHand = JsonSerializer.Deserialize<List<Card>>(game.DealerCards) ?? new List<Card>();

            // Deal one card to player
      var deck = _deckFactory.CreateMultipleDecks(6);
       _deckFactory.Shuffle(deck);
            playerHand.Add(deck[0]);

      // Calculate new total
  var playerTotal = CalculateHandTotal(playerHand);
            game.PlayerTotal = playerTotal;
game.PlayerCards = JsonSerializer.Serialize(playerHand);

   // Check for bust
     if (playerTotal > 21)
 {
       game.Status = GameStatus.PlayerBust.ToString();
      game.Payout = 0m; // Player loses
     game.CompletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

  _logger.LogInformation("Hit - Player bust. GameId: {GameId}, Total: {Total}", gameId, playerTotal);
             return MapToGameState(game, playerHand, dealerHand, true);
     }

            await _db.SaveChangesAsync();
  return MapToGameState(game, playerHand, dealerHand, false);
        }

   /// <summary>
        /// Player stands (ends their turn, dealer plays)
        /// </summary>
        public async Task<BlackjackGameState> Stand(int gameId, int userId)
      {
            _logger.LogInformation("Stand - GameId: {GameId}, UserId: {UserId}", gameId, userId);

       var game = await _db.BlackjackGames.FindAsync(gameId);
if (game == null || game.UserId != userId)
           throw new InvalidOperationException("Game not found or access denied");

 if (game.Status != "Active")
                throw new InvalidOperationException("Game is not active");

            // Deserialize hands
      var playerHand = JsonSerializer.Deserialize<List<Card>>(game.PlayerCards) ?? new List<Card>();
   var dealerHand = JsonSerializer.Deserialize<List<Card>>(game.DealerCards) ?? new List<Card>();

      // Dealer plays (hits until 17 or higher)
       var deck = _deckFactory.CreateMultipleDecks(6);
          _deckFactory.Shuffle(deck);
        int deckIndex = 0;

   var dealerTotal = CalculateHandTotal(dealerHand);
  while (dealerTotal < 17)
            {
    dealerHand.Add(deck[deckIndex++]);
     dealerTotal = CalculateHandTotal(dealerHand);
         }

        game.DealerCards = JsonSerializer.Serialize(dealerHand);
            game.DealerTotal = dealerTotal;

        // Determine winner
   var playerTotal = game.PlayerTotal;
     
        if (dealerTotal > 21)
        {
       game.Status = GameStatus.DealerBust.ToString();
         game.Payout = game.BetAmount * 2m; // Player wins (bet + bet)
            }
       else if (playerTotal > dealerTotal)
            {
     game.Status = GameStatus.PlayerWin.ToString();
         game.Payout = game.BetAmount * 2m;
          }
            else if (dealerTotal > playerTotal)
            {
                game.Status = GameStatus.DealerWin.ToString();
                game.Payout = 0m; // Player loses
            }
       else
   {
         game.Status = GameStatus.Push.ToString();
   game.Payout = game.BetAmount; // Return original bet
    }

    game.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

   _logger.LogInformation("Stand complete - GameId: {GameId}, Status: {Status}, Payout: {Payout}", 
   gameId, game.Status, game.Payout);

    return MapToGameState(game, playerHand, dealerHand, true);
  }

        /// <summary>
/// Player doubles down (double bet, one card, then stand)
        /// </summary>
        public async Task<BlackjackGameState> DoubleDown(int gameId, int userId)
 {
            _logger.LogInformation("DoubleDown - GameId: {GameId}, UserId: {UserId}", gameId, userId);

          var game = await _db.BlackjackGames.FindAsync(gameId);
            if (game == null || game.UserId != userId)
     throw new InvalidOperationException("Game not found or access denied");

        if (game.Status != "Active")
     throw new InvalidOperationException("Game is not active");

   var playerHand = JsonSerializer.Deserialize<List<Card>>(game.PlayerCards) ?? new List<Card>();
        
    // Can only double down on initial two cards
            if (playerHand.Count != 2)
      throw new InvalidOperationException("Can only double down on initial hand");

            // Double the bet
            game.BetAmount *= 2;

   // Deal one card
   var deck = _deckFactory.CreateMultipleDecks(6);
  _deckFactory.Shuffle(deck);
    playerHand.Add(deck[0]);

            var playerTotal = CalculateHandTotal(playerHand);
            game.PlayerTotal = playerTotal;
            game.PlayerCards = JsonSerializer.Serialize(playerHand);

 // If bust, game over
if (playerTotal > 21)
     {
       game.Status = GameStatus.PlayerBust.ToString();
       game.Payout = 0m;
                game.CompletedAt = DateTime.UtcNow;
      await _db.SaveChangesAsync();
        
           var dealerHand = JsonSerializer.Deserialize<List<Card>>(game.DealerCards) ?? new List<Card>();
    return MapToGameState(game, playerHand, dealerHand, true);
   }

            // Otherwise, automatically stand
            await _db.SaveChangesAsync();
     return await Stand(gameId, userId);
        }

        /// <summary>
        /// Player splits pairs (not implemented in this example)
      /// </summary>
      public Task<BlackjackGameState> Split(int gameId, int userId)
 {
            throw new NotImplementedException("Split functionality will be added in future update");
      }

 /// <summary>
        /// Calculate payout based on game outcome
        /// </summary>
        public decimal CalculatePayout(BlackjackGameState state)
        {
            return state.Payout ?? 0m;
        }

  /// <summary>
/// Calculate total value of a hand (handles Aces as 1 or 11)
    /// </summary>
        private int CalculateHandTotal(List<Card> hand)
  {
            int total = 0;
       int aces = 0;

            foreach (var card in hand)
   {
      if (card.IsAce())
                {
         aces++;
                    total += 11;
                }
        else
           {
         total += card.Value;
        }
            }

            // Convert Aces from 11 to 1 if necessary
 while (total > 21 && aces > 0)
   {
    total -= 10;
            aces--;
            }

      return total;
        }

     /// <summary>
   /// Map database entity to game state DTO
        /// </summary>
      private BlackjackGameState MapToGameState(BlackjackGame game, List<Card> playerHand, List<Card> dealerHand, bool showDealerCards)
    {
            return new BlackjackGameState
    {
       GameId = game.Id,
      PlayerHand = playerHand,
        DealerHand = showDealerCards ? dealerHand : new List<Card> { dealerHand[0] }, // Hide dealer's second card
  PlayerTotal = game.PlayerTotal,
           DealerTotal = showDealerCards ? CalculateHandTotal(dealerHand) : dealerHand[0].Value,
           DealerShowsAll = showDealerCards,
       Status = Enum.Parse<GameStatus>(game.Status),
      BetAmount = game.BetAmount,
                Payout = game.Payout,
     CanHit = game.Status == "Active",
          CanStand = game.Status == "Active",
                CanDoubleDown = game.Status == "Active" && playerHand.Count == 2,
        CanSplit = false // Not implemented yet
            };
        }
    }
}
