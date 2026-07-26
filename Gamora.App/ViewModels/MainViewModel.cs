using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Gamora.Core.Abstractions;
using Serilog;

namespace Gamora.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Faz 1 geliştirme yolu; PathResolver ve settings.json geldiğinde {GAMEDISK} ile değişecek.
    private const string CatalogPath = @"C:\GamoraData\catalog.json";
    private const string CoversDirectory = @"C:\GamoraData\covers";

    private readonly ICatalogService _catalogService;
    private readonly DispatcherTimer _clockTimer;

    [ObservableProperty]
    private ObservableCollection<GameViewModel> _games = [];

    [ObservableProperty]
    private string _clockText = DateTime.Now.ToString("HH:mm");

    // Şimdilik katalogdaki ilk oyun; popülerlik/istatistik servisi gelince ona bağlanacak.
    public GameViewModel? FeaturedGame => Games.FirstOrDefault();

    partial void OnGamesChanged(ObservableCollection<GameViewModel> value)
    {
        OnPropertyChanged(nameof(FeaturedGame));
    }

    public MainViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText = DateTime.Now.ToString("HH:mm");
        _clockTimer.Start();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var catalog = await _catalogService.LoadAsync(CatalogPath);

            var visibleGames = catalog.Games
                .Where(g => g.Visible)
                .OrderBy(g => g.SortOrder)
                .Select(g => new GameViewModel(g, CoversDirectory));

            Games = new ObservableCollection<GameViewModel>(visibleGames);
            Log.Information("Katalog yüklendi: {Count} oyun", Games.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Katalog yüklenemedi: {Path}", CatalogPath);
        }
    }
}
