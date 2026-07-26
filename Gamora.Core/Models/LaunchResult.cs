using System.Diagnostics;

namespace Gamora.Core.Models;

public sealed class LaunchResult
{
    public bool Success { get; }

    public string? ErrorMessage { get; }

    // URI tabanlı başlatmalarda (steam://, battlenet:// vb.) izlenebilir bir process her zaman
    // dönmeyebilir — Success=true iken Process null olabilir. Çağıran taraf bu durumda süreç
    // takibi yapamayacağını bilir (bkz. GameLauncher/MainViewModel).
    public Process? Process { get; }

    private LaunchResult(bool success, string? errorMessage, Process? process)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Process = process;
    }

    public static LaunchResult Ok(Process? process) => new(true, null, process);

    public static LaunchResult Failure(string errorMessage) => new(false, errorMessage, null);
}
