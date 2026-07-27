using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface IPasswordService
{
    bool IsPasswordSet(string lockFilePath);

    Task<PasswordSetupResult> SetPasswordAsync(string lockFilePath, string password, CancellationToken cancellationToken = default);

    Task<bool> VerifyPasswordAsync(string lockFilePath, string password, CancellationToken cancellationToken = default);
}
