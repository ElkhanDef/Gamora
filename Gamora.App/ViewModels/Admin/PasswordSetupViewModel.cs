using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Abstractions;
using Serilog;

namespace Gamora.App.ViewModels.Admin;

public partial class PasswordSetupViewModel : ObservableObject
{
    private const int MinPasswordLength = 6;

    private readonly IPasswordService _passwordService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _confirmPassword = "";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    // Yazma sunucu-dışı bir yerden (salt-okur paylaşım) denendiğinde true olur — akış burada
    // biter, form tekrar denemeye izin vermez (bkz. IsFormEnabled).
    [ObservableProperty]
    private bool _isTerminalError;

    public bool IsFormEnabled => !IsBusy && !IsTerminalError;

    public event EventHandler? SetupSucceeded;

    public PasswordSetupViewModel(IPasswordService passwordService, ISettingsService settingsService)
    {
        _passwordService = passwordService;
        _settingsService = settingsService;
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsFormEnabled));

    partial void OnIsTerminalErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormEnabled));

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ErrorMessage = null;

        if (Password.Length < MinPasswordLength)
        {
            ErrorMessage = $"Şifre en az {MinPasswordLength} karakter olmalı.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Şifreler eşleşmiyor.";
            return;
        }

        IsBusy = true;
        try
        {
            var settings = await _settingsService.LoadAsync();
            var result = await _passwordService.SetPasswordAsync(settings.AdminLockPath, Password);

            if (result.Success)
            {
                Log.Information("Admin kurulumu: şifre kaydedildi");
                SetupSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Yönetici kurulumu yalnızca sunucuda yapılabilir.";
                IsTerminalError = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
