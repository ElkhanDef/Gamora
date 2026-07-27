namespace Gamora.Core.Abstractions;

public interface IPopularityService
{
    // Stats klasöründeki tüm .jsonl dosyalarını okuyup oyun başına toplam başlatma sayısını
    // hesaplar ve bellekte tutar. Açılışta bir kez çağrılır; canlı güncelleme yapılmaz.
    Task LoadAsync(string statsDirectory, CancellationToken cancellationToken = default);

    int GetLaunchCount(string gameId);
}
