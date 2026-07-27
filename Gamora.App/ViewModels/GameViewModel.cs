using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.App.ViewModels;

public partial class GameViewModel : ObservableObject
{
    // Başlatma "başarıyla" bitse bile (URI tabanlı, çok hızlı) kart en az bu kadar kilitli
    // kalır — arka arkaya çift tıklamayla aynı oyunun iki kez açılmasını engeller.
    private static readonly TimeSpan MinimumLockDuration = TimeSpan.FromSeconds(3.5);

    private readonly Game _game;
    private readonly string _coverFullPath;
    private readonly Func<GameViewModel, Task> _requestLaunch;
    private bool _coverLoadStarted;

    [ObservableProperty]
    private ImageSource? _coverImage;

    [ObservableProperty]
    private bool _isLaunching;

    [ObservableProperty]
    private bool _isPopular;

    public GameViewModel(Game game, string coversDirectory, Func<GameViewModel, Task> requestLaunch, int launchCount)
    {
        _game = game;
        _requestLaunch = requestLaunch;
        LaunchCount = launchCount;
        _coverFullPath = string.IsNullOrWhiteSpace(game.Cover)
            ? ""
            : Path.Combine(coversDirectory, Path.GetFileName(game.Cover));
    }

    public Game Model => _game;

    // Açılışta bir kez hesaplanan toplam başlatma sayısı (bkz. IPopularityService); canlı
    // güncellenmez, sıralama ve "popüler" rozeti için kullanılır.
    public int LaunchCount { get; }

    public string Name => _game.Name;

    public string Category => _game.Category;

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
                _ => string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant()
            };
        }
    }

    // Kapak dosyası yoksa (veya kart hiç ekrana gelmediyse) placeholder kalıcı olarak görünür; hata fırlatılmaz.
    public async Task EnsureCoverLoadedAsync()
    {
        if (_coverLoadStarted || string.IsNullOrEmpty(_coverFullPath) || !File.Exists(_coverFullPath))
        {
            return;
        }

        _coverLoadStarted = true;

        try
        {
            var bytes = await File.ReadAllBytesAsync(_coverFullPath);
            var image = new BitmapImage();
            using (var stream = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            CoverImage = image;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Kapak yüklenemedi: {Path}", _coverFullPath);
        }
    }

    [RelayCommand]
    private async Task SelectAsync()
    {
        IsLaunching = true;
        try
        {
            var minimumLock = Task.Delay(MinimumLockDuration);
            var launch = _requestLaunch(this);
            await Task.WhenAll(launch, minimumLock);
        }
        finally
        {
            IsLaunching = false;
        }
    }
}
