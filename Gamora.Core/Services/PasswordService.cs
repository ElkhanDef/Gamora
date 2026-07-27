using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

// Tüm kripto mantığı burada toplanır — UI katmanında hiç kripto kodu olmaz.
// Düz metin/SHA256/MD5 KABUL EDİLMEZ: PBKDF2 (Rfc2898DeriveBytes), rastgele salt,
// en az 100.000 iterasyon.
public sealed class PasswordService : IPasswordService
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 100_000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public bool IsPasswordSet(string lockFilePath) => File.Exists(lockFilePath);

    public async Task<PasswordSetupResult> SetPasswordAsync(string lockFilePath, string password, CancellationToken cancellationToken = default)
    {
        // Reset saldırısına karşı: bir sfr.lock zaten varsa üzerine yazılmaz. Akışı başlatan
        // taraf (PasswordSetupViewModel) bu ekranı zaten yalnızca dosya yokken açıyor; burası
        // yarış durumuna karşı ikinci bir güvenlik katmanı.
        if (File.Exists(lockFilePath))
        {
            return PasswordSetupResult.Failure("Yönetici şifresi zaten belirlenmiş.");
        }

        try
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            var hash = await Task.Run(() => DeriveHash(password, salt, Iterations), cancellationToken);

            var lockData = new AdminLock
            {
                Salt = Convert.ToBase64String(salt),
                Iterations = Iterations,
                Hash = Convert.ToBase64String(hash)
            };

            var directory = Path.GetDirectoryName(lockFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = lockFilePath + ".tmp";
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, lockData, JsonOptions, cancellationToken);
            }

            if (File.Exists(lockFilePath))
            {
                File.Replace(tempPath, lockFilePath, destinationBackupFileName: null);
            }
            else
            {
                File.Move(tempPath, lockFilePath);
            }

            return PasswordSetupResult.Ok();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Yönetici şifresi kaydedilemedi: {Path}", lockFilePath);
            return PasswordSetupResult.Failure("Yönetici kurulumu yalnızca sunucuda yapılabilir.");
        }
    }

    public async Task<bool> VerifyPasswordAsync(string lockFilePath, string password, CancellationToken cancellationToken = default)
    {
        AdminLock? lockData;
        try
        {
            await using var stream = File.OpenRead(lockFilePath);
            lockData = await JsonSerializer.DeserializeAsync<AdminLock>(stream, JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "sfr.lock okunamadı: {Path}", lockFilePath);
            return false;
        }

        if (string.IsNullOrEmpty(lockData?.Salt) || string.IsNullOrEmpty(lockData.Hash) || lockData.Iterations <= 0)
        {
            return false;
        }

        var salt = Convert.FromBase64String(lockData.Salt);
        var expectedHash = Convert.FromBase64String(lockData.Hash);
        var actualHash = await Task.Run(() => DeriveHash(password, salt, lockData.Iterations), cancellationToken);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] DeriveHash(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSizeBytes);
    }
}
