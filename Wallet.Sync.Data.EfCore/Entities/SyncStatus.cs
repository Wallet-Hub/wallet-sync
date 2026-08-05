namespace Wallet.Sync.Data.EfCore.Entities
{
    /// <summary>
    /// Outcome of the most recent synchronization attempt tracked by <see cref="SyncState"/>.
    /// </summary>
    public enum SyncStatus
    {
        NotStarted,
        InProgress,
        Succeeded,
        Failed
    }
}
