namespace Casino.Backend.Enums
{
    /// <summary>
    /// KYC verification status for users
    /// </summary>
    public enum KycStatus
    {
      NotStarted = 0,
        Pending = 1,
  UnderReview = 2,
    Verified = 3,
        Rejected = 4,
        Expired = 5
    }

    /// <summary>
    /// Type of KYC document
    /// </summary>
    public enum DocumentType
    {
        Passport = 1,
        NationalId = 2,
        DriversLicense = 3,
        UtilityBill = 4,
BankStatement = 5,
        ProofOfAddress = 6
    }
}
