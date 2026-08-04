namespace Wallet.Sync.BudgetBakers.Host
{
    /// <summary>
    /// Options bound to the "BudgetBakers" section of appsettings.json.
    /// </summary>
    public class BudgetBakersOptions
    {
        public const string SectionName = "BudgetBakers";

        public string AccessToken { get; set; } = string.Empty;
    }
}
