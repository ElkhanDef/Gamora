using System.Windows;
using System.Windows.Controls;
using Gamora.App.ViewModels.Admin;

namespace Gamora.App.Views.Admin;

/// <summary>
/// Interaction logic for GameRowView.xaml
/// </summary>
public partial class GameRowView : UserControl
{
    public GameRowView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is GameRowViewModel viewModel)
        {
            await viewModel.EnsureCoverLoadedAsync();
        }
    }
}
