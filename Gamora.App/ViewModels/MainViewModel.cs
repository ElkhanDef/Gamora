using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
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
    public const string AllCategoriesLabel = "Tümü";

    private static readonly TimeSpan OverlayErrorAutoDismiss = TimeSpan.FromSeconds(4);

    private readonly ICatalogService _catalogService;
    private readonly ISettingsService _settingsService;
    private readonly IGameLauncher _gameLauncher;
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

    [ObservableProperty]
    private bool _isOverlayVisible;

    [ObservableProperty]
    private bool _isOverlayError;

    [ObservableProperty]
    private string _overlayGameName = "";

    [ObservableProperty]
    private string _overlayErrorMessage = "";

    // IsOverlayError'ın tersi — XAML'de BooleanToVisibilityConverter'ın ConverterParameter ile
    // ters çalışmasını beklemek yerine (desteklemiyor), ayrı bir hesaplanan property.
    public bool IsOverlayLaunching => !IsOverlayError;

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

    public MainViewModel(ICatalogService catalogService, ISettingsService settingsService, IGameLauncher gameLauncher)
    {
        _catalogService = catalogService;
        _settingsService = settingsService;
        _gameLauncher = gameLauncher;

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
            var settings = await _settingsService.LoadAsync();
            var catalog = await _catalogService.LoadAsync(settings.CatalogPath);

            var visibleGames = catalog.Games
                .Where(g => g.Visible)
                .OrderBy(g => g.SortOrder)
                .Select(g => new GameViewModel(g, settings.CoversPath, LaunchGameAsync));

            Games = new ObservableCollection<GameViewModel>(visibleGames);
            Categories = new ObservableCollection<string>([AllCategoriesLabel, .. catalog.Categories]);

            Log.Information("Katalog yüklendi: {Count} oyun ({CatalogPath})", Games.Count, settings.CatalogPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Katalog yüklenemedi");
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

    partial void OnIsOverlayErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsOverlayLaunching));
    }

    [RelayCommand]
    private void ClearSearch()
    {
        _searchDebounceTimer.Stop();
        SearchText = "";
        GamesView.Refresh();
    }

    [RelayCommand]
    private void DismissOverlay()
    {
        IsOverlayVisible = false;
        IsOverlayError = false;
    }

    private async Task LaunchGameAsync(GameViewModel game)
    {
        OverlayGameName = game.Name;
        IsOverlayError = false;
        IsOverlayVisible = true;

        var result = await _gameLauncher.LaunchAsync(game.Model);

        if (!result.Success)
        {
            IsOverlayError = true;
            OverlayErrorMessage = result.ErrorMessage ?? "Oyun başlatılamadı. Personele haber verin.";

            await Task.Delay(OverlayErrorAutoDismiss);
            if (IsOverlayError)
            {
                IsOverlayVisible = false;
                IsOverlayError = false;
            }

            return;
        }

        IsOverlayVisible = false;
        SetWindowState(WindowState.Minimized);

        // Steam/Riot gibi URI tabanlı başlatmalarda gerçek oyun process'i genelde
        // izlenemez (Process null döner) — bu durumda müşteri launcher'a kendisi döner.
        if (result.Process is not null)
        {
            try
            {
                await result.Process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Süreç bekleme sırasında hata: {GameId}", game.Model.Id);
            }

            SetWindowState(WindowState.Normal);
        }
    }

    private static void SetWindowState(WindowState state)
    {
        if (Application.Current?.MainWindow is { } window)
        {
            window.WindowState = state;
            if (state == WindowState.Normal)
            {
                window.Activate();
            }
        }
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
