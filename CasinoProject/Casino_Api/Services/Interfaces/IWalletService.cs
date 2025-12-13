namespace Casino_Api.Services.Interfaces;

public interface IWalletService
{
    Task<(bool Success, decimal NewBalance, string Message)> AddFunds(int userId, decimal amount);
    Task<(bool Success, decimal NewBalance, string Message)> CashOut(int userId, decimal amount);
    Task<(bool Success, decimal Balance)> GetBalance(int userId);
    Task<(bool Success, string Message)> DeductBet(int userId, decimal amount);
    Task<(bool Success, string Message)> ProcessPayout(int userId, decimal betAmount, decimal payout);
}
