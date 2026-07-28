# Gamora Kurulum Kılavuzu

Bu kılavuz, Gamora oyun arşivini bir internet kafeye kurmak içindir.
Teknik bilgi gerektirmez; CCBoot'lu disksiz kafe kurulumunu bilen herkes
uygulayabilir. Adımları sırayla takip edin, atlamayın.

> NOT: Bu belge geliştirme sürecinde hazırlanmıştır. Pilot kurulumdan önce
> son hali verilecektir. [PİLOT ÖNCESİ NETLEŞECEK] işaretli yerler,
> yazılım tamamlandığında kesinleşecek detaylardır.

---

## Gamora Nedir? (1 dakikada)

Gamora, ÜÇGEN benzeri bir oyun arşividir: müşteri makinelerinde tam ekran
açılır, kafedeki oyunları kapaklarıyla listeler, tıklanınca oyunu başlatır.
Oyunları sizin yerinize kurmaz veya güncellemez — oyunlar bugüne kadar
olduğu gibi sunucuda Steam / Riot / Battle.net / Epic üzerinden
güncellenmeye devam eder. Gamora sadece vitrindir.

İki parçası vardır ama ikisi de AYNI dosyadır (Gamora.exe):
- Normal açılınca → müşteri ekranı (oyun listesi)
- "--admin" ile açılınca → yönetim ekranı (oyun ekleme/çıkarma, şifreli)

---

## Kuruluma Başlamadan — Elinizde Olması Gerekenler

1. Gamora.exe (size verilecek tek dosya)
2. Sunucuya erişim (oyun diskinin bağlı olduğu kasa)
3. İmaj güncelleme (süper) modunu açma yetkisi — sadece 1 kez gerekecek
4. Oyun diskinin harfi (örnek bu belgede G: kabul edilmiştir;
   sizde farklıysa her G: gördüğünüz yere kendi harfinizi koyun)

Toplam süre: yaklaşık 30 dakika (oyun ekleme hariç).

---

## BÖLÜM 1 — Sunucu Kurulumu (süper GEREKMEZ, ~10 dk)

Bu bölümdeki her şey sunucu kasada, normal Windows'ta yapılır.

### 1.1 Veri klasörünü oluşturun

Oyun diskinde şu klasörü açın:

    G:\Gamora

İçine üç boş alt klasör açın:

    G:\Gamora\covers      (oyun kapak resimleri buraya)
    G:\Gamora\stats       (istatistikler buraya - kendiliğinden dolar)
    G:\Gamora\videos      (şimdilik boş kalacak)

catalog.json dosyasını (oyun listesi) elle oluşturmanıza gerek yok —
yönetim ekranı ilk oyunu eklediğinizde kendisi oluşturur.

### 1.2 Yönetim kısayolunu oluşturun

1. Gamora.exe'yi sunucuda kalıcı bir yere koyun, örnek: G:\Gamora\Gamora.exe
2. Masaüstüne kısayol oluşturun: exe'ye sağ tık → Gönder → Masaüstü (kısayol)
3. Kısayola sağ tık → Özellikler → "Hedef" kutusundaki yolun SONUNA
   bir boşluk bırakıp şunu ekleyin:  --admin
   Örnek hedef:  G:\Gamora\Gamora.exe --admin
4. Kısayolun adını "Gamora Yönetim" yapın.

Bu kısayol, oyun ekleyip çıkaracağınız yönetim ekranını açar.

### 1.3 Yönetici şifresini belirleyin

"Gamora Yönetim" kısayolunu ilk kez açtığınızda sizden bir yönetici
şifresi belirlemenizi ister. ÖNEMLİ:

- Sistem en az 6 karakteri kabul eder ama bununla yetinmeyin: tahmin
  edilebilir kısa şifreler koymayın ("kafe123" gibi — müşteriler dener).
  En az 10 karaktere, rastgele bir kelime/sayı karışımına çıkmanızı öneririz.
- Şifreyi güvenli bir yere not edin. Unutulursa sıfırlama işlemi
  yalnızca sunucudan yapılabilir. [PİLOT ÖNCESİ NETLEŞECEK:
  şifre sıfırlama adımları buraya yazılacak]

### 1.4 Paylaşım izinlerini ayarlayın (GÜVENLİK — atlamayın!)

Müşteri makinelerinin G:\Gamora'ya erişimi şöyle olmalı:

| Klasör / dosya        | Müşteri erişimi        |
|-----------------------|------------------------|
| G:\Gamora\catalog.json| SADECE OKUMA           |
| G:\Gamora\covers      | SADECE OKUMA           |
| G:\Gamora\videos      | SADECE OKUMA           |
| G:\Gamora\stats       | OKUMA + YAZMA          |
| G:\Gamora\sfr.lock    | ERİŞİM YOK (veya sadece okuma) |

Neden önemli: Bu izinler doğru ayarlanmazsa bir müşteri oyun listesini
bozabilir veya yönetici şifresini sıfırlamaya çalışabilir. İzinler doğru
olduğu sürece bunlar MÜMKÜN DEĞİLDİR.

sfr.lock dosyası: yönetici şifresinin şifrelenmiş halini tutan sistem
dosyasıdır. Silmeyin, taşımayın, adını değiştirmeyin.

[PİLOT ÖNCESİ NETLEŞECEK: kullandığınız paylaşım yöntemine (SMB/CCBoot)
göre iznin tam olarak nereden ayarlanacağı ekran görüntüleriyle eklenecek]

---

## BÖLÜM 2 — İmaj Kurulumu (süper GEREKİR, ~10 dk, TEK SEFERLİK)

Bu bölüm müşteri makinelerinin boot ettiği Windows imajına yapılır.
Steam veya sürücü güncellerken izlediğiniz imaj güncelleme adımlarının
aynısı: süper modu aç → değişikliği yap → kaydet.

### 2.1 Süper/imaj güncelleme modunu açın

Her zaman yaptığınız gibi.

### 2.2 Gamora'yı imaja kopyalayın

1. İmajda C:\Gamora klasörü oluşturun
2. Gamora.exe'yi içine kopyalayın
3. Aynı klasöre settings.json adında bir metin dosyası oluşturun
   (Not Defteri ile) ve içine TAM OLARAK şunu yazın:

    {
      "dataRoot": "G:\\Gamora",
      "gameDisk": "G:",
      "testMode": false
    }

   DİKKAT: Ters eğik çizgiler ÇİFT yazılır (G:\\Gamora). Oyun diskiniz
   G: değilse her iki satırda da kendi harfinizi kullanın.
   "testMode" mutlaka false olmalı — true kalırsa oyunlar yerine
   Not Defteri açılır (bu bir arıza değil, test ayarıdır).

### 2.3 Açılışta otomatik başlatmayı ayarlayın

Gamora'nın Windows açılınca kendiliğinden gelmesi için:

1. Windows + R tuşlarına basın, shell:startup yazın, Enter
2. Açılan Başlangıç klasörüne C:\Gamora\Gamora.exe'nin kısayolunu koyun

[PİLOT ÖNCESİ NETLEŞECEK: bu adımı sizin yerinize yapan tek tıklık
kurulum programı hazırlanacak — o gelene kadar elle yapılır]

### 2.4 Süper modu kapatın, imajı kaydedin

Her zamanki gibi. Bir müşteri makinesini yeniden başlatın —
Gamora tam ekran açılmalı. (Oyun listesi henüz boş olacaktır, normal.)

---

## BÖLÜM 3 — Oyunları Ekleme (sunucuda, süpersiz)

> NOT: Kapak resmi yükleme henüz yapım aşamasında — "Kapak Seç" adımı bir
> sonraki sürümde gelecek, o zamana kadar oyunlar kapaksız (baş harfli
> kutu) görünür. Bunun dışındaki tüm adımlar (ekleme/düzenleme/silme,
> arama, gizleme) çalışıyor.

Sunucudaki "Gamora Yönetim" kısayolunu açın, şifrenizi girin,
sol menüden "Oyunlar"ı seçin. Her oyun için:

1. "+ Yeni Oyun"a tıklayın
2. Oyunun adını yazın, kategorisini seçin ya da yeni bir kategori yazın
3. Başlatma tipini seçin:
   - Steam oyunu  → tip: Steam,  istenen bilgi: oyunun Steam AppID'si
     (AppID'yi steamdb.info sitesinde oyunun adını aratarak bulursunuz;
     örnek: CS2 = 730)
   - Valorant / LoL → tip: Riot
   - Battle.net oyunu (CoD vb.) → tip: Battle.net
   - Epic oyunu → tip: Epic
   - Bunların dışında, doğrudan klasörden çalışan oyun → tip: EXE,
     "Gözat" ile oyunun exe dosyasını G: diskinden seçin (oyun diskinin
     içinden seçtiğiniz sürece yol otomatik taşınabilir hale getirilir)
   - Steam/Riot/Battle.net/Epic'te kodu bilmiyorsanız: "Kodu bilmiyorum"
     kutusunu işaretleyin — tıklanınca oyunu değil, doğrudan o platformun
     kendisini (Steam, Riot Client, Battle.net, Epic Games) açar; müşteri
     oyunu oradan kendisi başlatır. EXE'de bu seçenek yoktur, yol zorunlu.
4. Görünürlük ve sırayı ayarlayın, "Ekle"ye basın.

Kaydettiğiniz anda TÜM müşteri makineleri yeni oyunu görür — makineleri
yeniden başlatmaya, imaja girmeye gerek YOKTUR.

---

## BÖLÜM 4 — Kurulum Sonrası Kontrol Listesi

Bir müşteri makinesinde şunları tek tek deneyin:

1. Makine açılınca Gamora kendiliğinden tam ekran geliyor mu?
2. Eklediğiniz oyunlar kapaklarıyla görünüyor mu?
3. Arama kutusuna yazınca liste anında süzülüyor mu?
4. Bir Steam oyununa tıklayın → Steam açılıp giriş istemeli
5. Valorant'a tıklayın → Riot Client açılmalı
6. Battle.net ve Epic'ten birer oyun deneyin
7. Olmayan bir exe'li oyun ekleyip tıklayın → "Oyun başlatılamadı.
   Personele haber verin." mesajı çıkmalı, program ÇÖKMEMELİ
8. Makineyi kapatıp açın → her şey aynı şekilde geri gelmeli

5 ve 6 çalışmazsa panik yok — Riot/Battle.net komutları sürüme göre
değişebiliyor; geliştiriciye (bize) haber verin, hızlı düzeltilir.
İlk kurulumda en çok beklenen pürüz noktası burasıdır.

---

## Günlük Kullanımda Neyi Nerede Yaparsınız?

| İş                              | Nerede                | Süper? |
|---------------------------------|-----------------------|--------|
| Oyun ekleme / çıkarma / gizleme | Sunucu, Gamora Yönetim| HAYIR  |
| Kapak değiştirme                | Sunucu, Gamora Yönetim| HAYIR  |
| Oyun güncelleme (Steam/Riot...) | Sunucu, her zamanki gibi| HAYIR|
| Hangi oyun çok oynanıyor bakma  | Sunucu, Gamora Yönetim| HAYIR  |
| Gamora'nın YENİ SÜRÜMÜ          | İmaj (2.2'deki exe değişir)| EVET |

Yani gündelik hiçbir iş için süper açmazsınız. Süper yalnızca Gamora'nın
kendisi güncellendiğinde gerekir (ileride bu da otomatikleşecek).

---

## Sorun Giderme

**Müşteri makinede Gamora açılmıyor** → Başlangıç kısayolu imajda mı
(2.3)? İmaj kaydedildi mi?

**"Oyun listesi yüklenemedi" görünüyor** → Müşteri makine G:\Gamora'yı
görüyor mu? settings.json'daki dataRoot doğru mu? Disk harfi doğru mu?

**Oyuna tıklayınca Not Defteri açılıyor** → settings.json'da
"testMode": false yapılmamış (2.2). Düzeltin, imajı güncelleyin.

**Steam oyunu "başlatılamadı" diyor** → İmajda Steam kurulu mu?
AppID doğru mu (steamdb.info'dan kontrol)?

**Yönetim şifresi unutuldu** → [PİLOT ÖNCESİ NETLEŞECEK]

**Oyun ekledim, müşteri makinede görünmüyor** → Yönetimde "Kaydet"e
basıldı mı? Müşteri makinede Gamora'yı kapatıp açın; hâlâ yoksa oyunda
"gizli" işareti olabilir.

Çözemediğiniz her şey için: geliştiriciye ulaşın, şu iki dosyayı
gönderirseniz sorun hızlı bulunur:
- Müşteri makinedeki C:\Gamora\logs klasöründeki son dosya
- G:\Gamora\catalog.json
