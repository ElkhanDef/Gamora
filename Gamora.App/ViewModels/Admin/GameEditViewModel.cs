using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Gamora.Core.Services;
using Microsoft.Win32;
using Serilog;

namespace Gamora.App.ViewModels.Admin;

public sealed record LaunchTypeOption(LaunchType Value, string Label);

public partial class GameEditViewModel : ObservableObject
{
    private readonly Game? _existingGame;
    private readonly Catalog _catalog;
    private readonly LauncherSettings _settings;
    private readonly ICatalogService _catalogService;
    private readonly IPathResolver _pathResolver;

    public static IReadOnlyList<LaunchTypeOption> LaunchTypeOptions { get; } =
    [
        new(LaunchType.Exe, "EXE (Yerel Program)"),
        new(LaunchType.Steam, "Steam"),
        new(LaunchType.Riot, "Riot Client"),
        new(LaunchType.Battlenet, "Battle.net"),
        new(LaunchType.Epic, "Epic Games")
    ];

    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _selectedCategory = "";

    [ObservableProperty]
    private LaunchType _selectedLaunchType = LaunchType.Exe;

    [ObservableProperty]
    private string _launchTarget = "";

    // Yalnızca steam/riot/battlenet/epic'te anlamlı: işaretlenince hedef alanı pasifleşir ve
    // boş kaydedilir — ilgili strateji belirli bir oyunu değil platformun kendisini açar.
    [ObservableProperty]
    private bool _usePlatformFallback;

    [ObservableProperty]
    private string _workingDir = "";

    [ObservableProperty]
    private string _args = "";

    [ObservableProperty]
    private bool _isAdvancedExpanded;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private double? _sortOrderValue;

    [ObservableProperty]
    private bool _ageRestricted;

    [ObservableProperty]
    private string? _nameError;

    [ObservableProperty]
    private string? _targetError;

    [ObservableProperty]
    private string? _duplicateNameWarning;

    [ObservableProperty]
    private string? _saveErrorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public bool CanSave => !IsBusy;

    public bool IsEditMode => _existingGame is not null;

    public string HeaderTitle => IsEditMode ? "Oyunu Düzenle" : "Yeni Oyun Ekle";

    public string SaveButtonLabel => IsEditMode ? "Kaydet" : "Ekle";

    public string TargetLabel => SelectedLaunchType switch
    {
        LaunchType.Exe => "Çalıştırılabilir Dosya",
        LaunchType.Steam => "Steam AppID",
        LaunchType.Riot => "Ürün Kodu",
        LaunchType.Battlenet => "Ürün Kodu",
        LaunchType.Epic => "Uygulama Kodu",
        _ => "Başlatma Hedefi"
    };

    // steamdb.info Steam'in kendi sitesi değil ama AppID aramak için kafe teknisyenleri
    // arasında yaygın kullanılan bir bilgi kaynağı — DEVELOPMENT.md launchTarget şemasında
    // Steam için sayısal AppID istiyor.
    public string? TargetHint => UsePlatformFallback ? null : SelectedLaunchType switch
    {
        LaunchType.Steam => "steamdb.info'dan bulabilirsiniz.",
        LaunchType.Riot => "Örn. valorant, lor.",
        LaunchType.Battlenet => "Örn. OW, WoW.",
        LaunchType.Epic => "Epic mağaza sayfasındaki uygulama kodu.",
        _ => null
    };

    public bool ShowBrowseButton => SelectedLaunchType == LaunchType.Exe;

    // Exe'de "kodu bilmiyorum" seçeneğinin bir karşılığı yok — yerel programın yolu zaten
    // zorunlu. Diğer dört tipte bu seçenek görünür.
    public bool ShowPlatformFallbackOption => SelectedLaunchType != LaunchType.Exe;

    public bool IsTargetFieldEnabled => !UsePlatformFallback;

    public string PlatformFallbackCheckboxLabel =>
        $"Kodu bilmiyorum — tıklanınca {CurrentLaunchTypeLabel} açılsın, müşteri oyunu oradan başlatsın";

    private string CurrentLaunchTypeLabel => LaunchTypeOptions.First(o => o.Value == SelectedLaunchType).Label;

    public event EventHandler? Saved;

    public event EventHandler? Cancelled;

    public GameEditViewModel(Game? existingGame, Catalog catalog, LauncherSettings settings, ICatalogService catalogService, IPathResolver pathResolver)
    {
        _existingGame = existingGame;
        _catalog = catalog;
        _settings = settings;
        _catalogService = catalogService;
        _pathResolver = pathResolver;

        Categories = catalog.Categories.ToList();

        if (existingGame is { } game)
        {
            Name = game.Name;
            SelectedCategory = game.Category;
            SelectedLaunchType = game.LaunchType;
            LaunchTarget = game.LaunchTarget;
            UsePlatformFallback = game.LaunchType != LaunchType.Exe && string.IsNullOrWhiteSpace(game.LaunchTarget);
            WorkingDir = game.WorkingDir ?? "";
            Args = game.Args ?? "";
            IsVisible = game.Visible;
            SortOrderValue = game.SortOrder;
            AgeRestricted = game.AgeRestricted;
            IsAdvancedExpanded = !string.IsNullOrWhiteSpace(game.WorkingDir) || !string.IsNullOrWhiteSpace(game.Args);
        }
        else
        {
            SortOrderValue = catalog.Games.Count == 0 ? 1 : catalog.Games.Max(g => g.SortOrder) + 1;
        }
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(CanSave));

    partial void OnSelectedLaunchTypeChanged(LaunchType value)
    {
        if (value == LaunchType.Exe)
        {
            // Exe'de bu seçeneğin karşılığı yok; tip değiştirildiğinde sessizce takılı kalmasın.
            UsePlatformFallback = false;
        }

        OnPropertyChanged(nameof(TargetLabel));
        OnPropertyChanged(nameof(TargetHint));
        OnPropertyChanged(nameof(ShowBrowseButton));
        OnPropertyChanged(nameof(ShowPlatformFallbackOption));
        OnPropertyChanged(nameof(PlatformFallbackCheckboxLabel));
        TargetError = null;
    }

    partial void OnUsePlatformFallbackChanged(bool value)
    {
        OnPropertyChanged(nameof(IsTargetFieldEnabled));
        OnPropertyChanged(nameof(TargetHint));
        TargetError = null;

        if (value)
        {
            LaunchTarget = "";
        }
    }

    partial void OnNameChanged(string value)
    {
        var trimmed = value.Trim();
        DuplicateNameWarning = trimmed.Length > 0 && _catalog.Games.Any(g => g != _existingGame && string.Equals(g.Name, trimmed, StringComparison.OrdinalIgnoreCase))
            ? $"\"{trimmed}\" adında başka bir oyun zaten var. Aynı oyunun başka bir sürümüyse sorun değil."
            : null;
    }

    [RelayCommand]
    private void BrowseExe()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Çalıştırılabilir dosya seçin",
            Filter = "Çalıştırılabilir dosyalar (*.exe)|*.exe|Tüm dosyalar (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        LaunchTarget = _pathResolver.ToTemplate(dialog.FileName, _settings);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        NameError = null;
        TargetError = null;
        SaveErrorMessage = null;

        var name = Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NameError = "Ad boş olamaz.";
            return;
        }

        var target = UsePlatformFallback ? "" : LaunchTarget.Trim();

        if (!UsePlatformFallback)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                TargetError = $"{TargetLabel} boş olamaz.";
                return;
            }

            if (SelectedLaunchType == LaunchType.Steam && !int.TryParse(target, out _))
            {
                TargetError = "Steam AppID sayısal olmalı (ör. 730).";
                return;
            }
        }

        var category = SelectedCategory.Trim();
        var workingDir = string.IsNullOrWhiteSpace(WorkingDir) ? null : WorkingDir.Trim();
        var args = string.IsNullOrWhiteSpace(Args) ? null : Args.Trim();
        var sortOrder = (int)(SortOrderValue ?? 0);

        IsBusy = true;
        try
        {
            string savedId;
            if (_existingGame is { } game)
            {
                game.Name = name;
                game.Category = category;
                game.LaunchType = SelectedLaunchType;
                game.LaunchTarget = target;
                game.WorkingDir = workingDir;
                game.Args = args;
                game.Visible = IsVisible;
                game.SortOrder = sortOrder;
                game.AgeRestricted = AgeRestricted;
                savedId = game.Id;
            }
            else
            {
                savedId = GameIdGenerator.GenerateUniqueId(name, _catalog.Games.Select(g => g.Id));
                _catalog.Games.Add(new Game
                {
                    Id = savedId,
                    Name = name,
                    Category = category,
                    LaunchType = SelectedLaunchType,
                    LaunchTarget = target,
                    WorkingDir = workingDir,
                    Args = args,
                    Visible = IsVisible,
                    SortOrder = sortOrder,
                    AgeRestricted = AgeRestricted
                });
            }

            if (category.Length > 0 && !_catalog.Categories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                _catalog.Categories.Add(category);
            }

            _catalog.UpdatedAt = DateTime.Now;
            await _catalogService.SaveAsync(_catalog, _settings.CatalogPath);

            Log.Information("Admin: oyun kaydedildi: {Id} {Name}", savedId, name);
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            SaveErrorMessage = "Kaydedilemedi. Diskin salt-okur olmadığından emin olun.";
            Log.Error(ex, "Admin: oyun kaydedilemedi: {Name}", name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
