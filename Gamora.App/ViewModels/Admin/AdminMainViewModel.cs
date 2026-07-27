using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gamora.App.ViewModels.Admin;

public partial class AdminMainViewModel : ObservableObject
{
    public IReadOnlyList<string> Sections { get; } = ["Oyunlar", "İstatistikler", "Ayarlar"];

    [ObservableProperty]
    private string _selectedSection = "Oyunlar";

    [RelayCommand]
    private void SelectSection(string section)
    {
        SelectedSection = section;
    }
}
