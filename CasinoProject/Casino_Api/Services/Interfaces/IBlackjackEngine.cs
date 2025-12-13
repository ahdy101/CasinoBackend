using Casino_Api.Models;

namespace Casino_Api.Services.Interfaces;

public interface IBlackjackEngine
{
    Task<BlackjackGame> InitializeGame(int userId, decimal betAmount);
    Task<BlackjackGame> Hit(int gameId, int userId);
    Task<BlackjackGame> Stand(int gameId, int userId);
    Task<BlackjackGame> DoubleDown(int gameId, int userId);
    Task<BlackjackGame> Split(int gameId, int userId);
    Task<decimal> CalculatePayout(BlackjackGame game);
}
