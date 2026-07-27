using Gamora.App.ViewModels.Admin;
using Serilog;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Gamora.App.Views.Admin;

/// <summary>
/// Interaction logic for PasswordSetupWindow.xaml
/// </summary>
public partial class PasswordSetupWindow : FluentWindow
{
    public PasswordSetupWindow(PasswordSetupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplicationThemeManager.Apply(this);

        viewModel.SetupSucceeded += (_, _) =>
        {
            Log.Information("Admin kurulumu: kurulum penceresi kapatılıyor");
            DialogResult = true;
            Close();
        };
    }
}
