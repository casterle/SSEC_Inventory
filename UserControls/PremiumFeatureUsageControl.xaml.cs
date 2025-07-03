using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using SSEC_Inventory.Models;
using SSEC_Inventory.Services;

namespace SSEC_Inventory.UserControls
{
    /// <summary>
    /// Interaction logic for PremiumFeatureUsageControl.xaml
    /// </summary>
    public partial class PremiumFeatureUsageControl : UserControl
    {
        private readonly PremiumFeatureManager _featureManager;
        private List<PremiumFeatureUsage> _currentUsageData;

        /// <summary>
        /// Initializes a new instance of the PremiumFeatureUsageControl
        /// </summary>
        public PremiumFeatureUsageControl()
        {
            InitializeComponent();
            _featureManager = new PremiumFeatureManager();
            _currentUsageData = new List<PremiumFeatureUsage>();
            
            // Load data when control is loaded
            Loaded += PremiumFeatureUsageControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event to initialize data when control is displayed
        /// </summary>
        private async void PremiumFeatureUsageControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsageDataAsync();
        }

        /// <summary>
        /// Loads premium feature usage data and updates the UI
        /// </summary>
        private async System.Threading.Tasks.Task LoadUsageDataAsync()
        {
            try
            {
                ShowLoadingIndicator(true);
                
                Log.Information("Loading premium feature usage data");

                // Load all usage data
                _currentUsageData = await _featureManager.GetAllFeatureUsageAsync();
                
                // Load summary statistics
                var summary = await _featureManager.GetUsageSummaryAsync();
                
                // Update UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    UpdateSummaryStatistics(summary);
                    UpdateFeatureUsageList(_currentUsageData);
                    LastUpdatedText.Text = $"Last updated: {DateTime.Now:MM/dd/yyyy HH:mm:ss}";
                });

                Log.Information("Premium feature usage data loaded successfully. Found {Count} features", 
                              _currentUsageData.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load premium feature usage data");
                
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        "Failed to load premium feature usage data. Please check the logs for more details.", 
                        "Error Loading Data", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        /// <summary>
        /// Updates the summary statistics display
        /// </summary>
        /// <param name="summary">Dictionary containing summary statistics</param>
        private void UpdateSummaryStatistics(Dictionary<string, object> summary)
        {
            try
            {
                TotalFeaturesText.Text = $"Total Features: {summary.GetValueOrDefault("TotalFeatures", 0)}";
                ActiveFeaturesText.Text = $"Active Features: {summary.GetValueOrDefault("ActiveFeatures", 0)}";
                TotalUsageText.Text = $"Total Usage: {summary.GetValueOrDefault("TotalUsageCount", 0)}";
                FeaturesAtLimitText.Text = $"At Limit: {summary.GetValueOrDefault("FeaturesAtLimit", 0)}";
                MostUsedFeatureText.Text = $"Most Used: {summary.GetValueOrDefault("MostUsedFeature", "None")}";
                
                var lastActivity = summary.GetValueOrDefault("LastActivity", null) as DateTime?;
                LastActivityText.Text = lastActivity.HasValue 
                    ? $"Last Activity: {lastActivity.Value:MM/dd/yyyy HH:mm}"
                    : "Last Activity: None";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update summary statistics");
            }
        }

        /// <summary>
        /// Updates the feature usage list display
        /// </summary>
        /// <param name="usageData">List of premium feature usage data</param>
        private void UpdateFeatureUsageList(List<PremiumFeatureUsage> usageData)
        {
            try
            {
                FeatureUsageList.ItemsSource = usageData;
                
                if (!usageData.Any())
                {
                    // Show message when no data is available
                    var noDataMessage = new TextBlock
                    {
                        Text = "No premium feature usage data available.",
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20)
                    };
                    
                    FeatureUsageList.Items.Clear();
                    FeatureUsageList.Items.Add(noDataMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update feature usage list");
            }
        }

        /// <summary>
        /// Shows or hides the loading indicator
        /// </summary>
        /// <param name="show">True to show the loading indicator, false to hide it</param>
        private void ShowLoadingIndicator(bool show)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Handles the Refresh button click event
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Premium feature usage refresh requested");
            await LoadUsageDataAsync();
        }

        /// <summary>
        /// Handles the Reset Selected button click event
        /// </summary>
        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFeature = FeatureUsageList.SelectedItem as PremiumFeatureUsage;
                if (selectedFeature == null)
                {
                    MessageBox.Show(
                        "Please select a feature to reset from the list.", 
                        "No Feature Selected", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to reset the usage count for '{selectedFeature.FeatureName}'?\n\n" +
                    "This action cannot be undone.",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ShowLoadingIndicator(true);
                    
                    await _featureManager.ResetFeatureUsageAsync(selectedFeature.FeatureName);
                    
                    Log.Information("Premium feature usage reset for {FeatureName}", selectedFeature.FeatureName);
                    
                    // Reload data to reflect changes
                    await LoadUsageDataAsync();
                    
                    MessageBox.Show(
                        $"Usage count for '{selectedFeature.FeatureName}' has been reset successfully.", 
                        "Reset Complete", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reset premium feature usage");
                
                MessageBox.Show(
                    "Failed to reset feature usage. Please check the logs for more details.", 
                    "Reset Failed", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        /// <summary>
        /// Public method to add some sample data for demonstration purposes
        /// </summary>
        public async System.Threading.Tasks.Task InitializeSampleDataAsync()
        {
            try
            {
                Log.Information("Initializing sample premium feature data");

                // Add some sample premium features
                await _featureManager.RecordFeatureUsageAsync("Advanced Reporting", 50);
                await _featureManager.RecordFeatureUsageAsync("Advanced Reporting", 50);
                await _featureManager.RecordFeatureUsageAsync("Advanced Reporting", 50);
                
                await _featureManager.RecordFeatureUsageAsync("Bulk Import", 25);
                await _featureManager.RecordFeatureUsageAsync("Bulk Import", 25);
                
                await _featureManager.RecordFeatureUsageAsync("Data Export", 100);
                await _featureManager.RecordFeatureUsageAsync("Data Export", 100);
                await _featureManager.RecordFeatureUsageAsync("Data Export", 100);
                await _featureManager.RecordFeatureUsageAsync("Data Export", 100);
                await _featureManager.RecordFeatureUsageAsync("Data Export", 100);
                
                await _featureManager.RecordFeatureUsageAsync("API Access", 0); // Unlimited
                await _featureManager.RecordFeatureUsageAsync("API Access", 0);
                
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10);
                await _featureManager.RecordFeatureUsageAsync("Cloud Sync", 10); // This one will reach the limit
                
                Log.Information("Sample premium feature data initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize sample premium feature data");
            }
        }
    }
}