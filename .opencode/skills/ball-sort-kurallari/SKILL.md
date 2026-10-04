---
name: ball-sort-kurallari
description: Ball sort oyununda hamle kuralı, tüp seçme, top taşıma, kapasite, kazanma koşulu, geçersiz hamle, undo veya oyun mantığı birim testi yazarken kullan. Oyun mantığının tek doğru kaynağıdır.
---

# Ball Sort Oyun Kuralları

## Durum Modeli
- Tüp = alttan üste sıralı top (renk id) listesi. En üst top listenin son elemanı.
- Her tüpün kapasitesi vardır (varsayılan 4, seviye verisinden gelir).
- Her renkten, tüp kapasitesi kadar top vardır.

## Hamle: `CanMove(source, target)`
Geçerli ancak ve ancak:
1. `source != target`
2. Kaynak boş değil
3. Hedefte boş yer var (`target.Count < capacity`)
4. Hedef boş **veya** `target.Top == source.Top`

## Hamle Uygulama: `Move(source, target)`
- Kaynağın **en üstündeki tek top** hedefe taşınır (bir hamlede bir top).
- Hamle undo yığınına yazılır (kaynak, hedef, renk).
- Geçersiz hamle oyun durumunu (`BoardState`) **değiştirmez**, hata/çökme olmaz.

## Seçim Akışı
- Boş tüp kaynak olarak seçilemez (dokunma yok sayılır).
- Tüp seçilince en üst top hafifçe yükselir.
- Aynı tüpe tekrar dokunmak seçimi iptal eder.
- Başka tüpe dokunulunca hamle denenir. Geçerliyse uygulanır ve seçim biter.
- Geçersizse "invalid move" olayı yayılır ve dokunulan tüp **boş değilse seçim o tüpe geçer** (yeni kaynak olur). Hızlı ve kullanıcı dostu davranış, bunu değiştirme.

## Kazanma: `IsSolved`
Boş olmayan **her tüp dolu ve tek renklidir.** Boş tüpler kazanmayı bozmaz.
(Yalnızca "her tüpte tek renk" kontrolü yetmez, bir rengin topları iki yarım tüpe bölünmüş olabilir.)

## MVP Kapsamı
Yapılacak: hamle, kapasite, geçersiz hamle, kazanma, undo, restart.
MVP dışı (istenirse sonra): boş tüp ekleme, kilitlenme (deadlock) tespiti, ipucu, tamamlanan tüpü kilitleme. Bunları kendi kafana göre ekleme.

## Birim Testleri (EditMode, zorunlu)
- Boş hedefe taşıma → geçerli
- Aynı renge taşıma → geçerli
- Farklı renge taşıma → geçersiz, durum değişmez
- Dolu hedefe taşıma → geçersiz
- Aynı tüpe taşıma → geçersiz
- Seçim akışı (mantık tarafında test edilebilen kısım): geçersiz hamlede kaynak durumu değişmez
- Boş kaynaktan taşıma → geçersiz
- Undo sonrası durumun birebir eski hâle dönmesi
- `IsSolved`: tüm tüpler dolu/tek renk (+ boş tüpler) → true
- `IsSolved`: bir renk iki yarım tüpe bölünmüşse → false
