using System;
using System.Threading.Tasks;
using SSEC_Inventory.Services;
using Serilog;

namespace SSEC_Inventory
{
    /// <summary>
    /// Console application to demonstrate premium feature usage functionality
    /// This would typically be integrated into the WPF application
    /// </summary>
    public class PremiumFeatureDemo
    {
        private static readonly PremiumFeatureManager _featureManager = new PremiumFeatureManager();

        /// <summary>
        /// Main entry point for the demonstration
        /// </summary>
        public static async Task Main(string[] args)
        {
            // Configure basic logging for demonstration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Console.WriteLine("=== Premium Feature Usage Demo ===\n");
                
                // Initialize sample data
                await InitializeSampleDataAsync();
                
                // Demonstrate feature usage
                await DemonstrateFeatureUsageAsync();
                
                // Show usage statistics
                await ShowUsageStatisticsAsync();
                
                Console.WriteLine("\n=== Demo Complete ===");
                Console.WriteLine("In the WPF application, this data would be displayed in a");
                Console.WriteLine("user-friendly interface with progress bars, charts, and");
                Console.WriteLine("interactive controls for managing feature usage.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Demo failed");
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Initializes sample premium feature data
        /// </summary>
        private static async Task InitializeSampleDataAsync()
        {
            Console.WriteLine("Initializing sample premium feature data...\n");

            // Clear any existing data for clean demo
            try
            {
                await _featureManager.ResetFeatureUsageAsync("Advanced Reporting");
                await _featureManager.ResetFeatureUsageAsync("Bulk Import");
                await _featureManager.ResetFeatureUsageAsync("Data Export");
                await _featureManager.ResetFeatureUsageAsync("API Access");
                await _featureManager.ResetFeatureUsageAsync("Cloud Sync");
                await _featureManager.ResetFeatureUsageAsync("Premium Analytics");
            }
            catch
            {
                // Ignore if features don't exist yet
            }

            // Add various premium features with different usage patterns
            await RecordMultipleUsages("Advanced Reporting", 50, 15);
            await RecordMultipleUsages("Bulk Import", 25, 8);
            await RecordMultipleUsages("Data Export", 100, 45);
            await RecordMultipleUsages("API Access", 0, 127); // Unlimited
            await RecordMultipleUsages("Cloud Sync", 10, 10); // At limit
            await RecordMultipleUsages("Premium Analytics", 30, 5);

            Console.WriteLine("Sample data initialized successfully!\n");
        }

        /// <summary>
        /// Helper method to record multiple usages of a feature
        /// </summary>
        private static async Task RecordMultipleUsages(string featureName, int limit, int usageCount)
        {
            for (int i = 0; i < usageCount; i++)
            {
                bool success = await _featureManager.RecordFeatureUsageAsync(featureName, limit);
                if (!success)
                {
                    Console.WriteLine($"  → {featureName}: Limit reached at {i + 1} usages");
                    break;
                }
            }
        }

        /// <summary>
        /// Demonstrates feature usage recording and limit checking
        /// </summary>
        private static async Task DemonstrateFeatureUsageAsync()
        {
            Console.WriteLine("=== Feature Usage Demonstration ===\n");

            // Try to use a feature that's at its limit
            Console.WriteLine("Attempting to use 'Cloud Sync' (already at limit):");
            bool success = await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
            Console.WriteLine($"  Result: {(success ? "Success" : "Failed - Limit reached")}\n");

            // Try to use a feature with remaining capacity
            Console.WriteLine("Attempting to use 'Premium Analytics' (has capacity):");
            success = await _featureManager.RecordFeatureUsageAsync("Premium Analytics", 30);
            Console.WriteLine($"  Result: {(success ? "Success" : "Failed - Limit reached")}\n");

            // Try to use an unlimited feature
            Console.WriteLine("Attempting to use 'API Access' (unlimited):");
            success = await _featureManager.RecordFeatureUsageAsync("API Access", 0);
            Console.WriteLine($"  Result: {(success ? "Success" : "Failed - Limit reached")}\n");
        }

        /// <summary>
        /// Shows comprehensive usage statistics
        /// </summary>
        private static async Task ShowUsageStatisticsAsync()
        {
            Console.WriteLine("=== Premium Feature Usage Statistics ===\n");

            // Get summary statistics
            var summary = await _featureManager.GetUsageSummaryAsync();
            Console.WriteLine("Summary Statistics:");
            Console.WriteLine($"  Total Features: {summary["TotalFeatures"]}");
            Console.WriteLine($"  Active Features: {summary["ActiveFeatures"]}");
            Console.WriteLine($"  Total Usage Count: {summary["TotalUsageCount"]}");
            Console.WriteLine($"  Features at Limit: {summary["FeaturesAtLimit"]}");
            Console.WriteLine($"  Most Used Feature: {summary["MostUsedFeature"]}");
            Console.WriteLine($"  Last Activity: {summary["LastActivity"]}\n");

            // Get detailed feature information
            var allFeatures = await _featureManager.GetAllFeatureUsageAsync();
            Console.WriteLine("Detailed Feature Usage:");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"Feature Name",-20} {"Usage",-10} {"Limit",-10} {"Remaining",-12} {"Status",-10}");
            Console.WriteLine(new string('-', 80));

            foreach (var feature in allFeatures)
            {
                string usage = $"{feature.UsageCount}";
                string limit = feature.UsageLimit == 0 ? "Unlimited" : feature.UsageLimit.ToString();
                string remaining = feature.UsageLimit == 0 ? "Unlimited" : feature.RemainingUsage.ToString();
                string status = feature.IsLimitReached ? "AT LIMIT" : "Available";
                
                Console.WriteLine($"{feature.FeatureName,-20} {usage,-10} {limit,-10} {remaining,-12} {status,-10}");
            }
            
            Console.WriteLine(new string('-', 80));
        }
    }
}