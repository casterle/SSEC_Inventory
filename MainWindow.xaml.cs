using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks; // Ensure this namespace is included

namespace SSEC_Inventory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the selection change logic here
            if (e.Source is TabControl)
            {
                TabItem selectedTab = (TabItem)((TabControl)sender).SelectedItem;
                TabPopupText.Text = $"You selected: {selectedTab.Header}";
                TabPopup.IsOpen = true;
            }
        }
    }
}