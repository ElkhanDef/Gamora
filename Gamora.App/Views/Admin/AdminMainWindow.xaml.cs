using Gamora.App.ViewModels.Admin;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Gamora.App.Views.Admin;

/// <summary>
/// Interaction logic for AdminMainWindow.xaml
/// </summary>
public partial class AdminMainWindow : FluentWindow
{
    public AdminMainWindow(AdminMainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);
    }
}
