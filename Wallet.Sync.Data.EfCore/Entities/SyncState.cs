using System;

namespace Wallet.Sync.Data.EfCore.Entities
{
    /// <summary>
    /// Tracks the synchronization state of an entity type against the external provider,
    /// enabling delta synchronization (via <see cref="LastSyncedAt"/>) and periodic full
    /// reconciliation to detect upstream deletions (via <see cref="LastFullReconciliationAt"/>).
    /// This is a persistence-only concern: it lives exclusively in Wallet.Sync.Data.EfCore
    /// and has no counterpart among the domain entities in Wallet.Sync.Data.
    /// </summary>
    public class SyncState
    {
        public int Id { get; set; }

        /// <summary>
        /// Identifies which entity type this row tracks (e.g. "Account", "Category", "Record").
        /// One row per entity type.
        /// </summary>
        public string EntityType { get; set; } = null!;

        /// <summary>
        /// Timestamp of the last successful delta synchronization. Used as the lower bound
        /// (UpdatedAt >= LastSyncedAt) for the next delta fetch. Null means a full sync is needed.
        /// </summary>
        public DateTime? LastSyncedAt { get; set; }

        /// <summary>
        /// Timestamp of the last successful full reconciliation pass, used to detect entities
        /// deleted upstream (which a delta sync cannot detect). Null means no reconciliation has run yet.
        /// </summary>
        public DateTime? LastFullReconciliationAt { get; set; }

        public SyncStatus Status { get; set; }

        /// <summary>
        /// Diagnostic message from the last failed synchronization attempt, if any.
        /// </summary>
        public string? LastError { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
