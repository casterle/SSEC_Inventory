using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog;
using SSEC_Inventory.Models;

namespace SSEC_Inventory.Services
{
    /// <summary>
    /// Service for managing premium feature usage tracking and analytics
    /// </summary>
    public class PremiumFeatureManager
    {
        private readonly string _connectionString;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the PremiumFeatureManager
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public PremiumFeatureManager(string databasePath = "premium_features.db")
        {
            _connectionString = $"Data Source={databasePath}";
            InitializeDatabase();
        }

        /// <summary>
        /// Creates the database schema if it doesn't exist
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS PremiumFeatureUsage (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FeatureName TEXT NOT NULL UNIQUE,
                        UsageCount INTEGER NOT NULL DEFAULT 0,
                        LastUsed DATETIME NOT NULL,
                        FirstUsed DATETIME NOT NULL,
                        UsageLimit INTEGER NOT NULL DEFAULT 0,
                        IsActive BOOLEAN NOT NULL DEFAULT 1,
                        Notes TEXT
                    )";

                using var command = new SqliteCommand(createTableSql, connection);
                command.ExecuteNonQuery();

                Log.Information("Premium feature usage database initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize premium feature usage database");
                throw;
            }
        }

        /// <summary>
        /// Records usage of a premium feature
        /// </summary>
        /// <param name="featureName">Name of the feature being used</param>
        /// <param name="usageLimit">Maximum allowed usage (0 for unlimited)</param>
        /// <returns>True if usage was recorded successfully, false if limit exceeded</returns>
        public async Task<bool> RecordFeatureUsageAsync(string featureName, int usageLimit = 0)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));

            lock (_lockObject)
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    // Get current usage
                    var currentUsage = GetFeatureUsage(featureName, connection);
                    
                    // Check if limit would be exceeded
                    if (currentUsage != null && currentUsage.UsageLimit > 0 && 
                        currentUsage.UsageCount >= currentUsage.UsageLimit)
                    {
                        Log.Warning("Premium feature {FeatureName} usage limit exceeded. Current: {Current}, Limit: {Limit}", 
                                  featureName, currentUsage.UsageCount, currentUsage.UsageLimit);
                        return false;
                    }

                    DateTime now = DateTime.UtcNow;

                    if (currentUsage == null)
                    {
                        // Create new usage record
                        string insertSql = @"
                            INSERT INTO PremiumFeatureUsage 
                            (FeatureName, UsageCount, LastUsed, FirstUsed, UsageLimit, IsActive) 
                            VALUES (@featureName, 1, @now, @now, @usageLimit, 1)";

                        using var insertCommand = new SqliteCommand(insertSql, connection);
                        insertCommand.Parameters.AddWithValue("@featureName", featureName);
                        insertCommand.Parameters.AddWithValue("@now", now);
                        insertCommand.Parameters.AddWithValue("@usageLimit", usageLimit);
                        insertCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        // Update existing usage record
                        string updateSql = @"
                            UPDATE PremiumFeatureUsage 
                            SET UsageCount = UsageCount + 1, LastUsed = @now, UsageLimit = @usageLimit 
                            WHERE FeatureName = @featureName";

                        using var updateCommand = new SqliteCommand(updateSql, connection);
                        updateCommand.Parameters.AddWithValue("@now", now);
                        updateCommand.Parameters.AddWithValue("@usageLimit", usageLimit);
                        updateCommand.Parameters.AddWithValue("@featureName", featureName);
                        updateCommand.ExecuteNonQuery();
                    }

                    Log.Information("Premium feature usage recorded: {FeatureName}", featureName);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to record premium feature usage for {FeatureName}", featureName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets usage information for a specific premium feature
        /// </summary>
        /// <param name="featureName">Name of the feature</param>
        /// <returns>Feature usage information or null if not found</returns>
        public PremiumFeatureUsage? GetFeatureUsage(string featureName)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return GetFeatureUsage(featureName, connection);
        }

        /// <summary>
        /// Internal method to get feature usage with existing connection
        /// </summary>
        private PremiumFeatureUsage? GetFeatureUsage(string featureName, SqliteConnection connection)
        {
            string selectSql = @"
                SELECT Id, FeatureName, UsageCount, LastUsed, FirstUsed, UsageLimit, IsActive, Notes 
                FROM PremiumFeatureUsage 
                WHERE FeatureName = @featureName";

            using var command = new SqliteCommand(selectSql, connection);
            command.Parameters.AddWithValue("@featureName", featureName);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PremiumFeatureUsage
                {
                    Id = reader.GetInt32("Id"),
                    FeatureName = reader.GetString("FeatureName"),
                    UsageCount = reader.GetInt32("UsageCount"),
                    LastUsed = reader.GetDateTime("LastUsed"),
                    FirstUsed = reader.GetDateTime("FirstUsed"),
                    UsageLimit = reader.GetInt32("UsageLimit"),
                    IsActive = reader.GetBoolean("IsActive"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes")
                };
            }

            return null;
        }

        /// <summary>
        /// Gets all premium feature usage records
        /// </summary>
        /// <returns>List of all premium feature usage records</returns>
        public async Task<List<PremiumFeatureUsage>> GetAllFeatureUsageAsync()
        {
            var results = new List<PremiumFeatureUsage>();

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                string selectSql = @"
                    SELECT Id, FeatureName, UsageCount, LastUsed, FirstUsed, UsageLimit, IsActive, Notes 
                    FROM PremiumFeatureUsage 
                    ORDER BY LastUsed DESC";

                using var command = new SqliteCommand(selectSql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new PremiumFeatureUsage
                    {
                        Id = reader.GetInt32("Id"),
                        FeatureName = reader.GetString("FeatureName"),
                        UsageCount = reader.GetInt32("UsageCount"),
                        LastUsed = reader.GetDateTime("LastUsed"),
                        FirstUsed = reader.GetDateTime("FirstUsed"),
                        UsageLimit = reader.GetInt32("UsageLimit"),
                        IsActive = reader.GetBoolean("IsActive"),
                        Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes")
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all premium feature usage records");
                throw;
            }

            return results;
        }

        /// <summary>
        /// Resets usage count for a specific feature
        /// </summary>
        /// <param name="featureName">Name of the feature to reset</param>
        public async Task ResetFeatureUsageAsync(string featureName)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                string updateSql = @"
                    UPDATE PremiumFeatureUsage 
                    SET UsageCount = 0, FirstUsed = @now, LastUsed = @now 
                    WHERE FeatureName = @featureName";

                using var command = new SqliteCommand(updateSql, connection);
                command.Parameters.AddWithValue("@now", DateTime.UtcNow);
                command.Parameters.AddWithValue("@featureName", featureName);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    Log.Information("Premium feature usage reset for {FeatureName}", featureName);
                }
                else
                {
                    Log.Warning("No premium feature found with name {FeatureName} to reset", featureName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reset premium feature usage for {FeatureName}", featureName);
                throw;
            }
        }

        /// <summary>
        /// Gets a summary of premium feature usage statistics
        /// </summary>
        /// <returns>Dictionary containing usage statistics</returns>
        public async Task<Dictionary<string, object>> GetUsageSummaryAsync()
        {
            try
            {
                var allUsage = await GetAllFeatureUsageAsync();
                
                return new Dictionary<string, object>
                {
                    ["TotalFeatures"] = allUsage.Count,
                    ["ActiveFeatures"] = allUsage.Count(f => f.IsActive),
                    ["TotalUsageCount"] = allUsage.Sum(f => f.UsageCount),
                    ["FeaturesAtLimit"] = allUsage.Count(f => f.IsLimitReached),
                    ["MostUsedFeature"] = allUsage.OrderByDescending(f => f.UsageCount).FirstOrDefault()?.FeatureName ?? "None",
                    ["LastActivity"] = allUsage.Any() ? allUsage.Max(f => f.LastUsed) : (DateTime?)null
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate premium feature usage summary");
                throw;
            }
        }
    }
}