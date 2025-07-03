using System;
using System.IO;
using System.Threading.Tasks;
using SSEC_Inventory.Services;
using SSEC_Inventory.Models;

namespace SSEC_Inventory.Tests
{
    /// <summary>
    /// Simple test class to verify premium feature usage functionality
    /// In a real application, this would use a proper testing framework like xUnit or NUnit
    /// </summary>
    public class PremiumFeatureUsageTests
    {
        /// <summary>
        /// Runs all tests and reports results
        /// </summary>
        public static async Task RunAllTests()
        {
            Console.WriteLine("=== Premium Feature Usage Tests ===\n");

            var tests = new PremiumFeatureUsageTests();
            int passed = 0;
            int total = 0;

            // Run individual tests
            passed += await RunTest("Test Basic Feature Recording", tests.TestBasicFeatureRecording);
            total++;

            passed += await RunTest("Test Usage Limits", tests.TestUsageLimits);
            total++;

            passed += await RunTest("Test Unlimited Features", tests.TestUnlimitedFeatures);
            total++;

            passed += await RunTest("Test Usage Statistics", tests.TestUsageStatistics);
            total++;

            passed += await RunTest("Test Feature Reset", tests.TestFeatureReset);
            total++;

            Console.WriteLine($"\n=== Test Results ===");
            Console.WriteLine($"Passed: {passed}/{total}");
            Console.WriteLine($"Success Rate: {(double)passed / total * 100:F1}%");
        }

        /// <summary>
        /// Helper method to run a test and handle exceptions
        /// </summary>
        private static async Task<int> RunTest(string testName, Func<Task> testMethod)
        {
            try
            {
                Console.Write($"{testName}... ");
                await testMethod();
                Console.WriteLine("PASSED");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Test basic feature recording functionality
        /// </summary>
        public async Task TestBasicFeatureRecording()
        {
            string dbPath = Path.GetTempFileName();
            var manager = new PremiumFeatureManager(dbPath);

            // Record a feature usage
            bool result = await manager.RecordFeatureUsageAsync("Test Feature", 10);
            if (!result) throw new Exception("Failed to record feature usage");

            // Verify the feature was recorded
            var usage = manager.GetFeatureUsage("Test Feature");
            if (usage == null) throw new Exception("Feature usage not found");
            if (usage.UsageCount != 1) throw new Exception($"Expected usage count 1, got {usage.UsageCount}");
            if (usage.UsageLimit != 10) throw new Exception($"Expected limit 10, got {usage.UsageLimit}");

            // Clean up
            File.Delete(dbPath);
        }

        /// <summary>
        /// Test usage limit enforcement
        /// </summary>
        public async Task TestUsageLimits()
        {
            string dbPath = Path.GetTempFileName();
            var manager = new PremiumFeatureManager(dbPath);

            // Record usage up to the limit
            for (int i = 0; i < 3; i++)
            {
                bool result = await manager.RecordFeatureUsageAsync("Limited Feature", 3);
                if (!result) throw new Exception($"Failed to record usage {i + 1}");
            }

            // Try to exceed the limit
            bool shouldFail = await manager.RecordFeatureUsageAsync("Limited Feature", 3);
            if (shouldFail) throw new Exception("Should have failed when exceeding limit");

            // Verify the feature is at its limit
            var usage = manager.GetFeatureUsage("Limited Feature");
            if (usage == null) throw new Exception("Feature usage not found");
            if (!usage.IsLimitReached) throw new Exception("Feature should be at limit");
            if (usage.RemainingUsage != 0) throw new Exception($"Expected 0 remaining usage, got {usage.RemainingUsage}");

            // Clean up
            File.Delete(dbPath);
        }

        /// <summary>
        /// Test unlimited features (limit = 0)
        /// </summary>
        public async Task TestUnlimitedFeatures()
        {
            string dbPath = Path.GetTempFileName();
            var manager = new PremiumFeatureManager(dbPath);

            // Record many usages of unlimited feature
            for (int i = 0; i < 100; i++)
            {
                bool result = await manager.RecordFeatureUsageAsync("Unlimited Feature", 0);
                if (!result) throw new Exception($"Failed to record unlimited usage {i + 1}");
            }

            // Verify the feature stats
            var usage = manager.GetFeatureUsage("Unlimited Feature");
            if (usage == null) throw new Exception("Feature usage not found");
            if (usage.UsageCount != 100) throw new Exception($"Expected 100 usages, got {usage.UsageCount}");
            if (usage.IsLimitReached) throw new Exception("Unlimited feature should never reach limit");
            if (usage.RemainingUsage != int.MaxValue) throw new Exception("Unlimited feature should have max remaining usage");

            // Clean up
            File.Delete(dbPath);
        }

        /// <summary>
        /// Test usage statistics calculation
        /// </summary>
        public async Task TestUsageStatistics()
        {
            string dbPath = Path.GetTempFileName();
            var manager = new PremiumFeatureManager(dbPath);

            // Add multiple features with different usage patterns
            await manager.RecordFeatureUsageAsync("Feature A", 10); // 1/10
            await manager.RecordFeatureUsageAsync("Feature A", 10); // 2/10
            
            await manager.RecordFeatureUsageAsync("Feature B", 5);  // 1/5
            await manager.RecordFeatureUsageAsync("Feature B", 5);  // 2/5
            await manager.RecordFeatureUsageAsync("Feature B", 5);  // 3/5
            await manager.RecordFeatureUsageAsync("Feature B", 5);  // 4/5
            await manager.RecordFeatureUsageAsync("Feature B", 5);  // 5/5 (at limit)

            await manager.RecordFeatureUsageAsync("Feature C", 0);  // Unlimited

            // Get summary statistics
            var summary = await manager.GetUsageSummaryAsync();

            // Verify statistics
            if ((int)summary["TotalFeatures"] != 3) 
                throw new Exception($"Expected 3 features, got {summary["TotalFeatures"]}");
            
            if ((int)summary["TotalUsageCount"] != 8) 
                throw new Exception($"Expected 8 total usages, got {summary["TotalUsageCount"]}");
            
            if ((int)summary["FeaturesAtLimit"] != 1) 
                throw new Exception($"Expected 1 feature at limit, got {summary["FeaturesAtLimit"]}");

            // Clean up
            File.Delete(dbPath);
        }

        /// <summary>
        /// Test feature usage reset functionality
        /// </summary>
        public async Task TestFeatureReset()
        {
            string dbPath = Path.GetTempFileName();
            var manager = new PremiumFeatureManager(dbPath);

            // Record some usage
            await manager.RecordFeatureUsageAsync("Reset Test", 10);
            await manager.RecordFeatureUsageAsync("Reset Test", 10);
            await manager.RecordFeatureUsageAsync("Reset Test", 10);

            // Verify initial usage
            var usage = manager.GetFeatureUsage("Reset Test");
            if (usage == null) throw new Exception("Feature not found before reset");
            if (usage.UsageCount != 3) throw new Exception($"Expected 3 usages before reset, got {usage.UsageCount}");

            // Reset the feature
            await manager.ResetFeatureUsageAsync("Reset Test");

            // Verify reset worked
            usage = manager.GetFeatureUsage("Reset Test");
            if (usage == null) throw new Exception("Feature not found after reset");
            if (usage.UsageCount != 0) throw new Exception($"Expected 0 usages after reset, got {usage.UsageCount}");

            // Clean up
            File.Delete(dbPath);
        }
    }
}