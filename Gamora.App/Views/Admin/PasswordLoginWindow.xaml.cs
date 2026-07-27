using Gamora.App.ViewModels.Admin;
using Serilog;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Gamora.App.Views.Admin;

/// <summary>
/// Interaction logic for PasswordLoginWindow.xaml
/// </summary>
public partial class PasswordLoginWindow : FluentWindow
{
    public PasswordLoginWindow(PasswordLoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);

        viewModel.LoginSucceeded += (_, _) =>
        {
            Log.Information("Admin girişi: giriş penceresi kapatılıyor");
            DialogResult = true;
            Close();
        };
    }
}
