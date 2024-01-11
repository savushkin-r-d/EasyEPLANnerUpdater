using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Updater;

/// <summary>
/// Логика взаимодействия для SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Owner is MainWindow mainWindow)
        {
            if (string.IsNullOrEmpty(Pat.Text) is false)
            {
                mainWindow.Model.GitHub.Credentials = new Credentials(Pat.Text);
            }
            else
            {
                mainWindow.Model.GitHub.Credentials = Credentials.Anonymous;
            }
            _ = CheckPAT();
        }
    }

    private async Task CheckPAT()
    {
        if (Owner is MainWindow mainWindow)
        {
            try
            {
                await mainWindow.Model.GitHub.User.Current();
                Pass.Visibility = Visibility.Visible;
                NeedSetPAT.Visibility = Visibility.Collapsed;
                Failure.Visibility = Visibility.Collapsed;
                ShowPullRequests.IsEnabled = true;
                ReviewRequested.IsEnabled = true;

                Pat.Visibility = Visibility.Hidden;
                CreateTokenButton.Visibility = Visibility.Hidden;
            }
            catch 
            {
                ResetPAT_Click(this, new RoutedEventArgs());
                Pass.Visibility = Visibility.Collapsed;
                NeedSetPAT.Visibility = Visibility.Visible;
                Failure.Visibility = Visibility.Visible;
                Settings.Default.ShowPullRequests = false;
                Settings.Default.Save();
                ShowPullRequests.IsEnabled = false;
                ReviewRequested.IsEnabled = false;
                if (RunModeComboBox.SelectedIndex == RunMode.ThereAreUpdatesOrReviewRequested)
                    RunModeComboBox.SelectedIndex = RunMode.ThereAreUpdates;
                Pat.Visibility = Visibility.Visible;

                CreateTokenButton.Visibility = Visibility.Visible;
            }
            _ = mainWindow.Model.CheckPAT();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _ = CheckPAT();
    }

    private void CreateTokenButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/settings/tokens/new")
            {
                UseShellExecute = true
            });
    }

    private void ResetPAT_Click(object sender, RoutedEventArgs e)
    {
        Pat.Text = string.Empty;
        Settings.Default.PAT = string.Empty;
        Settings.Default.Save();
    }

    private void Pat_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            // Enable copy/paste and selection of all text.
            case Key.C:
            case Key.V:
            case Key.A:
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    return;
                break;

            // Enable keyboard navigation/selection.
            case Key.Left:
            case Key.Up:
            case Key.Right:
            case Key.Down:
            case Key.PageUp:
            case Key.PageDown:
            case Key.Home:
            case Key.End:
                return;
        }
        e.Handled = true;
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                break;
        }
    }

    private void PATHelpButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/savushkin-r-d/EasyEPLANnerUpdater/blob/manual-readme/README.md#как-создать-pat")
        {
            UseShellExecute = true
        });
    }
}
