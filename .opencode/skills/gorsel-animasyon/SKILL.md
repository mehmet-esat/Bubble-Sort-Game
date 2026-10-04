---
name: gorsel-animasyon
description: Ball sort oyununda tüp ve top görünümü, renk seçimi, kısa animasyonlar, seçme geri bildirimi, ses efektleri, UI düzeni veya görsel stil kararları verilirken kullan. Minimalist ve sade stili korur, aşırı efekti engeller.
---

# Görsel Stil, Animasyon, Ses

Cazibe: **renkler + temiz UI + puzzle.** Önce oynanabilir prototip, görsele fazla zaman harcama.

## Stil
- Minimalist 2D / 2.5D. Temiz, okunabilir tüpler.
- Toplar parlak ve birbirinden **net ayırt edilebilir** renklerde.
- Yok: fotogerçekçilik, aşırı detay, aşırı particle, glow, screen shake, aşırı bounce, karmaşık efekt.
- Orijinal tasarım: başka oyunun ikon/top/tüp görsellerini kopyalama.

## Renkler
- Renkler `ColorDefinition` ScriptableObject'inde (ColorId → renk).
- Benzer renk çiftlerini (kırmızı/turuncu, yeşil/açık yeşil) birbirinden belirgin ayır.
- Renk körlüğü desteği (desen/sembol) MVP sonrası değerlendirilir, şimdilik renkleri ayırt edilebilir seçmek yeterli.

## Animasyon (kısa ve basit)
- **Seçme:** Seçilen tüpün en üst topu hafifçe yükselir.
- **Taşıma:** Hedefe doğru hareket, tüpe yerleşme.
- **Geçersiz hamle:** Animasyon yok veya çok hafif, sadece ses.
- Animasyon gameplay'i yavaşlatmaz. Süre kısa tutulur ve Inspector'dan ayarlanır.
- Animasyon sürerken girişi kilitle.
- Başlangıçta Coroutine veya basit eğri yeterli. Tween kütüphanesi ekleme (gereksiz üçüncü parti yok), gerekirse önce sor.

## Ses (MVP'de minimum)
- `ball move`, `invalid move`, `level complete`, `button click`.
- `AudioManager` gameplay sistemlerinden bağımsız. Gameplay sadece olay yayar, ses kodu gameplay'i bilmez.
- Sesler sonra eklenebilir, ilk fazlarda gerekmez.

## UI
- Gameplay ekranı: level numarası, tüpler, toplar, restart, undo, basit ilerleme bilgisi. Başka bir şey ekleme.
- Düğmeler başparmakla erişilebilir alt bölgede, güvenli alan (safe area) içinde.
- Main Menu ve Level Selection sade. Karmaşık tutorial, fazla metin yok.

## Yerleşim
- Tüpler satırlara dizilir, tüp sayısı arttıkça ekrana sığacak şekilde ölçeklenir.
- Farklı en/boy oranlarında ve çentikli ekranlarda test et.
