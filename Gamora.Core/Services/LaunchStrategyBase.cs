using System.Diagnostics;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

public abstract class LaunchStrategyBase : ILaunchStrategy
{
    public abstract LaunchType LaunchType { get; }

    protected abstract LaunchCommand BuildCommand(Game game, LauncherSettings settings);

    public Task<LaunchResult> LaunchAsync(Game game, LauncherSettings settings, CancellationToken cancellationToken = default)
    {
        LaunchCommand command;
        try
        {
            command = BuildCommand(game, settings);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Başlatma komutu oluşturulamadı: {GameId}", game.Id);
            return Task.FromResult(LaunchResult.Failure("Oyun başlatılamadı. Personele haber verin."));
        }

        // Test modunda gerçek komut çalıştırılmaz; sadece hangi komutun çalışacağı loglanır ve
        // yerine zararsız notepad.exe açılır — kafeye gitmeden akışı (kilit, izleme, küçültme)
        // doğrulamak için.
        if (settings.TestMode)
        {
            Log.Information("[TEST] {Command} çalıştırılacaktı", command.Describe());
            command = LaunchCommand.ForFile("notepad.exe");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.FileName,
                UseShellExecute = command.UseShellExecute
            };

            if (!string.IsNullOrEmpty(command.Arguments))
            {
                startInfo.Arguments = command.Arguments;
            }

            if (!string.IsNullOrEmpty(command.WorkingDirectory))
            {
                startInfo.WorkingDirectory = command.WorkingDirectory;
            }

            var process = Process.Start(startInfo);
            Log.Information("Oyun başlatıldı: {GameId} ({Command})", game.Id, command.Describe());
            return Task.FromResult(LaunchResult.Ok(process));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Oyun başlatılamadı: {GameId} ({Command})", game.Id, command.Describe());
            return Task.FromResult(LaunchResult.Failure("Oyun başlatılamadı. Personele haber verin."));
        }
    }
}
