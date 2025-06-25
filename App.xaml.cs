using System.Diagnostics;
using System.Windows;
using Serilog;

namespace SSEC_Inventory
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// This class serves as the entry point for the WPF application.
   /// It can be used to handle application-level events such as Startup, Exit, and DispatcherUnhandledException.
   /// </summary>
   public partial class App : Application
   {
      /// <summary>
      /// Application startup logic, including Serilog configuration and global exception handlers.
      /// </summary>
      protected override void OnStartup(StartupEventArgs e)
      {
         // Configure Serilog to log to a local file with daily rolling.
         // Only Information, Warning, Error, and Fatal logs will be written (due to MinimumLevel.Information).
         // The last 7 days of logs are retained; older logs are deleted automatically.
         Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .WriteTo.File(
                 "app.log",
                 rollingInterval: RollingInterval.Day,
                 retainedFileCountLimit: 7 // Keep logs for the last 7 days
             ).CreateLogger();

         Log.Information("Application Starting Up");

         // Subscribe to unhandled exception events for both UI and non-UI threads.
         this.DispatcherUnhandledException += App_DispatcherUnhandledException;
         AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

         base.OnStartup(e);
      }

      /// <summary>
      /// Handles unhandled exceptions on the UI thread.
      /// Logs the exception with additional context and shows a user-friendly message.
      /// </summary>
      private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
      {
         // Gather additional context for diagnostics.
         string user = Environment.UserName;
         string machine = Environment.MachineName;
         string time = DateTime.Now.ToString("u");
         string stackTrace = e.Exception.StackTrace ?? "No stack trace available";

         // Log the exception with context using structured logging.
         Log.Error(e.Exception, "Unhandled UI exception at {Time} on {Machine} by {User}. StackTrace: {StackTrace}", time, machine, user, stackTrace);

         // Optionally, write to Windows Event Log for critical errors.
         try {
            EventLog.WriteEntry("Application", $"Unhandled UI exception: {e.Exception}", EventLogEntryType.Error);
         } catch { /* Ignore if not permitted */ }

         // Show a detailed error dialog to the user (avoid exposing sensitive info).
         MessageBox.Show(
            "An unexpected error occurred and has been logged.\n\n" +
            $"Time: {time}\n" +
            $"User: {user}\n" +
            $"Machine: {machine}\n\n" +
            "The application will now close.",
            "Application Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

         e.Handled = true; // Prevents the default crash dialog

         // Gracefully shut down the application
         Application.Current.Shutdown();
      }

      /// <summary>
      /// Handles unhandled exceptions on non-UI threads.
      /// Logs the exception with additional context for diagnostics.
      /// </summary>
      private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
      {
         // Gather additional context for diagnostics.
         string user = Environment.UserName;
         string machine = Environment.MachineName;
         string time = DateTime.Now.ToString("u");

         // Log the exception or object with context.
         if(e.ExceptionObject is Exception ex) {
            Log.Error(ex, "Unhandled non-UI exception at {Time} on {Machine} by {User}", time, machine, user);
         } else {
            Log.Error("Unhandled non-UI exception (non-Exception object) at {Time} on {Machine} by {User}: {ExceptionObject}", time, machine, user, e.ExceptionObject);
         }


      }

      /// <summary>
      /// Application exit logic, ensures Serilog flushes all logs.
      /// </summary>
      protected override void OnExit(ExitEventArgs e)
      {
         Log.Information("Application Shutting Down");
         Log.CloseAndFlush(); // Ensures all logs are written before exit.
         base.OnExit(e);
      }
   }
}
