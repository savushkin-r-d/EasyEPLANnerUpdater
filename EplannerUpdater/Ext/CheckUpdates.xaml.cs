using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
/// Логика взаимодействия для CheckUpdates.xaml
/// </summary>
public partial class CheckUpdates : Window
{
    public CheckUpdates()
    {
        InitializeComponent();
    }

    public void Error(string message)
    {
        Dispatcher.Invoke(async () =>
        {
            LoadingGif.Visibility = Visibility.Collapsed;
            FailureCheck.Visibility = Visibility.Visible;
            Text.Text = message;
            await Task.Delay(1000);
            Environment.Exit(0);
        }).Wait();
    }

    public void Pass(string message)
    {
        Dispatcher.Invoke(async () =>
        {
            LoadingGif.Visibility = Visibility.Collapsed;
            PassCheck.Visibility = Visibility.Visible;
            Text.Text = message;
            await Task.Delay(1000);
            Environment.Exit(0);
        }).Wait();
    }
}
