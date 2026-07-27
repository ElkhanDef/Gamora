namespace Gamora.Core.Models;

public sealed class PasswordSetupResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private PasswordSetupResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static PasswordSetupResult Ok() => new(true, null);

    public static PasswordSetupResult Failure(string errorMessage) => new(false, errorMessage);
}
