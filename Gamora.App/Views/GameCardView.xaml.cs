using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Gamora.App.ViewModels;

namespace Gamora.App.Views;

/// <summary>
/// Interaction logic for GameCardView.xaml
/// </summary>
public partial class GameCardView : UserControl
{
    // Sütun sayısından bağımsız, kaydırma sırasında yeniden kullanılan kartların
    // gecikmesi büyümesin diye bir üst sınırla döngüye sokuyoruz.
    private const int MaxStaggerSteps = 16;
    private const int StaggerStepMs = 25;

    public GameCardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        PlayEntranceAnimation();

        if (DataContext is GameViewModel viewModel)
        {
            await viewModel.EnsureCoverLoadedAsync();
        }
    }

    private void PlayEntranceAnimation()
    {
        var index = GetAlternationIndex();
        var delay = TimeSpan.FromMilliseconds((index % MaxStaggerSteps) * StaggerStepMs);
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
        {
            BeginTime = delay,
            EasingFunction = easing
        };
        var slide = new DoubleAnimation(16, 0, TimeSpan.FromMilliseconds(200))
        {
            BeginTime = delay,
            EasingFunction = easing
        };

        CardRoot.BeginAnimation(OpacityProperty, fade);
        EntranceTranslate.BeginAnimation(TranslateTransform.YProperty, slide);
    }

    private int GetAlternationIndex()
    {
        var current = VisualTreeHelper.GetParent(this);
        while (current is not null and not ListViewItem)
        {
            current = VisualTreeHelper.GetParent(current);
        }

        return current is null ? 0 : ItemsControl.GetAlternationIndex(current);
    }
}
