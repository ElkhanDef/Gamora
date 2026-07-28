using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.App.ViewModels.Admin;

public partial class GameRowViewModel : ObservableObject
{
    private readonly string _coverFullPath;
    private bool _coverLoadStarted;

    [ObservableProperty]
    private ImageSource? _coverImage;

    public GameRowViewModel(Game game, string coversDirectory)
    {
        Model = game;
        _coverFullPath = string.IsNullOrWhiteSpace(game.Cover)
            ? ""
            : Path.Combine(coversDirectory, Path.GetFileName(game.Cover));
    }

    public Game Model { get; }

    public string Name => Model.Name;

    public string Category => Model.Category;

    public bool Visible => Model.Visible;

    public int SortOrder => Model.SortOrder;

    // Aynı Türkçe etiketleri ekleme/düzenleme formuyla da paylaşmak için tek kaynak
    // GameEditViewModel.LaunchTypeOptions.
    public string LaunchTypeLabel => GameEditViewModel.LaunchTypeOptions
        .First(o => o.Value == Model.LaunchType).Label;

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

    // Kapak dosyası yoksa (veya satır hiç ekrana gelmediyse) placeholder kalıcı olarak
    // görünür; hata fırlatılmaz. GameViewModel'deki aynı desenin admin listesi için küçük
    // bir kopyası — burada müşteri tarafının kart animasyonu/gecikme mantığı yok.
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
            Log.Warning(ex, "Admin: kapak yüklenemedi: {Path}", _coverFullPath);
        }
    }
}
