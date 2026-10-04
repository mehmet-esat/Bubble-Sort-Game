---
name: seviye-yapisi
description: Ball sort oyununda LevelData (ScriptableObject), seviye yükleme, ilerleme, yeni seviye ekleme, renk ve tüp sayısı, zorluk eğrisi veya seviyenin çözülebilirlik doğrulaması konuşulurken kullan. Çözülemeyen seviyenin oyuncuya verilmemesini garanti eder.
---

# Seviye Yapısı ve Doğrulama

## Kural: Seviyeler Hard-Coded Olmaz
Renkleri ve tüpleri koda gömme. Seviye verisi ScriptableObject'tedir.

## `LevelData` Alanları
- `levelId`
- `colorCount`
- `tubeCount`
- `tubeCapacity` (varsayılan 4)
- `initialTubes`: her tüp için alttan üste renk id listesi (başlangıç dizilimi)
- Boş tüp sayısı (`tubeCount - colorCount` ile uyumlu olmalı, ayrıca doğrula)
- `difficulty` (isteğe bağlı)

Seviyeler tek bir `LevelDatabase` (ScriptableObject listesi) içinde toplanır, `LevelManager` buradan okur.

## Doğrulama (veri tutarlılığı)
Her seviye için şunlar kontrol edilir (Editor aracı veya EditMode testi):
- Her renkten tam `tubeCapacity` kadar top var
- Hiçbir tüp kapasiteyi aşmıyor
- Renk sayısı ve tüp sayısı alanlarla uyumlu
- Seviye **çözülebilir** (aşağıya bak)

## Çözülebilirlik
Rastgele dizilmiş her başlangıç çözülebilir olmaz. Oyuncuya çözülemeyen seviye verilmez.
- **MVP:** Elle hazırlanmış seviyeler. Her biri çözülebilir olduğu kanıtlanarak eklenir (çözücü ile veya çözüm adımları yazılarak).
- **Çözücü (küçük, saf C#):** Durum uzayı araması (BFS/DFS). Ziyaret edilen durumları hash'le, tüp sıralamasını normalize et, arama bütçesi koy. Editor'de çalışır, oyun içinde değil.
- **Procedural generator:** MVP'den sonra. Üretilen her seviye çözücüyle doğrulanmadan kullanılmaz. (Alternatif: çözülmüş durumdan geçerli hamlelerin tersini uygulayarak karıştırmak çözülebilirliği garanti eder.)

## Zorluk Eğrisi
- İlk seviyeler: 3-4 renk, 1-2 boş tüp, ör. 3 renk + 1 boş, 4 renk + 2 boş.
- Sonra 5, 6, 7, 8 renk. Tüp sayısı buna göre artar.
- Temel kurallar hiç değişmez.
- Arada kolay "nefes" seviyeleri koy.
- Çok sayıda renk/tüp, ekrana sığmayı etkiler. Üst sınırı mobil ekranda test ederek belirle.

## İlerleme
- Mevcut seviye ve tamamlanan seviyeler yerel kaydedilir (bkz. `unity-mimari`).
- Sonraki seviye yoksa (son seviye) oyuncuya bunu zarifçe göster.

## Yeni Seviye Önerirken Çıktı
- Seviye no, renk sayısı, tüp sayısı, kapasite
- Tüp tüp başlangıç dizilimi (alttan üste)
- Çözülebilirlik kanıtı (çözüm adımları veya çözücü sonucu), mümkünse minimum hamle
