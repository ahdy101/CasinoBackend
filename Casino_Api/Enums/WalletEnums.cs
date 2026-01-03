namespace Casino.Backend.Enums
{
    /// <summary>
    /// Types of wallet transactions
    /// </summary>
    public enum WalletTransactionType
    {
        /// <summary>
        /// Money added to wallet (deposit/top-up)
        /// </summary>
        Deposit = 1,

        /// <summary>
     /// Money withdrawn from wallet
        /// </summary>
        Withdrawal = 2,

        /// <summary>
        /// Bet placed (debit)
        /// </summary>
        Bet = 3,

        /// <summary>
        /// Winnings paid out (credit)
        /// </summary>
        Payout = 4,

        /// <summary>
   /// Bonus credited to account
        /// </summary>
        Bonus = 5,

        /// <summary>
        /// Refund issued
        /// </summary>
        Refund = 6,

        /// <summary>
        /// Manual adjustment by admin
/// </summary>
        AdminAdjustment = 7,

  /// <summary>
        /// Promotional credit
        /// </summary>
        Promotion = 8,

 /// <summary>
  /// Cashback reward
  /// </summary>
        Cashback = 9,

 /// <summary>
  /// Transaction fee charged
        /// </summary>
        Fee = 10
    }

 /// <summary>
  /// Status of wallet transaction
 /// </summary>
    public enum WalletTransactionStatus
    {
        /// <summary>
  /// Transaction is pending processing
        /// </summary>
        Pending = 1,

 /// <summary>
        /// Transaction completed successfully
        /// </summary>
        Completed = 2,

        /// <summary>
 /// Transaction failed
        /// </summary>
        Failed = 3,

        /// <summary>
    /// Transaction was reversed/refunded
        /// </summary>
    Reversed = 4,

        /// <summary>
        /// Transaction is being processed
        /// </summary>
        Processing = 5
    }
}
