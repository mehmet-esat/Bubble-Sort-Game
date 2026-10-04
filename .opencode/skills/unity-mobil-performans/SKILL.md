---
name: unity-mobil-performans
description: Ball sort oyununda performans, FPS, bellek, build boyutu, URP ayarları, UI/animasyon maliyeti, ekran oranları veya düşük/orta seviye Android cihaz uyumu konuşulurken kullan.
---

# Android / URP Performans

Bu oyun hafif bir puzzle. Düşük/orta seviye cihazlarda sorunsuz çalışmalı. Hedef: 60 FPS, zayıf cihazda en az 30 FPS.

## Kod
- Update/FixedUpdate içinde `new`, LINQ, string birleştirme, `GetComponent`, `Find` kullanma.
- Top ve tüp sayısı sabitse nesneleri seviye başında oluştur, hamle sırasında yaratıp yok etme (gerekirse basit pooling).
- `Debug.Log`'ları release build'de kapat.
- Çözücü gibi ağır hesaplar Editor'de çalışsın, oyun içinde değil.

## URP / Görsel
- Hafif URP ayarı. Gölge ve gerçek zamanlı ışık gerekmez.
- Post-processing kullanma (zaten glow/bloom yok).
- Sprite Atlas, az materyal (batching).
- Büyük yarı saydam yüzeylerden kaçın.

## UI
- Statik ve dinamik UI'ı ayrı Canvas'lara böl.
- Gereksiz Raycast Target'ları kapat.

## Mobil Uyum
- Portrait, farklı ekran oranları (16:9, 20:9 vb.), çentik/güvenli alan.
- Dokunma hedefleri (tüpler, butonlar) parmak için yeterince büyük.
- Texture boyutu ve sıkıştırmayı mobile uygun ayarla.

## Ölçüm
- Tahmin etme, ölç: Profiler ve gerçek Android cihaz. Editör mobili yansıtmaz.
- Faz 5'te (ve büyük değişikliklerden sonra) gerçek cihazda FPS ve bellek kontrolü öner.
- Build boyutunu Build Report ile kontrol et.
