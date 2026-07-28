using Gamora.Core.Abstractions;
using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class PathResolver : IPathResolver
{
    public string Resolve(string pathTemplate, LauncherSettings settings)
    {
        return pathTemplate.Replace("{GAMEDISK}", settings.GameDisk, StringComparison.OrdinalIgnoreCase);
    }

    // Dosya seçici (admin "Gözat") gerçek bir mutlak yol döner; oyun diskinin altındaysa
    // catalog.json'a yazılmadan önce {GAMEDISK} kalıbına geri çevrilir ki başka bir kafede
    // farklı bir sürücü harfiyle de çalışsın. Kapsam dışındaki yollar (ör. C: altında özel
    // bir kurulum) değiştirilmeden bırakılır.
    //
    // settings.json'daki gameDisk elle düzenlenebildiği için "/" ile de yazılmış olabilir
    // (ör. "C:/GamoraData"), OpenFileDialog ise her zaman "\" döner — ikisini de "\" düzenine
    // getirip karşılaştırıyoruz. Ayrıca gameDisk salt bir sürücü harfi de olabilir (kafede
    // "G:" gibi) ya da sonunda "\" taşıyabilir; sınırı GameDisk + "\" ile arayarak hem bunu
    // hem de "C:\GamoraData" ile "C:\GamoraDataOther\..." gibi komşu bir klasörün yanlışlıkla
    // eşleşmesini (StartsWith'in salt önek kontrolü olması) engelliyoruz.
    public string ToTemplate(string actualPath, LauncherSettings settings)
    {
        if (string.IsNullOrEmpty(actualPath) || string.IsNullOrEmpty(settings.GameDisk))
        {
            return actualPath;
        }

        var normalizedActual = actualPath.Replace('/', '\\');
        var normalizedGameDisk = settings.GameDisk.Replace('/', '\\').TrimEnd('\\');

        if (normalizedActual.Equals(normalizedGameDisk, StringComparison.OrdinalIgnoreCase))
        {
            return "{GAMEDISK}";
        }

        if (normalizedActual.StartsWith(normalizedGameDisk + "\\", StringComparison.OrdinalIgnoreCase))
        {
            return "{GAMEDISK}" + normalizedActual[normalizedGameDisk.Length..];
        }

        return actualPath;
    }
}
