---
name: unity-mimari
description: Ball sort oyununda yeni C# sınıfı, ScriptableObject, oyun mantığı (BoardState), undo, kayıt, Input System kodu yazarken ya da klasör yapısını kurarken kullan. Basit ve test edilebilir mimari kurallarını içerir, overengineering'i engeller.
---

# Unity Mimari Kuralları

Basit oyuna basit mimari. Her sınıfın tek bir sorumluluğu olsun, sırf isim çoğaltmak için sınıf açma.

## Sınıflar
| Sınıf | Sorumluluk |
|---|---|
| `GameManager` | Oyun durumu (menü, oynuyor, bitti), sahne akışı |
| `LevelManager` | Seviye listesi, mevcut seviye, ilerleme, sonraki seviye |
| `LevelController` | Aktif seviyeyi kurar, hamleleri yönetir, kazanmayı kontrol eder |
| `Tube`, `Ball` | Görünüm bileşenleri (konum, animasyon) |
| `InputController` | Dokunmayı okur, tüp seçimini `LevelController`'a iletir |
| `LevelData` | Seviye verisi (ScriptableObject) |
| `ColorDefinition` | ColorId → renk eşlemesi (ScriptableObject) |
| `UIManager` | Ekranlar ve butonlar |
| `AudioManager` | Sesler, gameplay'den bağımsız |

## Mantık / Görünüm Ayrımı
- Oyun kuralları saf C# sınıfında durur (öneri: `BoardState`): tüp içerikleri, `CanMove`, `Move`, `IsSolved`, undo.
- `Tube`/`Ball` MonoBehaviour'ları sadece **gösterir**. Mantık kararı vermez.
- `BoardState`, `UnityEngine`'e bağımlı olmamalı, böylece EditMode testleri yazılabilir.
- Mantık → görünüm iletişimi basit C# `Action` olaylarıyla yapılır (ör. `OnBallMoved`, `OnLevelCompleted`). Daha fazla olay sistemi kurma.
- Gameplay mantığı UI'ı bilmez, UI gameplay'i doğrudan değiştirmez.

## Bağımlılıklar
- Bağımlılıkları `[SerializeField] private` ile açıkça bağla. `Find`/`GetComponent` çağrılarını Update'te kullanma.
- Gereksiz Singleton açma. Gerekirse sadece `GameManager`/`AudioManager` düşünülebilir, önce sor.
- Dependency injection framework, ECS, servis locator **yok**.

## Undo
- Her geçerli hamle bir kayıt olarak yığına yazılır: kaynak tüp, hedef tüp, top rengi.
- Undo, son kaydın tersini uygular. Yığın seviye başında/restart'ta temizlenir.
- Faz 2'de baştan tut (sonradan eklemek pahalı), UI düğmesi Faz 4'te bağlanır.

## Kayıt
- Mevcut seviye ve tamamlanan seviyeler `PlayerPrefs` ile (veya tek bir küçük JSON ile) saklanır.
- Karmaşık kayıt mimarisi kurma. Kayıt kodu tek bir yerde (ör. `LevelManager` veya küçük bir `SaveService`) toplansın.

## Input System
- `UnityEngine.InputSystem` kullan. Eski `UnityEngine.Input` yasak.
- Dokunulan tüpü bulmak için collider/raycast. UI üstüne dokunma oyun girişine karışmasın.
- Animasyon sürerken yeni girişi kilitle.

## Klasör Düzeni
```
Assets/_Project/
  Scripts/
    Core/        (GameManager, LevelManager, LevelController)
    Gameplay/    (BoardState, Tube, Ball, InputController)
    Data/        (LevelData, ColorDefinition)
    UI/
    Audio/
  ScriptableObjects/
  Prefabs/
  Art/
  Audio/
  Scenes/
  Tests/
    EditMode/
```

## Assembly Definition (asmdef)
Test assembly'si, asmdef'siz (`Assembly-CSharp`) kodu göremez. Bu yüzden baştan iki asmdef kur:
- `Assets/_Project/Scripts/ColorSortPuzzle.Runtime.asmdef`: tüm runtime kod. Gereken paket assembly'lerine referans verir (Input System, TextMeshPro/uGUI). Paket assembly adlarını Unity 6.6 sürümünde doğrula, tahmin etme.
- `Assets/_Project/Tests/EditMode/ColorSortPuzzle.Tests.EditMode.asmdef`: yalnızca Editor, Runtime asmdef'i ve Unity Test Framework'ü referans alır.
- `.meta` dosyalarını elle yazma, Unity üretir. asmdef ayarlarında emin değilsen Editor'de yapılacak adımı bana numaralı ver.
- `BoardState` ve testleri `UnityEngine`'e bağımlı olmadan derlenmeli.

## Kod Yazmadan Önce
- [ ] Bu sınıfın tek sorumluluğu ne?
- [ ] Mantık görünüme karışmış mı?
- [ ] Gereksiz soyutlama/singleton/olay var mı?
- [ ] Onay alındı mı? (AGENTS.md)
