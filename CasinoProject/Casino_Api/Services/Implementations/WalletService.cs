using Casino_Api.Repositories.Interfaces;
using Casino_Api.Services.Interfaces;

namespace Casino_Api.Services.Implementations;

public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, decimal NewBalance, string Message)> AddFunds(int userId, decimal amount)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, 0, "User not found");

            user.Balance += amount;
            await _unitOfWork.CommitTransactionAsync();

            return (true, user.Balance, $"Successfully added {amount:C}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return (false, 0, $"Error adding funds: {ex.Message}");
        }
    }

    public async Task<(bool Success, decimal NewBalance, string Message)> CashOut(int userId, decimal amount)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, 0, "User not found");

            if (user.Balance < amount)
                return (false, user.Balance, "Insufficient balance");

            user.Balance -= amount;
            await _unitOfWork.CommitTransactionAsync();

            return (true, user.Balance, $"Successfully cashed out {amount:C}");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return (false, 0, $"Error cashing out: {ex.Message}");
        }
    }

    public async Task<(bool Success, decimal Balance)> GetBalance(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, 0);

        return (true, user.Balance);
    }

    public async Task<(bool Success, string Message)> DeductBet(int userId, decimal amount)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (user.Balance < amount)
            return (false, "Insufficient balance");

        user.Balance -= amount;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Bet deducted successfully");
    }

    public async Task<(bool Success, string Message)> ProcessPayout(int userId, decimal betAmount, decimal payout)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        user.Balance += payout;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Payout processed successfully");
    }
}
