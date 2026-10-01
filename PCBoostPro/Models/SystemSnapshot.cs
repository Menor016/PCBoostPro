using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using PCBoostPro.Models;
using PCBoostPro.Services;
using PCBoostPro.ViewModels;

namespace PCBoostPro;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _viewModel.Refresh();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += (_, _) => _viewModel.Refresh();
        _timer.Start();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Refresh();
    }

    private void RunOptimization_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement button || button.DataContext is not OptimizationAction action)
        {
            return;
        }

        _viewModel.ExecuteAction(action);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
