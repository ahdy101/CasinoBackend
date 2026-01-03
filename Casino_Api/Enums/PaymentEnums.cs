namespace Casino.Backend.Enums
{
    /// <summary>
    /// Payment transaction type
    /// </summary>
    public enum PaymentType
    {
        Deposit = 1,
      Withdrawal = 2
    }

    /// <summary>
    /// Payment provider/gateway
    /// </summary>
    public enum PaymentProvider
    {
        Stripe = 1,
        PayPal = 2,
        Square = 3,
        CoinbaseCommerce = 4,  // Cryptocurrency
     BankTransfer = 5,
        ApplePay = 6,
        GooglePay = 7,
        Manual = 8  // Admin manual processing
    }

    /// <summary>
    /// Payment method used
    /// </summary>
    public enum PaymentMethod
    {
        CreditCard = 1,
   DebitCard = 2,
        BankTransfer = 3,
        PayPalAccount = 4,
    Bitcoin = 5,
        Ethereum = 6,
        ApplePay = 7,
        GooglePay = 8,
        Other = 9
    }

    /// <summary>
    /// Payment transaction status
    /// </summary>
    public enum PaymentStatus
    {
      Pending = 1,     // Payment initiated but not processed
        Processing = 2,    // Being processed by payment provider
        Completed = 3,     // Successfully completed
        Failed = 4,   // Payment failed
     Cancelled = 5,     // User cancelled
        Refunded = 6,      // Payment was refunded
        Disputed = 7,      // Payment disputed/chargeback
        RequiresAction = 8 // Requires user action (3D Secure, etc.)
    }
}
