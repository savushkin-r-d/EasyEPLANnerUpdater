using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Printing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Updater;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public static RunSourceArg SourceArg { get; set; } = RunSourceArg.Default;
    public static int ParrentProcessID { get; set; } = -1;

    public static string CheckUpdatesState { get; set; } = "Проверка наличия обновлений";

    public static CheckUpdates? CheckUpdates { get; set; }

    new public static MainWindow? MainWindow { get; set; }

    public static CancellationTokenSource LoadingTokenSource { get; set; } = new CancellationTokenSource();

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Settings.Default.Save();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length == 2)
        {
            SourceArg = e.Args[0];
            ParrentProcessID = int.Parse(e.Args[1]);
        }

        if (SourceArg == RunSourceArg.AtStartUpEplan && Settings.Default.RunMode == RunMode.Never)
        {
            Environment.Exit(0);
        }

        if (SourceArg == RunSourceArg.AtStartUpEplan && Settings.Default.RunMode == RunMode.ThereAreUpdates)
        {
            Thread newWindowThread = new(new ThreadStart(ThreadUpdateChecker));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        MainWindow = new MainWindow();

        MainWindow.Loading(LoadingTokenSource.Token);

        if (SourceArg == RunSourceArg.FromMenu)
        {
            MainWindow.StartButton.Visibility = Visibility.Collapsed;
        }

        if (SourceArg == RunSourceArg.AtStartUpEplan && Settings.Default.RunMode == RunMode.ThereAreUpdates)
            MainWindow.Model.ReleasesInitialized += Model_ReleasesInitialized;

        _ = MainWindow.InitialyzeData();

        if (SourceArg != RunSourceArg.AtStartUpEplan || Settings.Default.RunMode != RunMode.ThereAreUpdates)
        {
            MainWindow?.Show();
            MainWindow?.Activate();
        }
    }

    private async void Model_ReleasesInitialized(int code)
    {
        await Task.Run(() =>
        {
            CheckUpdates?.Dispatcher.InvokeShutdown();

            MainWindow?.Dispatcher.Invoke(() =>
            {
                MainWindow.Show();
                MainWindow.Activate();
                MainWindow.Model.ReleasesInitialized -= Model_ReleasesInitialized;
            });

        });
    }

    private void ThreadUpdateChecker()
    {
        CheckUpdates = new CheckUpdates();
        CheckUpdates.Show();
        CheckUpdates.Dispatcher.ShutdownStarted += (sender, e) =>
        {
            CheckUpdates?.Close();
            CheckUpdates?.Dispatcher.InvokeShutdown();
            CheckUpdates = null;
        };
        
        Dispatcher.Run();
    }

    public static void UpdateCheckerError(string message)
    {
        if (Settings.Default.RunMode != RunMode.ThereAreUpdates || SourceArg != RunSourceArg.AtStartUpEplan)
            return;

        if (CheckUpdates is not null)
        {
            CheckUpdates.Error(message);
            MainWindow?.Dispatcher.Invoke(() =>
            {
                MainWindow.Close();
            });
        }
    }

    public static void UpdateCheckerPass(string message)
    {
        if (Settings.Default.RunMode != RunMode.ThereAreUpdates || SourceArg != RunSourceArg.AtStartUpEplan)
            return;

        if (CheckUpdates is not null)
        {
            CheckUpdates.Pass(message);
            MainWindow?.Dispatcher.Invoke(() =>
            {
                MainWindow.Close();
            });
        }
    }
}
