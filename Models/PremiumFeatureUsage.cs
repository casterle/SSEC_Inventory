using System;

namespace SSEC_Inventory.Models
{
    /// <summary>
    /// Represents usage data for a premium feature
    /// </summary>
    public class PremiumFeatureUsage
    {
        /// <summary>
        /// Unique identifier for the usage record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the premium feature
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// Number of times the feature has been used
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// Date and time when the feature was last used
        /// </summary>
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// Date and time when the feature was first used
        /// </summary>
        public DateTime FirstUsed { get; set; }

        /// <summary>
        /// Maximum number of times this feature can be used (0 = unlimited)
        /// </summary>
        public int UsageLimit { get; set; }

        /// <summary>
        /// Whether the feature is currently active/available
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Additional notes or description about the feature usage
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Calculates the remaining usage count for this feature
        /// </summary>
        public int RemainingUsage => UsageLimit == 0 ? int.MaxValue : Math.Max(0, UsageLimit - UsageCount);

        /// <summary>
        /// Determines if the feature has reached its usage limit
        /// </summary>
        public bool IsLimitReached => UsageLimit > 0 && UsageCount >= UsageLimit;

        /// <summary>
        /// Calculates the usage percentage (0-100)
        /// </summary>
        public double UsagePercentage => UsageLimit == 0 ? 0 : Math.Min(100, (double)UsageCount / UsageLimit * 100);
    }
}