using System.Windows;
using System.Windows.Controls;
using Serilog;
using SSEC_Inventory.Services;

namespace SSEC_Inventory
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private readonly PremiumFeatureManager _premiumFeatureManager;

      /// <summary>
      /// Initializes the main window and its components.
      /// </summary>
      public MainWindow()
      {
         Log.Information("Entering MainWindow constructor");
         InitializeComponent();
         
         // Initialize premium feature manager
         _premiumFeatureManager = new PremiumFeatureManager();
         
         // Initialize sample data when window is loaded
         Loaded += MainWindow_Loaded;
         
         Log.Information("Exiting MainWindow constructor");
      }

      /// <summary>
      /// Handles the window loaded event to initialize premium feature sample data
      /// </summary>
      private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
      {
         try
         {
            Log.Information("Initializing premium feature sample data");
            await PremiumUsageControl.InitializeSampleDataAsync();
            
            // Simulate some feature usage for demonstration
            await SimulatePremiumFeatureUsage();
         }
         catch (System.Exception ex)
         {
            Log.Error(ex, "Failed to initialize premium feature data");
         }
      }

      /// <summary>
      /// Simulates premium feature usage for demonstration purposes
      /// </summary>
      private async System.Threading.Tasks.Task SimulatePremiumFeatureUsage()
      {
         try
         {
            // Record usage when user navigates between tabs (simulate premium features)
            await _premiumFeatureManager.RecordFeatureUsageAsync("Tab Navigation", 100);
            Log.Information("Simulated premium feature usage: Tab Navigation");
         }
         catch (System.Exception ex)
         {
            Log.Error(ex, "Failed to simulate premium feature usage");
         }
      }

      /// <summary>
      /// Handles the selection change event for the TabControl.
      /// Displays a popup with the selected tab's header and logs the event.
      /// Also records premium feature usage for tab navigation.
      /// </summary>
      private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Log.Information("Entering TabControl_SelectionChanged");
         if(e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab) {
            TabPopupText.Text = $"You selected: {selectedTab.Header}";
            TabPopup.IsOpen = true;
            Log.Information("Tab selection changed to {TabHeader}", selectedTab.Header);
            
            // Record premium feature usage for tab navigation
            try
            {
               await _premiumFeatureManager.RecordFeatureUsageAsync("Tab Navigation", 100);
               
               // If it's the Premium Usage tab, record that as a premium feature too
               if (selectedTab.Header.ToString() == "Premium Usage")
               {
                  await _premiumFeatureManager.RecordFeatureUsageAsync("Premium Usage Viewing", 20);
               }
            }
            catch (System.Exception ex)
            {
               Log.Error(ex, "Failed to record premium feature usage for tab navigation");
            }
         }
         Log.Information("Exiting TabControl_SelectionChanged");
      }

      /// <summary>
      /// Destructor for MainWindow. Logs when the window is finalized.
      /// </summary>
      ~MainWindow()
      {
         Log.Information("MainWindow destructor called");
      }
   }
}