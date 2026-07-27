using System.Text.Json.Serialization;

namespace Gamora.Core.Models;

public sealed class LauncherSettings
{
    // Admin şifre hash'inin tutulduğu dosyanın adı — kasıtlı nötr, dışarıdan içeriği belli
    // olmasın. settings.json'da şifreyle ilgili hiçbir alan YOK; hash sadece bu dosyada,
    // paylaşımlı diskte (dataRoot) durur, dosya sistemi izinleri onu müşteriden korur.
    public const string AdminLockFileName = "sfr.lock";

    // Oyunların kurulu olduğu disk — {GAMEDISK} bu değere çözülür (bkz. IPathResolver).
    // DataRoot'tan bağımsız bir ayardır: kafede ikisi genelde aynı fiziksel diskte olur
    // (ör. G:\) ama farklı köklerde durabilirler (G:\ vs G:\Gamora) ve ayrı kalmalı.
    public string GameDisk { get; set; } = @"C:\GamoraData";

    // Gamora'nın kendi veri kökü. catalog.json, covers/ ve stats/ hepsi buradan türer —
    // aşağıdaki üç property dışında hiçbir yerde ayrıca hardcode edilmemeli.
    public string DataRoot { get; set; } = @"C:\GamoraData";

    // Faz 1 geliştirme varsayılanı: gerçek komut yerine notepad.exe açılır. Kafe kurulumunda
    // exe yanındaki settings.json'da false yapılır.
    public bool TestMode { get; set; } = true;

    [JsonIgnore]
    public string CatalogPath => Path.Combine(DataRoot, "catalog.json");

    [JsonIgnore]
    public string CoversPath => Path.Combine(DataRoot, "covers");

    [JsonIgnore]
    public string StatsPath => Path.Combine(DataRoot, "stats");

    [JsonIgnore]
    public string AdminLockPath => Path.Combine(DataRoot, AdminLockFileName);
}
