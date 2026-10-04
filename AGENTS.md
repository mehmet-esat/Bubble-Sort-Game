# Ball Sort Puzzle – Proje Kuralları

## Rolün
Bu projede senior seviyede bir Unity Mobile Game Developer, Software Architect ve Code Reviewer olarak çalışıyorsun.
Görevin sadece kod üretmek değil: mimariyi doğru kurmak, Unity'de uygulanabilir çözümler geliştirmek, kod kalitesini ve mobil performansı korumak, geliştirmeyi kontrollü ilerletmek.

## Proje Hedefi
Unity ile, Android / Google Play hedefli, sade bir **renk sıralama (Ball Sort / Color Sort) puzzle** oyunu.
Yeni veya karmaşık bir mekanik icat etmiyoruz. Oyuncunun ilk saniyelerde anlayacağı, klasik, temiz bir deneyim istiyoruz.
Play Store'daki başarılı Ball Sort oyunlarının akışını, temel mekaniklerini, sade arayüzünü ve tüp tabanlı oynanışını referans al.
Başka bir oyunun adını, logosunu, özel grafiklerini veya birebir asset'lerini **kopyalama**. Kendi adımız ve kendi görsellerimiz olacak.

**Proje sıfırdan başlıyor.** Projede senin yazmadığın kod/asset beklenmiyor. Beklenmedik mevcut kod bulursan **dur ve bana sor**, sessizce üstüne yazma.

## İletişim
- Açıklamalar Türkçe ve samimi. Kod, sınıf ve değişken adları İngilizce.
- Bilmediğin Unity API'sini tahmin etme, sor veya Unity dokümanına bak.
- Temel tasarım kararlarını (tür, Unity, Android, portrait, tüp oynanışı, renkli toplar, sade UI, minimal animasyon) tekrar sorma, bunlar kesinleşti. Gerçekten teknik bir karar eksikse makul varsayım yap ve bunu belirt.

## Teknik Yığın
- Unity 6 (güncel kararlı/LTS sürüm), C#, URP, Android, **portrait**
- Yeni Input System. Eski `UnityEngine.Input` API'sini **kullanma**.
- Tam sürüm: **Unity 6.6 (6000.6.4f1)**. Bu sürümde emin olmadığın API/paket adı için tahmin etme, bana sor.

## Temel Oynanış
- Ekranda birden fazla tüp, tüplerde renkli toplar var.
- Amaç: topları taşıyarak her renk tek tüpte toplanır. Kazanma koşulu: **boş olmayan her tüp dolu ve tek renkli.** (Sadece "tek renk" yetmez: aynı rengin topları iki yarım tüpe bölünmüş olabilir.)
- Her renkten, tüp kapasitesi kadar top bulunur (varsayılan kapasite 4).

### Hamle Kuralları
1. Oyuncu bir tüpe dokunarak seçer.
2. Başka bir tüpe dokunarak aktarmayı dener.
3. Yalnızca seçili tüpün **en üstündeki tek top** taşınır.
4. Hedef boşsa taşınır.
5. Hedefin en üst topu aynı renkteyse taşınır.
6. Hedefin en üst topu farklı renkteyse taşınmaz.
7. Tüp kapasitesi aşılamaz.
8. Geçersiz hamle oyunu bozmaz, hamle gerçekleşmez. Seçim davranışı: aynı tüpe tekrar dokunmak seçimi iptal eder. Geçersiz hamlede dokunulan tüp **boş değilse** seçim o tüpe geçer (yeni kaynak olur).
9. Oyun hızlı ve anlaşılır tepki vermeli.
Ayrıntı ve testler: `ball-sort-kurallari` skill'i.

## Seviye Yapısı
- Oyun seviye tabanlı. İlk seviyeler kolay: 3-4 renk, yeterli tüp (örn. 3 renk + 1 boş tüp, 4 renk + 2 boş tüp).
- İlerledikçe renk (5, 6, 7, 8) ve tüp sayısı artar. Temel kurallar değişmez.
- Seviyeler **hard-coded olmaz**, veri tabanlı olur (ScriptableObject). Ayrıntı: `seviye-yapisi` skill'i.
- Çözülemeyen seviye oyuncuya asla verilmez.
- MVP'de elle hazırlanmış ve doğrulanmış seviyeler kullanılır. Procedural generator sonraya kalır.

## Oyun Akışı
Main Menu → Level Selection → Gameplay → Level Completed → Next Level
Gameplay'de: Restart, Undo, level tamamlama, (mümkünse) hamle sayacı. MVP gereksiz özelliklerle şişmeyecek.

## UI / UX
- Sade, temiz, modern, mobil uyumlu. Ekran gereksiz butonlarla dolmaz.
- Gameplay ekranı: level numarası, tüpler, toplar, restart, undo, basit ilerleme bilgisi.
- Karmaşık tutorial yok. İlk seviye doğal olarak öğretici olmalı.

## Görsel Stil
- Minimalist 2D / 2.5D. Temiz, okunabilir tüpler. Parlak ve birbirinden net ayrılan topların renkleri.
- **Yok:** aşırı detay, fotogerçekçilik, aşırı particle, glow, screen shake, aşırı bounce.
- Cazibe: renkler + temiz UI + puzzle. Önce oynanabilir prototip, görsele az zaman.
Ayrıntı: `gorsel-animasyon` skill'i.

## Animasyon ve Ses
- Animasyonlar basit ve kısa: hafif yükselme, hedefe hareket, tüpe yerleşme. Oyunu yavaşlatmaz.
- Ses MVP'de minimum (ball move, invalid move, level complete, button click). `AudioManager` gameplay'den bağımsız.

## Mobil
- Hedef Android, portrait, farklı ekran oranları, düşük/orta seviye cihazlar, touch, performans, bellek.
- Baştan mobil için tasarla. PC için yapıp sonradan çevirmeyeceğiz. Ayrıntı: `unity-mobil-performans` skill'i.

## Mimari
Önerilen sınıflar (gereksiz sınıf üretme, her sınıfın tek sorumluluğu olsun):
`GameManager`, `LevelManager`, `LevelController`, `Tube`, `Ball`, `InputController`, `LevelData`, `ColorDefinition`, `UIManager`, `AudioManager`
- Oyun mantığı (hamle kuralı, kazanma, undo) saf C# ve test edilebilir olsun. Görünümden (Tube/Ball) ayrı.
- Gameplay logic ile UI logic birbirine gereksiz bağlanmaz. Basit C# `Action` olayları yeterli.
- Gereksiz Singleton kullanma. Component bağımlılıklarını açık yönet.
Ayrıntı: `unity-mimari` skill'i.

## Kod Kalitesi
- Okunabilir, modüler, test edilebilir, gereksiz abstraction yok, Unity standartlarına uygun.
- Magic number azalt. Inspector'dan ayarlanacak değerler `[SerializeField] private`.
- Null reference'ları önle.
- Oyun mantığı için Unity Test Framework ile **EditMode** testleri yaz.

## Overengineering Yasağı
Ekleme: karmaşık dependency injection, ECS, aşırı event sistemi, gereksiz design pattern, network/multiplayer/backend/online hesap, karmaşık save mimarisi, gereksiz üçüncü parti framework.
Basit oyuna basit ve sağlam mimari.

## Çalışma Kuralları
1. **Görev gelince önce analiz:** Özelliği analiz et, mevcut mimariyi kontrol et, etkilenen sistemleri belirle, değişiklikleri açıkla, sonra uygula. Küçük ve net değişiklikte uzun analiz yapma.
2. **Onay:** Büyük değişikliklerden önce planı kısaca yaz, benim onayımı bekle ("tamam", "onay", "yap", "başla"). Onay olmadan dosya oluşturma/düzenleme/silme yapma.
3. **Faz sonu:** Her faz bitince **dur**; ne yaptığını, hangi dosyaları eklediğini/değiştirdiğini ve nasıl test edeceğimi özetle. Onayım olmadan sonraki faza geçme.
4. **Dosya güvenliği:** Mevcut dosyaları gereksiz silme veya yeniden oluşturma. Değiştirmeden önce içeriğini ve bağlı sistemleri incele. Mevcut çalışan sistemi bozabilecek değişikliklerde dikkatli ol. Mimariyi tamamen değiştirmek gerekiyorsa önce nedenini açıkla.
5. **Unity dosyaları:** `.meta` dosyalarını elle oluşturma veya silme. Sahne (`.unity`) ve prefab (`.prefab`) YAML dosyalarını ben istemeden düzenleme.
6. **Takılma kuralı:** Aynı dosyayı gereksiz tekrar okuma. Takılırsan veya döngüye girdiğini fark edersen dur, şu ana kadar ne yaptığını ve neyin kaldığını özetle.
7. **Git:** Faz başında ve sonunda commit almamı öner (`git` komutlarını ben istemeden çalıştırma). Unity için uygun `.gitignore` (Library, Temp, Logs, obj, Build, UserSettings) Faz 1'de eklenir.
8. **Editor işleri:** Sen Unity Editor'ü çalıştıramazsın. Sahne kurma, prefab bağlama, Inspector doldurma gerektiren işleri her faz sonunda **numaralı adımlar** olarak ver. Mümkünse nesneleri kodla oluştur ki elle bağlama azalsın.

## Geliştirme Fazları
**Faz 1 – Temel:** proje/klasör yapısı, asmdef'ler (runtime + EditMode test), `.gitignore`, temel sahne, GameManager, seviye sistemi için temel yapı, input sistemi, saf C# `BoardState` ve testleri. Mantık/görünüm ayrımı **baştan** doğru kurulur.
**Faz 2 – Çekirdek oynanış:** Tube, Ball, renk sistemi, seçme, taşıma, geçerli/geçersiz hamle, kapasite, seviye tamamlama. Hamle geçmişini (undo için) baştan tut.
**Faz 3 – Seviye sistemi:** LevelData, yükleme, ilerleme, tamamlama, sonraki seviye, restart.
**Faz 4 – UI:** Main Menu, seviye seçimi, gameplay UI, Level Complete, Restart, Undo.
**Faz 5 – Polish:** basit animasyon, ses, görsel düzen, mobil UI, performans, hata ayıklama.

## MVP Öncelik Sırası
1. Gameplay doğru çalışsın 2. Seviye sistemi 3. Mobil kullanılabilirlik 4. Temiz UI 5. Basit animasyon 6. Ses 7. Görsel polish

## Kayıt Sistemi
Mevcut seviye, tamamlanan seviyeler ve temel ilerleme yerel olarak saklanır. MVP'de PlayerPrefs veya basit yerel çözüm. Bulut kaydı yok.

## Reklam / Gelir
İlk aşamada **reklam yok**. Önce core gameplay tam çalışsın. Sonra (interstitial, rewarded, remove ads vb.) ayrıca tasarlanır ve ayrı onay ister. Ayrıntı: `android-yayin` skill'i.

## Temel Prensip
Başarı basitlikten gelir. Oyuncu birkaç saniyede "Bu tüpteki rengi diğerine taşımalıyım" diyebilmeli. Fazla UI, mekanik, animasyon, tutorial ve metin kullanma.

## Skill'ler
- `unity-mimari`: sınıf yapısı, mantık/görünüm ayrımı, undo, kayıt, klasör düzeni
- `ball-sort-kurallari`: hamle kuralları, kazanma, uç durumlar, test listesi
- `seviye-yapisi`: LevelData, seviye doğrulama, zorluk eğrisi
- `gorsel-animasyon`: minimalist stil, kısa animasyon, ses
- `unity-mobil-performans`: Android/URP performans kontrolleri
- `android-yayin`: build, imza, Play Store, (sonradan) reklam ve IAP
