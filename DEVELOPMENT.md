# Gamora — Geliştirme Dokümanı

İnternet kafeler için modern oyun arşivi / launcher sistemi.
Hedef: ÜÇGEN Oyun Arşivi'nin özelliklerini modern bir arayüzle karşılamak,
sonrasında merkezi yönetimle onu geçmek.

## Ortam Bilgisi

- Kafeler disksiz (diskless) sistem kullanıyor (CCBoot). Müşteri makineleri
  PXE ile sunucudan boot eder; C: writeback'lidir, oturum kapanınca sıfırlanır.
- Oyunlar sunucudaki paylaşımlı oyun diskinde kuruludur (ör. `G:\`).
  Bu disk kalıcıdır — writeback'ten etkilenmez.
- Oyun güncellemeleri sunucuda resmi launcher'lar (Steam, Riot, Battle.net,
  Epic) üzerinden manuel yapılır. Biz içerik dağıtmıyoruz.
- Launcher exe'si Windows imajına kurulur; imaj güncellenince tüm
  makinelere yayılır.

## Teknoloji Yığını (Faz 1)

| Katman | Teknoloji | Not |
|---|---|---|
| Dil | C# / .NET 10 (LTS) | `net10.0-windows` hedefi |
| Arayüz | WPF | Tek masaüstü uygulaması |
| UI kütüphanesi | WPF-UI (lepo.co) | Fluent / Windows 11 görünümü |
| Mimari | MVVM — CommunityToolkit.Mvvm | Source generator tabanlı |
| Veri | JSON (System.Text.Json) | Veritabanı sunucusu YOK |
| Loglama | Serilog | Dosyaya; ileride merkeze |
| Paketleme | dotnet publish, self-contained, tek exe | .NET kurulumu gerektirmez |
| IDE | JetBrains Rider | |
| VCS | Git + GitHub (private) | |

Faz 1'de Docker YOK (masaüstü uygulaması). Docker, Faz 2'deki Spring Boot
merkezi sunucuda kullanılacak.

## Çözüm Yapısı

```
Gamora.sln
├── Gamora.Core   (classlib, net10.0)
│   ├── Models/        Game, Catalog, GameCategory, LaunchType, StatEvent
│   ├── Services/      CatalogService (oku/atomik yaz), GameLauncher,
│   │                  StatsService, PathResolver ({GAMEDISK} çözümü)
│   └── Abstractions/  ICatalogService, IGameLauncher, IStatsService
└── Gamora.App    (WPF, net10.0-windows)
    ├── Views/         Müşteri: MainWindow, GameGridView, SearchBar
    │                  Admin: AdminWindow, GameEditView, StatsView
    ├── ViewModels/
    ├── Converters/
    └── Assets/
```

Tek uygulama, iki mod:
- Normal açılış → müşteri launcher'ı (tam ekran)
- `Gamora.exe --admin` → şifreyle yönetici modu (sunucuda kullanılır)

## Veri Yerleşimi (paylaşımlı oyun diskinde)

```
G:\Gamora\
├── catalog.json      Oyun kataloğu — tek gerçek kaynak. Tek yazar: admin.
├── covers\           Kapak görselleri (600x900 dikey, jpg/png)
├── videos\           (Faz 1.5) Tanıtım videoları
└── stats\            Makine başına ayrı olay dosyası (çakışma önlenir)
    ├── PC-01.jsonl   Her satır bir olay (JSON Lines)
    └── PC-02.jsonl
```

Kurallar:
- catalog.json'a YALNIZCA admin modülü yazar. Yazma atomik olmalı:
  önce `catalog.json.tmp`'ye yaz, sonra File.Replace/Move ile değiştir.
- Her müşteri makinesi stats altında SADECE kendi
  `{MachineName}.jsonl` dosyasına append yapar.
- Yollar catalog'da `{GAMEDISK}` değişkeniyle tutulur; makinedeki gerçek
  harf, exe yanındaki `settings.json`'dan okunur.
- SQLite kullanılmıyor: ağ paylaşımı üzerinde dosya kilitleme güvenilmez.

## catalog.json Şeması (özet)

```json
{
  "version": 1,
  "updatedAt": "2026-07-24T12:00:00",
  "categories": ["FPS", "MOBA", "Battle Royale", "Spor", "Yarış"],
  "games": [
    {
      "id": "cs2",
      "name": "Counter-Strike 2",
      "category": "FPS",
      "cover": "covers/cs2.jpg",
      "launchType": "steam",
      "launchTarget": "730",
      "workingDir": null,
      "args": null,
      "visible": true,
      "sortOrder": 1,
      "ageRestricted": false
    }
  ]
}
```

`launchType` değerleri: `exe | steam | riot | battlenet | epic`
- exe: `launchTarget` = `{GAMEDISK}\Games\Oyun\oyun.exe`
- steam: `launchTarget` = Steam AppID → `steam://rungameid/{id}`
- riot: `launchTarget` = ürün kodu (ör. `valorant`) →
  RiotClientServices `--launch-product=... --launch-patchline=live`
- battlenet / epic: kendi URI/komut kalıpları

## Fazlar

**Faz 1 — Çekirdek (ÜÇGEN'in temeli):**
tam ekran modern arayüz, oyun grid'i + kategoriler, anlık arama,
tüm başlatma tipleri, admin modu (oyun CRUD + kapak), tıklama
istatistiği + popülerleri üste taşıma, 500 oyunda akıcı liste.

**Faz 1.5 — ÜÇGEN paritesi:**
oyun tanıtım videoları, müşteri hata bildirimi (admin panelinde listelenir),
18 yaş kilidi (şifre), save yedekleme (sunucu diskine).

**Faz 2 — Fark yaratma (çok sonra):**
Spring Boot 3 + PostgreSQL merkezi panel (100+ kafe tek ekrandan),
otomatik güncelleme (Velopack), SteamGridDB'den otomatik kapak,
Docker bu fazda gelir.

## Performans Hedefleri

- Açılış → grid görünür: < 2 sn (kapaklar tembel/aşamalı yüklenir)
- 500 oyunlu listede arama/kaydırma takılmaz (UI virtualization açık)
- Boşta RAM: < 150 MB (oyun çalışırken launcher görünmez ve sessizdir)
- Disksiz sistem notu: tüm makineler sabah aynı anda açılır; kapaklar
  küçük tutulur, önbelleklenir, aynı anda yüzlerce dosya çekilmez.

## Dağıtım (kafe başına bir kez)

1. Oyun diskinde `G:\Gamora\` oluştur, örnek catalog + covers kopyala.
2. Sunucuda `Gamora.exe --admin` ile katalog doldurulur.
3. İmaj güncelleme modunda exe imaja kopyalanır, otomatik başlatma
   (Run kaydı) eklenir.
4. Müşteri makineleri yeniden başlatılır → launcher gelir.
