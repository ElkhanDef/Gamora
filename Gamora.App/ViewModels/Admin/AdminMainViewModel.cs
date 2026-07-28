using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gamora.App.ViewModels.Admin;

public partial class AdminMainViewModel : ObservableObject
{
    private readonly GameListViewModel _gameListViewModel;

    public IReadOnlyList<string> Sections { get; } = ["Oyunlar", "İstatistikler", "Ayarlar"];

    [ObservableProperty]
    private string _selectedSection = "Oyunlar";

    // null iken AdminMainWindow sadece SelectedSection başlığını gösterir (henüz yapılmamış
    // bölümler için basit yer tutucu); dolu iken ilgili içerik (liste/form) DataTemplate ile
    // gösterilir — form ayrı bir pencere değil, aynı ekranın içeriği.
    [ObservableProperty]
    private object? _currentContent;

    public AdminMainViewModel(GameListViewModel gameListViewModel)
    {
        _gameListViewModel = gameListViewModel;
        _gameListViewModel.EditRequested += OnEditRequested;

        CurrentContent = _gameListViewModel;
        _ = _gameListViewModel.ReloadAsync();
    }

    partial void OnSelectedSectionChanged(string value)
    {
        CurrentContent = value == "Oyunlar" ? _gameListViewModel : null;
    }

    [RelayCommand]
    private void SelectSection(string section) => SelectedSection = section;

    private void OnEditRequested(object? sender, Core.Models.Game? game)
    {
        var editViewModel = _gameListViewModel.CreateEditViewModel(game);
        editViewModel.Saved += OnEditFinished;
        editViewModel.Cancelled += OnEditFinished;
        CurrentContent = editViewModel;
    }

    private async void OnEditFinished(object? sender, EventArgs e)
    {
        if (sender is GameEditViewModel editViewModel)
        {
            editViewModel.Saved -= OnEditFinished;
            editViewModel.Cancelled -= OnEditFinished;
        }

        CurrentContent = _gameListViewModel;
        await _gameListViewModel.ReloadAsync();
    }
}
