---
name: android-yayin
description: Ball sort oyununu Android'e build alırken, imzalama, AAB, Google Play Console, mağaza sayfası, yayın öncesi kontrol listesi veya (MVP sonrası) reklam, ödüllü reklam, IAP konuşulurken kullan.
---

# Android Yayın ve Gelir

Play Store gereksinimleri (hedef API seviyesi, veri güvenliği formu, reklam/gizlilik kuralları) sık değişir. Yayından önce güncel resmi gereksinimlere bakmayı öner, hafızadan kesin bilgi verme.

## Build
- Play Store için **AAB**, test için APK.
- Scripting Backend: IL2CPP, mimari: ARM64 (gerekirse ARMv7).
- Hedef/minimum API seviyelerini yayın zamanındaki Play kurallarına göre ayarla.
- Release build'de Development Build ve Debug.Log kapalı.

## İmzalama
- Keystore ve şifreleri **asla** repoya koyma (.gitignore).
- Keystore'u yedekle. Kaybolursa uygulama güncellenemez.
- Play App Signing'i değerlendir.

## Reklam / Gelir (MVP'de YOK)
İlk geliştirme aşamasında reklam ekleme. Core gameplay tamamen çalıştıktan sonra, kullanıcının onayıyla ayrıca tasarlanır:
- Ödüllü reklam (ör. ek yardım için), seviye geçişinde interstitial, reklamları kaldırma (IAP).
- Hangi reklam ağının kullanılacağı kullanıcı kararıdır.
- Gizlilik politikası, veri güvenliği bildirimi ve bölgeye göre kullanıcı onayı (consent) gereksinimleri kontrol edilir.

## Yayın Öncesi Kontrol Listesi
- [ ] Gerçek cihazda FPS ve bellek testi
- [ ] Farklı ekran oranlarında UI (çentik, güvenli alan)
- [ ] Orijinal uygulama adı ve ikonu (başka oyundan bağımsız)
- [ ] Ekran görüntüleri ve açıklama
- [ ] Gizlilik politikası linki
- [ ] Veri güvenliği formu
- [ ] İç test (internal testing) kanalında deneme
