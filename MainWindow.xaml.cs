using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace SSEC_Inventory
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      /// <summary>
      /// Initializes the main window and its components.
      /// </summary>
      public MainWindow()
      {
         Log.Information("Entering MainWindow constructor");
         InitializeComponent();
         Log.Information("Exiting MainWindow constructor");
      }

      /// <summary>
      /// Handles the selection change event for the TabControl.
      /// Displays a popup with the selected tab's header and logs the event.
      /// </summary>
      private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Log.Information("Entering TabControl_SelectionChanged");
         if(e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab) {
            TabPopupText.Text = $"You selected: {selectedTab.Header}";
            TabPopup.IsOpen = true;
            Log.Information("Tab selection changed to {TabHeader}", selectedTab.Header);
         }
         Log.Information("Exiting TabControl_SelectionChanged");
      }

      /// <summary>
      /// Handles the click event for the ThrowException button.
      /// Throws a test exception for the global handler.
      /// </summary>
      private void ThrowException_Click(object sender, RoutedEventArgs e)
      {
         throw new System.Exception("Test exception for global handler");
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