# CLAUDE.md — Gamora Projesi Kuralları

Bu proje internet kafeler için bir oyun launcher'ıdır (WPF, .NET 10).
Mimari ve bağlam için önce DEVELOPMENT.md dosyasını oku.

## Genel

- Benimle Türkçe konuş. Kod, tanımlayıcılar ve commit mesajları İngilizce;
  kullanıcıya görünen tüm arayüz metinleri Türkçe.
- Ben Java/Spring geçmişinden geliyorum, C# ve WPF'te yeniyim.
  C#'a özgü kalıpları (async/await, LINQ, source generator, XAML binding)
  kullandığında bir cümleyle Java karşılığına değinerek açıkla.
- Küçük adımlarla ilerle: her seferde tek özellik, çalışır durumda bırak.
  Büyük refactor'ları önce öner, onayımı almadan yapma.
- Var olmayan NuGet paketi veya API uydurma; emin değilsen söyle.
- docs/KURULUM.md kafe teknisyeni için kurulum kılavuzudur (hedef kitle
  developer DEĞİL, disksiz kafe sistemi bilen teknisyen). İlgili bir
  özellik değiştiğinde (ayar alanı, dosya adı, admin ekranı) bu belgedeki
  karşılığını da güncelle; [PİLOT ÖNCESİ NETLEŞECEK] işaretlerini ancak
  ilgili özellik gerçekten bitince doldur. Belgenin tonunu ve yapısını
  koru.

## Mimari Kurallar

- MVVM zorunlu: iş mantığı ViewModel ve Core servislerinde.
  Code-behind (.xaml.cs) yalnızca görsel/pencere işleri için.
- Gamora.Core hiçbir WPF/UI referansı içermez. UI'dan Core'a bağımlılık
  tek yönlüdür.
- Servisler interface üzerinden (ICatalogService vb.) kullanılır,
  constructor injection ile verilir (Spring'deki gibi).
- CommunityToolkit.Mvvm kalıpları: [ObservableProperty], [RelayCommand].
  INotifyPropertyChanged'i elle yazma.

## Veri Kuralları (kritik)

- Veritabanı YOK. Veri = paylaşımlı diskteki JSON. SQLite ekleme —
  ağ paylaşımında kilitleme sorunları nedeniyle bilinçli olarak dışlandı.
- catalog.json yazımı DAİMA atomik: geçici dosyaya yaz + File.Replace.
  Doğrudan üzerine yazma.
- Launcher (müşteri modu) catalog.json'a asla yazmaz; salt okur.
- İstatistikler yalnızca stats/{MachineName}.jsonl dosyasına append edilir.
  Ortak bir istatistik dosyasına birden çok makine yazamaz.
- Dosya yolları catalog'da {GAMEDISK} değişkeniyle durur; PathResolver
  ile çözülür. Mutlak yol hardcode etme.
- Müşteri makinesinde C: diskine kalıcı veri yazma — writeback ile silinir.
  Kalıcı her şey paylaşımlı diske gider.

## WPF / Arayüz Kuralları

- WPF-UI (lepo.co) bileşenlerini ve temasını kullan; klasik gri
  WinForms görünümlü kontroller üretme.
- Koyu tema varsayılan. Tasarım hedefi: Steam Big Picture / konsol
  arayüzü hissi — büyük kapaklar, bol boşluk, az metin.
- Liste/grid'lerde UI virtualization açık olmalı (500+ oyun hedefi).
- Kapak görselleri asenkron ve tembel yüklenir; UI thread'i bloklama.
- Animasyonlar hafif olmalı; her karta gölge/efekt yığma.

## Oyun Başlatma Kuralları

- Her başlatma tipi (exe, steam, riot, battlenet, epic) ayrı strateji
  sınıfı olarak yazılır; ortak IGameLauncher arkasında toplanır.
- Process başlatmalarında hata yakala: exe yoksa, URI işleyicisi yoksa
  kullanıcıya Türkçe, sade bir mesaj göster; uygulama çökmesin.
- Oyun açıkken launcher kaynak tüketmemeli (izleme hafif polling ya da
  process exit event ile).

## Yapmaman Gerekenler

- Docker, mikroservis, message queue, Entity Framework önerme —
  Faz 1 kapsamı dışı ve bilinçli olarak dışlandı.
- Faz 2 özelliklerini (merkezi sunucu, otomatik güncelleme, SteamGridDB)
  şimdiden koda ekleme; sadece Core'u buna engel olmayacak şekilde tasarla.
- Telemetri, internet çağrısı, harici API ekleme — Faz 1 tamamen çevrimdışı
  çalışır.
- README/doküman dosyalarını ben istemeden yeniden yazma.

## Test ve Doğrulama

- Geliştirme verisi: C:\GamoraData\ (catalog.json + covers).
  Gerçek ortamda yol settings.json'dan gelir.
- Oyun başlatmayı geliştirme sırasında notepad.exe gibi zararsız
  süreçlerle test et.
- Core'daki servisler (CatalogService, PathResolver, StatsService) için
  xUnit birim testleri yaz; UI için test zorunlu değil.
- Her önemli adımdan sonra `dotnet build` ve testleri çalıştır.
