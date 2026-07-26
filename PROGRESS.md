# Gamora — İlerleme Günlüğü

Kısa, tarihli günlük. Her önemli adımdan sonra güncellenir.

## 2026-07-24/25 — İskelet

- Çözüm oluşturuldu: Gamora.Core (classlib), Gamora.App (WPF), Gamora.Core.Tests (xUnit).
- Core: `Game`, `Catalog`, `LaunchType`, `StatEvent` modelleri; `ICatalogService` / `CatalogService`
  (atomik yazma: `.tmp` + `File.Replace`). 4 unit test.
- DEVELOPMENT.md'ye uçtan uca akış (müşteri + admin) eklendi.

## 2026-07-26 — Müşteri modu ana ekranı (kiosk kısıtlaması olmadan)

- **Paketler:** `Microsoft.Extensions.DependencyInjection` (10.0.10), `VirtualizingWrapPanel` (2.5.3)
  Gamora.App'e eklendi.
- **DI + Serilog:** `App.xaml.cs` içinde `ServiceCollection` kuruldu
  (`ICatalogService` → `CatalogService`, `MainViewModel`, `MainWindow` singleton).
  Serilog `logs/gamora-.log` dosyasına günlük rolling ile yazıyor. `StartupUri` kaldırıldı,
  başlatma `OnStartup`'ta manuel yapılıyor.
- **MVVM:** `MainViewModel` (`ICatalogService` inject, `InitializeAsync` ile catalog.json okuma,
  saat için `DispatcherTimer`, `FeaturedGame` = ilk görünür oyun), `GameViewModel` (kapak
  asenkron yükleme, baş harf, `SelectCommand` → Serilog log).
- **Oyun grid'i:** `ListView` + `VirtualizingWrapPanel` (UI virtualization açık, `SpacingMode=None`
  ile sabit kart aralığı). `GameCardView`: dikey kapak (2:3 oranı, 170x255), hover'da 1.05 ölçek +
  2px vurgu kenarlığı + isim şeridinin yukarı kayması, açılışta `AlternationIndex`'e göre
  25ms arayla (max 16 adım) fade+kayma animasyonu. Kapak yükleme kartın `Loaded` event'ine bağlı.
- **Test verisi:** `C:\GamoraData\catalog.json` 200 sahte (kapaksız) oyunla dolduruldu;
  eski 5 oyunluk örnek `catalog.sample-launchtypes.json` olarak yedeklendi.

## 2026-07-26 — Görsel kimlik: "premium gamer" paleti

- **Renk paleti (tek vurgu rengi):** Arka plan `#0E1015`, kartlar `#161A22`, hero `#12151C`,
  vurgu (elektrik moru) `#7C5CFF` — hepsi `App.xaml`'de merkezi kaynak. İkinci vurgu rengi yok.
- **Font:** Rajdhani Bold (Google Fonts, OFL), `Assets/Fonts/Rajdhani-Bold.ttf` olarak gömülü
  (`Gamora.App.csproj`'da `<Resource>`), pack URI ile `RajdhaniFont` kaynağı — logo, hero başlığı
  ve kart isimlerinde kullanılıyor. Klasik WPF `TextBlock` letter-spacing desteklemediği için
  logo harf harf ayrı `TextBlock`'larla diziliyor.
- **Logo:** "GAMORA" (son harf vurgu renginde) + yanında eğik vurgu-renkli çubuk işaret.
  Üst barın altında ortası parlak, kenarlara doğru şeffaflaşan 1px vurgu ayırıcı.
- **Placeholder disiplini:** Kapaksız kartlarda rastgele renk YOK — hepsi aynı: kart zemini
  (`#161A22`), soluk baş harfler (`#3A3F4E`), sadece ince üst kenarda vurgu moru çizgi.
- **Hero (vitrin):** İlk görünür oyunu büyük yatay şeritte gösteriyor. Kapak yoksa zemin düz
  `#12151C` + sağda %7 opaklıkta dev baş harf watermark; kapak varsa görsel sağda (480px),
  soldan sağa koyu→şeffaf gradyan overlay ile metin okunabilir kalıyor. Yükseklik 170px
  (~%18, hedefin altında) — grid daha erken başlıyor.
- **Ferahlık:** Kart genişliği 200→170 (%15 küçültme), kartlar arası boşluk 28px, grid'in
  sol/sağ kenar boşluğu (10+14=24) hero'nun 24px kenar boşluğuyla hizalı.
- **Doğrulama:** `dotnet build` ve `dotnet test` (Core, 4/4 yeşil) temiz her adımda. Font
  embed'i derlenmiş DLL'in `.g.resources`'ı incelenerek doğrulandı (gerçek "Rajdhani" aile adı
  onaylandı — `System.Drawing.Text.PrivateFontCollection` ile). Uygulama arkaplanda kısaca
  başlatılıp çökmeden katalog yüklediği doğrulandı. **Kullanıcı görsel olarak inceledi ve
  onayladı** (2026-07-26).

## 2026-07-26 — Anlık arama + kategori filtresi

- **Filtreleme mimarisi:** `MainViewModel.GamesView` (`ICollectionView`, `CollectionViewSource.
  GetDefaultView(Games)`) — ikinci bir filtrelenmiş `ObservableCollection` değil. Sebep: `Games`
  tek gerçek kaynak kalıyor; `Refresh()` sadece eşleşme durumu değişen kartların container'ını
  oluşturup söküyor (hâlâ eşleşenler yeniden animasyonlanmıyor, sadece yeni eşleşenler fade-in
  ile beliriyor); `VirtualizingWrapPanel` filtrelenmiş view üzerinden çalışıyor, virtualization
  bozulmuyor.
- **Türkçe-toleranslı arama:** `Gamora.Core/Services/TurkishSearch.cs` — kültüre bağımlı
  `ToLower()`'ın Türkçe kültüründe "FIFA"yı "fıfa" yapması gibi tuzaklara düşmeden, karakter
  karakter eşleme (I/İ/ı/i→i, ş→s, ğ→g, ü→u, ö→o, ç→c). 6 xUnit testiyle doğrulandı.
- **Arama kutusu:** `ui:TextBox`, her tuşta `SearchText` güncelleniyor (`UpdateSourceTrigger=
  PropertyChanged`), 180ms debounce'lu `DispatcherTimer` gerçek filtrelemeyi tetikliyor.
- **Kategori sekmeleri:** `ListBox` + `SelectedItem` iki yönlü bağlama, pill görünümü
  `ControlTemplate.Triggers` ile (seçiliyken vurgu moru dolgu). Kategori + arama kesişim olarak
  birlikte çalışıyor (`MatchesFilter`).
- **Boş sonuç:** `GamesView.IsEmpty`'e bağlı "Oyun bulunamadı" + anında temizleyen
  `ClearSearchCommand` butonu.
- **Klavye:** Griddeyken yazmaya başlayınca odak otomatik arama kutusuna geçiyor
  (`MainWindow.xaml.cs`, `PreviewTextInput`); ESC, `Window.InputBindings` üzerinden
  `ClearSearchCommand`'a bağlı (code-behind gerekmedi).
- **Doğrulama:** `dotnet build` temiz, `dotnet test` 10/10 yeşil (4 eski + 6 yeni). Uygulama
  arkaplanda kısaca çalıştırılıp çökme olmadığı doğrulandı. **Kullanıcı görsel/etkileşim olarak
  inceledi ve onayladı** (2026-07-26).

### Sıradaki adımlar (henüz yapılmadı)
- `PathResolver` (`{GAMEDISK}` çözümü), `IGameLauncher` (exe/steam/riot/battlenet/epic başlatma).
- `IStatsService` (stats/{MachineName}.jsonl).
- Admin modu (`--admin`), oyun CRUD, kapak yükleme.
