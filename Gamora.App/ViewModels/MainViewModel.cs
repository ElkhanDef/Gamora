using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Abstractions;
using Gamora.Core.Services;
using Serilog;

namespace Gamora.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Faz 1 geliştirme yolu; PathResolver ve settings.json geldiğinde {GAMEDISK} ile değişecek.
    private const string CatalogPath = @"C:\GamoraData\catalog.json";
    private const string CoversDirectory = @"C:\GamoraData\covers";

    public const string AllCategoriesLabel = "Tümü";

    private readonly ICatalogService _catalogService;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _searchDebounceTimer;

    [ObservableProperty]
    private ObservableCollection<GameViewModel> _games = [];

    [ObservableProperty]
    private ObservableCollection<string> _categories = [AllCategoriesLabel];

    [ObservableProperty]
    private string _selectedCategory = AllCategoriesLabel;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _clockText = DateTime.Now.ToString("HH:mm");

    // Games'in filtrelenmiş görünümü. ICollectionView (CollectionViewSource.GetDefaultView)
    // seçildi çünkü: (1) arama/kategori değiştiğinde ikinci bir ObservableCollection'ı elle
    // temizleyip yeniden doldurmaya gerek kalmıyor — tek gerçek kaynak Games kalıyor;
    // (2) Refresh() sadece eşleşme durumu değişen öğeleri gösterir/gizler, hâlâ eşleşen
    // kartların container'ları yeniden oluşmaz — her tuş vuruşunda 200 kartın açılış
    // animasyonunu baştan oynatmaz; (3) VirtualizingWrapPanel filtrelenmiş view üzerinden
    // Count/indexer okuduğu için virtualization sorunsuz çalışır.
    public ICollectionView GamesView { get; private set; }

    // Şimdilik katalogdaki ilk oyun; popülerlik/istatistik servisi gelince ona bağlanacak.
    public GameViewModel? FeaturedGame => Games.FirstOrDefault();

    public MainViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;

        GamesView = CollectionViewSource.GetDefaultView(Games);
        GamesView.Filter = MatchesFilter;

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText = DateTime.Now.ToString("HH:mm");
        _clockTimer.Start();

        _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        _searchDebounceTimer.Tick += (_, _) =>
        {
            _searchDebounceTimer.Stop();
            GamesView.Refresh();
        };
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
            Categories = new ObservableCollection<string>([AllCategoriesLabel, .. catalog.Categories]);

            Log.Information("Katalog yüklendi: {Count} oyun", Games.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Katalog yüklenemedi: {Path}", CatalogPath);
        }
    }

    partial void OnGamesChanged(ObservableCollection<GameViewModel> value)
    {
        OnPropertyChanged(nameof(FeaturedGame));

        GamesView = CollectionViewSource.GetDefaultView(value);
        GamesView.Filter = MatchesFilter;
        OnPropertyChanged(nameof(GamesView));
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        GamesView.Refresh();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        _searchDebounceTimer.Stop();
        SearchText = "";
        GamesView.Refresh();
    }

    private bool MatchesFilter(object obj)
    {
        if (obj is not GameViewModel game)
        {
            return false;
        }

        if (SelectedCategory != AllCategoriesLabel && game.Category != SelectedCategory)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return TurkishSearch.Normalize(game.Name).Contains(TurkishSearch.Normalize(SearchText), StringComparison.Ordinal);
    }
}
