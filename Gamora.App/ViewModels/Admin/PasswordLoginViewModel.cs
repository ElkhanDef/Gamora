using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Abstractions;
using Serilog;

namespace Gamora.App.ViewModels.Admin;

public partial class PasswordLoginViewModel : ObservableObject
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);

    private readonly IPasswordService _passwordService;
    private readonly ISettingsService _settingsService;
    private readonly DispatcherTimer _lockoutTimer;

    private int _failedAttempts;
    private DateTime _lockoutEndsAt;

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLockedOut;

    [ObservableProperty]
    private int _lockoutSecondsRemaining;

    // BooleanToVisibilityConverter yalnızca bool->Visibility yapar; IsEnabled bir bool
    // beklediği için burada da ayrı bir hesaplanan property kullanıyoruz.
    public bool IsPasswordFieldEnabled => !IsLockedOut && !IsBusy;

    public event EventHandler? LoginSucceeded;

    public PasswordLoginViewModel(IPasswordService passwordService, ISettingsService settingsService)
    {
        _passwordService = passwordService;
        _settingsService = settingsService;

        _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _lockoutTimer.Tick += (_, _) => UpdateLockoutCountdown();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var settings = await _settingsService.LoadAsync();
            var valid = await _passwordService.VerifyPasswordAsync(settings.AdminLockPath, Password);

            Password = "";

            if (valid)
            {
                Log.Information("Admin girişi: şifre doğrulandı");
                _failedAttempts = 0;
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return;
            }

            _failedAttempts++;

            if (_failedAttempts >= MaxAttempts)
            {
                StartLockout();
            }
            else
            {
                ErrorMessage = $"Hatalı şifre. Kalan deneme: {MaxAttempts - _failedAttempts}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSubmit() => !IsBusy && !IsLockedOut;

    partial void OnIsBusyChanged(bool value)
    {
        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsPasswordFieldEnabled));
    }

    partial void OnIsLockedOutChanged(bool value)
    {
        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsPasswordFieldEnabled));
    }

    private void StartLockout()
    {
        IsLockedOut = true;
        _lockoutEndsAt = DateTime.Now.Add(LockoutDuration);
        LockoutSecondsRemaining = (int)LockoutDuration.TotalSeconds;
        ErrorMessage = $"Çok fazla hatalı deneme. {LockoutSecondsRemaining} saniye bekleyin.";
        _lockoutTimer.Start();
    }

    private void UpdateLockoutCountdown()
    {
        var remaining = _lockoutEndsAt - DateTime.Now;
        if (remaining <= TimeSpan.Zero)
        {
            _lockoutTimer.Stop();
            _failedAttempts = 0;
            LockoutSecondsRemaining = 0;
            ErrorMessage = null;
            IsLockedOut = false;
        }
        else
        {
            LockoutSecondsRemaining = (int)Math.Ceiling(remaining.TotalSeconds);
            ErrorMessage = $"Çok fazla hatalı deneme. {LockoutSecondsRemaining} saniye bekleyin.";
        }
    }
}
