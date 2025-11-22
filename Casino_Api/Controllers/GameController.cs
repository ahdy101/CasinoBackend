using Casino.Backend.Data;
using Casino.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Casino.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GameController(AppDbContext db) { _db = db; }

        private bool IsApiKeyValid(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
        }

        [HttpPost("roulette")]
        [Authorize]
        public async Task<IActionResult> PlayRoulette([FromBody] RouletteRequest req, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();
            if (req.Amount <= 0 || req.Amount > user.Balance) return BadRequest("Invalid stake");

            // Withdraw stake
            user.Balance -= req.Amount;

            // Spin (0-36)
            int roll = GetRandomInt(0, 37);
            bool win = EvaluateRoulette(roll, req.Choice); // implement evaluation
            decimal payout = 0m;
            if (win) payout = req.Amount * 2m; // example even money

            // Apply payout
            user.Balance += payout;

            var bet = new Bet
            {
                UserId = userId,
                Game = "Roulette",
                Amount = req.Amount,
                Choice = req.Choice,
                Payout = payout
            };
            _db.Bets.Add(bet);
            await _db.SaveChangesAsync();

            return Ok(new { roll, payout, balance = user.Balance });
        }

        private int GetRandomInt(int min, int max)
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            int val = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return min + (val % (max - min));
        }

        private bool EvaluateRoulette(int roll, string choice)
        {
            // Simple evaluation: supports "Red", "Black", or a number as string (e.g., "17")
            if (int.TryParse(choice, out int numberChoice))
                return roll == numberChoice;
            if (choice.Equals("Red", StringComparison.OrdinalIgnoreCase))
                return IsRed(roll);
            if (choice.Equals("Black", StringComparison.OrdinalIgnoreCase))
                return IsBlack(roll);
            return false;
        }

        // Helper: basic roulette color logic (European wheel)
        private bool IsRed(int n)
        {
            int[] reds = {1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36};
            return reds.Contains(n);
        }
        private bool IsBlack(int n)
        {
            int[] blacks = {2,4,6,8,10,11,13,15,17,20,22,24,26,28,29,31,33,35};
            return blacks.Contains(n);
        }
    }

}
