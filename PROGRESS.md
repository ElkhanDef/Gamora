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

## 2026-07-26/27 — Oyun başlatma (strateji deseni + test modu)

- **Strateji deseni:** `Gamora.Core/Services/LaunchStrategyBase` (soyut) ortak akışı tek yerde
  topluyor: komut oluşturma hatası, test modu ikamesi, `Process.Start` + hata yakalama.
  5 somut sınıf (`ExeLaunchStrategy`, `SteamLaunchStrategy`, `RiotLaunchStrategy`,
  `BattleNetLaunchStrategy`, `EpicLaunchStrategy`) sadece kendi komutunu üretiyor.
  `GameLauncher`, DI'dan `IEnumerable<ILaunchStrategy>` alıp `LaunchType`'a göre dispatch ediyor.
  **Not:** battlenet/epic URI kalıpları DEVELOPMENT.md'de net değildi ("kendi URI/komut
  kalıpları") — bilinen gerçek şemalar kullanıldı (`battlenet://{kod}`,
  `com.epicgames.launcher://apps/{id}?action=launch&silent=true`) ama gerçek cihazda
  doğrulanmadı; steam/riot DEVELOPMENT.md'de net olduğu için eminiz.
- **PathResolver + settings.json:** `{GAMEDISK}` → `settings.GameDisk`. `SettingsService`
  dosya yoksa varsayılanlarla oluşturuyor.
- **Test modu:** `testMode=true` iken gerçek komut yerine `notepad.exe` açılır, gerçek komut
  Serilog'a `[TEST] {komut} çalıştırılacaktı` olarak yazılır — kafeye gitmeden doğrulanabilir.
- **Overlay + kilit:** Tam ekran yarı saydam "BAŞLATILIYOR" katmanı (pulse animasyonu + oyun
  adı); hatada aynı katman Türkçe mesaj + "Tamam" butonuna dönüşüp 4sn sonra kendiliğinden
  kapanır. `GameViewModel.SelectCommand` async olduğu için CommunityToolkit.Mvvm'in
  `[RelayCommand]`'i otomatik yeniden-giriş engeli sağlıyor; `Task.WhenAll(başlatma,
  Task.Delay(3.5sn))` ile bu süre en az 3.5 saniyeye sabitlendi, kartta da görsel karartma var.
- **Süreç izleme:** Başarılı başlatmadan sonra pencere küçülür; `LaunchResult.Process` doluysa
  (çoğunlukla exe tipinde) `WaitForExitAsync()` ile kapanması beklenip pencere geri getirilir.
  URI tabanlı başlatmalarda (steam/riot/battlenet/epic) process genelde izlenemez — Core sadece
  `Process: null` döner, müşteri kendisi geri döner (bilinçli Faz 1 sınırı).
- **Doğrulama:** `dotnet build`/`test` temiz (17/17). Ortamın kendi otomatik input-injection'ı
  arkaplan testinde birkaç kartı tıklayıp exe/steam tiplerini uçtan uca doğruladı (loglar tam
  beklenen formatta). Riot/battlenet/epic'i ve overlay'in görselini **kullanıcı bizzat
  tıklayarak doğrulamadı** — sıradaki oturumda kontrol edilmeli.

## 2026-07-27 — Veri yolu: sabit C:\GamoraData yerine settings.json → dataRoot

- **Sorun:** `MainViewModel`'de `CatalogPath`/`CoversDirectory` sabit `C:\GamoraData` idi —
  CLAUDE.md'nin "mutlak yol hardcode etme" kuralına aykırıydı.
- **Tarama sonucu:** Kod genelinde sabit veri yolu geçen TEK yer `MainViewModel.cs`'teki bu iki
  `const` idi. `CatalogService`/`GameViewModel` zaten path'i parametre olarak alıyor, kendileri
  hardcode etmiyor. `StatsService` projede henüz YOK (sadece `StatEvent` modeli var) — o yüzden
  taranacak bir yeri yoktu, ama `LauncherSettings.StatsPath` şimdiden hazır, ileride
  `StatsService` yazılınca sıfırdan doğru kaynağı kullanacak.
- **Çözüm:** `LauncherSettings`'e `DataRoot` eklendi (varsayılan `C:\GamoraData`, `GameDisk`'ten
  bağımsız — kafede ikisi aynı diskte olabilir ama ayrı ayarlanabilir kalıyor). `CatalogPath`,
  `CoversPath`, `StatsPath` bu kökten türeyen `[JsonIgnore]` hesaplanan property'ler — settings.
  json'a asla yazılmıyorlar, sadece `DataRoot` yazılıyor. `MainViewModel` artık `ISettingsService`
  inject ediyor, `InitializeAsync`'te önce ayarları yükleyip `settings.CatalogPath`/`CoversPath`
  kullanıyor.
- **Doğrulama:** `dotnet build`/`test` temiz (17/17, yeni DataRoot testleri dahil). Gerçek
  uygulamada `settings.json`'daki `dataRoot`'u geçici olarak `C:\GamoraData_Alt`'a çevirip orada
  1 oyunluk farklı bir catalog.json'dan yüklendiğini log'da doğruladım
  (`Katalog yüklendi: 1 oyun (...GamoraData_Alt\catalog.json)`), sonra `C:\GamoraData`'ya geri
  alıp 200 oyunun yine oradan geldiğini teyit ettim; geçici klasör silindi.

## 2026-07-27 — İstatistik: launch kaydı + popülerlik sıralaması

- **StatsService:** Her başarılı oyun başlatmada `{dataRoot}\stats\{MachineName}.jsonl`'e bir
  satır ekler: `{"gameId":"cs2","event":"launch","time":"2026-07-26T19:45:00"}`. Makine kimliği
  kaydın içinde yok, zaten dosya adında. `GameLauncher.LaunchAsync` bu çağrıyı **beklemeden**
  (`_ = ...`) yapıyor — başlatma akışı bir milisaniye bile gecikmiyor. `StatsService` kendi
  içinde tüm hataları yakalar, asla fırlatmaz (istatistik kritik değil, oyun başlatma kritik).
- **PopularityService:** `stats/` altındaki tüm `.jsonl` dosyalarını okuyup oyun başına toplam
  sayıyı hesaplıyor; bozuk/yarım satırları (başka makine tam o anda yazıyor olabilir) sessizce
  atlıyor. Açılışta bir kez çalışıp bellekte tutuluyor, canlı güncelleme yok.
- **Sıralama + hero:** `MainViewModel.InitializeAsync` artık `Games`'i popülerlik (azalan) →
  sortOrder → ad sırasına göre dolduruyor. `FeaturedGame` zaten `Games.FirstOrDefault()`
  olduğu için hiçbir ek değişiklik gerekmeden otomatik olarak en popüler oyunu gösteriyor.
- **Popüler rozeti:** İlk 10 (launch sayısı > 0 olan) karta sağ üst köşede, yarı saydam koyu
  20px yuvarlak zemin üstünde `ui:SymbolIcon Symbol="Fire16"` (14px, vurgu moru, sayı yok).
  İlk halde elle çizilmiş bir `Path` kullanmıştım (alev değil su damlasına benziyordu) —
  `SymbolRegular.cs` kaynağını indirip `Fire16/20/24`'ün gerçekten var olduğunu satır satır
  doğruladıktan sonra WPF-UI'ın kendi ikonuna geçtim. **Kullanıcı görsel olarak inceledi ve
  onayladı** (2026-07-27).
- **Test aracı:** `tools/reset-testdata.ps1` — `stats/`'i temizleyip PC-01..04 için dağıtılmış
  sahte olaylar üretiyor (test-game-50=6, test-game-10=4, test-game-77=3, + 9 tekli, + 1 kasıtlı
  bozuk satır), catalog.json'a dokunmuyor. İstatistiği sıfırdan test etmek için tekrar çalıştır.
- **Doğrulama:** `dotnet build`/`test` temiz (24/24, 8 yeni test). Gerçek uygulamada script'i
  çalıştırıp katalogu açtım, log tam beklendiği gibi:
  `Popülerlik hesaplandı: 12 oyun, 4 dosya` / `En popüler oyunlar: test-game-50=6, test-game-10=4,
  test-game-77=3` — bozuk satır sessizce atlandı (12 oyun sayıldı, 13. hiç görünmedi), hero
  gerçekten test-game-50'ye döndü.

### Sıradaki adımlar (henüz yapılmadı)
- Riot/battlenet/epic başlatmayı ve oyun başlatma overlay'ini kullanıcı gözüyle doğrulama.

## 2026-07-27 — Admin modu iskeleti: şifre kurulum/giriş + ana pencere

- **PasswordService (Core, PBKDF2):** `Rfc2898DeriveBytes.Pbkdf2` (modern statik API — eski
  constructor artık obsolete, `SYSLIB0060` uyarısı verdi, o yüzden statik metoda geçtim),
  16 byte rastgele salt, 100.000 iterasyon, SHA256. `AdminLock` dosya formatı salt+iterasyon+
  hash'i birlikte tutuyor ki iterasyon sayısı ileride artırılınca eski kayıtlar hâlâ okunabilsin.
  Karşılaştırma `CryptographicOperations.FixedTimeEquals` ile zamanlama saldırılarına karşı
  sabit sürede. Tüm kripto Core'da — UI'da hiç kripto kodu yok.
- **sfr.lock:** `dataRoot`'ta duruyor, adı `LauncherSettings.AdminLockFileName` sabitinde tek
  yerde tanımlı (`AdminLockPath` diğer üç yol gibi `[JsonIgnore]` türetilen property).
  settings.json'da şifreyle ilgili hiçbir alan yok. Yazma atomik (`.tmp` + `File.Replace`/`Move`,
  catalog.json'daki desenin aynısı). Reset saldırısına karşı: `sfr.lock` zaten varsa
  `SetPasswordAsync` üzerine yazmayı reddediyor.
- **İlk kurulum akışı:** `--admin` + sfr.lock yoksa `PasswordSetupWindow` (şifre + tekrar, min
  6 karakter). Yazma gerçekten başarısız olursa (salt-okur paylaşım) "Yönetici kurulumu
  yalnızca sunucuda yapılabilir." mesajı gösterilip form kilitleniyor (`IsTerminalError`) —
  akış orada bitiyor, tekrar deneme yok.
- **Giriş akışı:** sfr.lock varsa `PasswordLoginWindow` — tek şifre alanı (`ui:PasswordBox`,
  gerçek bir `DependencyProperty` olduğu için klasik `PasswordBox` gibi code-behind köprüsü
  gerekmedi), "Giriş" butonu `IsDefault="True"` (Enter otomatik çalışır). 3 yanlış denemede
  `DispatcherTimer` ile 30 saniyelik geri sayım, süre dolunca sayaç sıfırlanıp tekrar denemeye
  izin veriyor.
- **Admin ana penceresi (iskelet):** `AdminMainWindow` — sol dikey menü (Oyunlar/İstatistikler/
  Ayarlar, seçili öğede vurgu-moru sol şerit), içerik alanı şimdilik sadece başlık gösteriyor.
  Boyutlandırılabilir, aynı koyu tema/tipografi.
- **Mod ayrımı:** `App.xaml.cs` `e.Args`'ta `--admin` arıyor; yoksa akış eskisiyle birebir aynı
  (hiç dokunulmadı). Admin akışı `ShowDialog()` ile senkron bekliyor — kurulum/giriş penceresi
  `DialogResult=true` ile kapanmadan `AdminMainWindow` açılmıyor.
- **Doğrulama:** `dotnet build`/`test` temiz (34/34, 12 yeni `PasswordService` testi — doğru
  şifre geçiyor, yanlış geçmiyor, her kurulumda salt farklı, en az 100.000 iterasyon, düz metin
  dosyada yok, var olan kilit üzerine yazılmıyor, yazılamayan yol dostane hata döndürüyor).
  Gerçek uygulamada: (a) parametresiz açılış — loglar birebir eskisiyle aynı, admin'e hiç
  değinmiyor; (b) `--admin` + sfr.lock yok → log "kurulum ekranı açılıyor"; (c) sfr.lock'u
  GUI'ye dokunmadan `PasswordService.SetPasswordAsync`'i doğrudan çalıştırarak (şifre
  `test123456`) oluşturup ikinci `--admin`'de log "giriş ekranı açılıyor"nu doğruladım; (e)
  `icacls ... /deny` ile gerçekten yazılamayan bir klasöre `dataRoot`'u çevirip aynı üretim
  kod yolunu (`SetPasswordAsync`) çalıştırdım — çıktı tam beklenen: `Success: False`,
  `"Yönetici kurulumu yalnızca sunucuda yapılabilir."`; sonra ACL/klasör/settings.json geri
  alındı. **(d) yanlış şifre + 30sn kilitlenmeyi ve tüm ekranların görselini bizzat
  tıklayarak doğrulamadım** — gerçek `sfr.lock` hâlâ duruyor (şifre: `test123456`), bunu
  kullanıcının kendi gözüyle kontrol etmesi lazım.
- **KURULUM.md senkronu:** Dosya repo kökünde duruyormuş (docs/ altında değil). CLAUDE.md'nin
  "ilgili özellik değiştiğinde güncelle" kuralına göre iki uyuşmazlığı düzelttim: (1) "en az 10
  karakter" iddiası gerçek kuralla (6) çelişiyordu — 6'yı kabul edip 10+'ı öneri olarak
  belirtecek şekilde yeniden yazdım; (2) Bölüm 3 (oyun ekleme) sanki bitmiş gibi anlatılmış,
  oysa şu an sadece admin iskeleti (üç boş bölüm başlığı) var — başına "henüz yapım aşamasında"
  notu ekledim. Belgenin tonu/yapısı korunuyor, [PİLOT ÖNCESİ NETLEŞECEK] işaretlerine dokunmadım.

### Sıradaki adımlar (henüz yapılmadı)
- Admin modu içerikleri: oyun CRUD, kapak yükleme, istatistik ekranı, ayarlar ekranı
  (tamamlandıkça KURULUM.md Bölüm 3'ü güncellenmeli).
- Riot/battlenet/epic başlatmayı, oyun başlatma overlay'ini ve popüler rozetini kullanıcı
  gözüyle doğrulama.
- Giriş ekranındaki 3-deneme/30sn kilitlenmeyi ve tüm şifre ekranlarının görselini doğrulama.
