using Octokit;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Updater;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowModel Model { get; init; }

    public MainWindow()
    {
        InitializeComponent();
        Model = new(this);
    }

    public async Task InitialyzeData()
    {
        await Task.Run(() => 
        {
            _ = Model.InitialyzeReleses();
            Model.CheckUpdatesApp();
            if (Settings.Default.ShowPullRequests)
            {
                Model.InitializePullRequests();
                Dispatcher.Invoke(RefreshCancel);
            }
        });
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetUpItems();
        _ = Model.CheckPAT();
    }

    private void SetUpItems()
    {
        var latestsReleases = Model.GetLatestsReleases();
        if (latestsReleases is not null)
        {
            ReleasesView.ItemsSource = Model.GetLatestsReleases();
            ViewAllRelease.Visibility = Visibility.Visible;
        }
        else
        {
            ReleasesView.ItemsSource = Model.Releases;
            ViewAllRelease.Visibility = Visibility.Hidden;
        }

        if (Settings.Default.ShowPullRequests)
        {
            PullRequestView.ItemsSource = null;

            PullRequestView.ItemsSource = Model.PullRequests;

            var collectionView = (CollectionViewSource.GetDefaultView(PullRequestView.ItemsSource) as CollectionView);

            if (collectionView is null)
                return;

            if (collectionView.GroupDescriptions.Count <= 0)
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PullRequestItem.GroupName)));
            if (collectionView.SortDescriptions.Count <= 0)
                collectionView.SortDescriptions.Add(
                    new SortDescription(nameof(PullRequestItem.GroupName), ListSortDirection.Descending));
        }

    }

    public CancellationTokenSource? LoadingTokenSource { get; private set; } = null;

    public async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadingTokenSource?.Cancel();
        LoadingTokenSource = new CancellationTokenSource();

        ReleasesView.ItemsSource = null;
        PullRequestView.ItemsSource = null;

        Loading(LoadingTokenSource.Token);
        await InitialyzeData();
    }

    public void RefreshCancel()
    {
        LoadingTokenSource?.Cancel();
        SetUpItems();
    }

    public void Loading(CancellationToken token)
    {
        _ = Dispatcher.Invoke(async() =>
        {
            LoadingState.Visibility = Visibility.Visible;
            State.Visibility = Visibility.Collapsed;
            ViewAllRelease.Visibility = Visibility.Hidden;
            int dots = 0;

            while (!token.IsCancellationRequested)
            {
                LoadingState.Text = $"Загрузка{new string('.', dots++)}";
                if (dots == 4)
                    dots = 0;
                await Task.Delay(250);
            }

            LoadingState.Visibility = Visibility.Collapsed;
        });
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new SettingsDialog
        {
            Owner = this,
        };
        settings.ShowDialog();
    }

    private void Scroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e) 
        => Model.InstallAsset((sender as System.Windows.Controls.Button)?.DataContext as ReleaseItem);

    private void InstallPRArtifactButton_Click(object sender, RoutedEventArgs e)
        => Model.InstallPRArtifact((sender as System.Windows.Controls.Button)?.DataContext as PullRequestItem);

    public async void ResetProgressBarWithDelay()
    {
        await Task.Delay(300);
        Dispatcher.Invoke(() => ProgressBarDownload.Value = 0);
    }

    public double Progress
    {
        get => ProgressBarDownload.Value;
        set => Dispatcher.Invoke(() => ProgressBarDownload.Value = value);
    }

    private void ViewAllRelease_Click(object sender, RoutedEventArgs e)
    {
        ReleasesView.ItemsSource = Model.Releases;
        ViewAllRelease.Visibility = Visibility.Hidden;
        State.Visibility = Visibility.Collapsed;
    }

    private void PullRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.DataContext is PullRequestItem item)
        {
            Process.Start(
            new ProcessStartInfo(item.PullRequest.HtmlUrl)
            {
                UseShellExecute = true
            });
        }
    }

    private void IssueButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.DataContext is PullRequestItem item)
        {
            Process.Start(
            new ProcessStartInfo(item?.Issue?.HtmlUrl ?? "")
            {
                UseShellExecute = true
            });
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {  
        Close();
        if (Model.StartButtonMode)
            Process.Start(Settings.Default.EplanAppPath, "/WithoutUpdater /variant:\"Electric P8\"");
    }

    private void ToolBarWithoutOverflow_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is ToolBar toolBar)
        {
            if (toolBar.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
    private void ReleaseItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Model.InstallAsset((sender as System.Windows.Controls.ListViewItem)?.DataContext as ReleaseItem);
        }
    }

    private void ReleaseItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed &&
            (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            Process.Start(
            new ProcessStartInfo(((sender as System.Windows.Controls.ListViewItem)?.DataContext as ReleaseItem)?.Release.HtmlUrl ?? "")
            {
                UseShellExecute = true
            });
        }
    }

    private void UpdateBTODescriptionButton_Click(object sender, RoutedEventArgs e)
        => Model.UpdateBTODescription();

    private void HelpToWorkApp_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo($"{Settings.Default.ReadMeUrl}использование-приложения")
            {
                UseShellExecute = true
            });
    }
}