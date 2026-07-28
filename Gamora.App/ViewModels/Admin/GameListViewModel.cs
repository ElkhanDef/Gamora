using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Gamora.Core.Services;
using Serilog;
using Wpf.Ui.Controls;

namespace Gamora.App.ViewModels.Admin;

public partial class GameListViewModel : ObservableObject
{
    private readonly ICatalogService _catalogService;
    private readonly ISettingsService _settingsService;
    private readonly IPathResolver _pathResolver;
    private readonly DispatcherTimer _searchDebounceTimer;

    private Catalog? _catalog;
    private LauncherSettings? _settings;

    [ObservableProperty]
    private ObservableCollection<GameRowViewModel> _games = [];

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ICollectionView GamesView { get; private set; }

    // null = yeni oyun, dolu = düzenleme. AdminMainViewModel bu olayı dinleyip içerik alanını
    // GameEditViewModel'e çevirir — form ayrı bir pencere değil.
    public event EventHandler<Game?>? EditRequested;

    public GameListViewModel(ICatalogService catalogService, ISettingsService settingsService, IPathResolver pathResolver)
    {
        _catalogService = catalogService;
        _settingsService = settingsService;
        _pathResolver = pathResolver;

        GamesView = CollectionViewSource.GetDefaultView(Games);
        GamesView.Filter = MatchesFilter;

        _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        _searchDebounceTimer.Tick += (_, _) =>
        {
            _searchDebounceTimer.Stop();
            GamesView.Refresh();
        };
    }

    public async Task ReloadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            _settings ??= await _settingsService.LoadAsync();
            _catalog = await _catalogService.LoadAsync(_settings.CatalogPath);

            var rows = _catalog.Games
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => new GameRowViewModel(g, _settings.CoversPath))
                .ToList();

            Games = new ObservableCollection<GameRowViewModel>(rows);

            Log.Information("Admin: oyun listesi yüklendi: {Count} oyun (gizliler dahil)", Games.Count);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Katalog okunamadı. Sunucu diskine erişimi kontrol edin.";
            Log.Error(ex, "Admin: oyun listesi yüklenemedi");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public GameEditViewModel CreateEditViewModel(Game? game)
    {
        if (_catalog is null || _settings is null)
        {
            throw new InvalidOperationException("Katalog henüz yüklenmedi.");
        }

        return new GameEditViewModel(game, _catalog, _settings, _catalogService, _pathResolver);
    }

    partial void OnGamesChanged(ObservableCollection<GameRowViewModel> value)
    {
        GamesView = CollectionViewSource.GetDefaultView(value);
        GamesView.Filter = MatchesFilter;
        OnPropertyChanged(nameof(GamesView));
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    [RelayCommand]
    private void NewGame()
    {
        if (_catalog is null || _settings is null)
        {
            return;
        }

        EditRequested?.Invoke(this, null);
    }

    [RelayCommand]
    private void EditGame(GameRowViewModel row)
    {
        if (_catalog is null || _settings is null)
        {
            return;
        }

        EditRequested?.Invoke(this, row.Model);
    }

    [RelayCommand]
    private async Task DeleteGameAsync(GameRowViewModel row)
    {
        if (_catalog is null || _settings is null)
        {
            return;
        }

        var confirm = new MessageBox
        {
            Title = "Oyunu Sil",
            Content = $"\"{row.Name}\" oyununu silmek istediğinize emin misiniz?",
            PrimaryButtonText = "Sil",
            CloseButtonText = "Vazgeç"
        };

        var result = await confirm.ShowDialogAsync();
        if (result != MessageBoxResult.Primary)
        {
            return;
        }

        // Kapak dosyasına dokunulmuyor: covers'ta kalsın, başka bir kayıt aynı dosyayı
        // kullanıyor olabilir. Temizlik ileride ayrı bir bakım adımı.
        _catalog.Games.RemoveAll(g => g.Id == row.Model.Id);
        _catalog.UpdatedAt = DateTime.Now;

        try
        {
            await _catalogService.SaveAsync(_catalog, _settings.CatalogPath);
            Log.Information("Admin: oyun silindi: {Id} {Name}", row.Model.Id, row.Name);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Kaydedilemedi. Diskin salt-okur olmadığından emin olun.";
            Log.Error(ex, "Admin: oyun silinemedi: {Id}", row.Model.Id);
        }
    }

    private bool MatchesFilter(object obj)
    {
        if (obj is not GameRowViewModel row)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return TurkishSearch.Normalize(row.Name).Contains(TurkishSearch.Normalize(SearchText), StringComparison.Ordinal);
    }
}
