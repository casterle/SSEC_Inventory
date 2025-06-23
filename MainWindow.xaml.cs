using System.Windows;
using System.Windows.Controls;

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
         InitializeComponent();
      }

      /// <summary>
      /// Handles the selection change event for the TabControl.
      /// Displays a popup with the selected tab's header.
      /// </summary>
      private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         // Only handle event if the source is the TabControl itself
         if(e.Source is TabControl) {
            TabItem selectedTab = (TabItem)((TabControl)sender).SelectedItem;
            TabPopupText.Text = $"You selected: {selectedTab.Header}";
            TabPopup.IsOpen = true;
         }
      }
   }
}