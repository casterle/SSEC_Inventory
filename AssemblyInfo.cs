using System.Windows;

//------------------------------------------------------------------------------
// AssemblyInfo.cs
// Contains assembly-level attributes for the WPF application.
//------------------------------------------------------------------------------

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // Specifies that there are no theme-specific resource dictionaries.
                                                // (Used if a resource is not found in the page or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   // Specifies that the generic resource dictionary is located in the source assembly.
                                                // (Used if a resource is not found in the page, app, or any theme-specific resource dictionaries)
)]

// In App.xaml.cs
public partial class App : Application
{
   protected override void OnStartup(StartupEventArgs e)
   {
      this.DispatcherUnhandledException += App_DispatcherUnhandledException;
      base.OnStartup(e);
   }

   private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
   {
      // Log the exception and show a user-friendly message
      MessageBox.Show("An unexpected error occurred. Please contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
   }
}
