using System;
using System.Collections.Generic;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Oyun tahtasının mantıksal durumu. Saf C#, UnityEngine bağımlılığı yok.
    /// Tüpler List&lt;int&gt; olarak tutulur (int = colorId). 
    /// Liste indeksi 0 = tüpün dibi, son eleman = tüpün tepesi.
    /// </summary>
    public class BoardState
    {
        private readonly List<List<int>> _tubes;
        private readonly int _tubeCapacity;
        private readonly Stack<MoveRecord> _moveHistory;

        public int TubeCount => _tubes.Count;
        public int TubeCapacity => _tubeCapacity;
        public int MoveCount => _moveHistory.Count;

        public BoardState(int tubeCapacity, List<List<int>> initialTubes)
        {
            if (tubeCapacity <= 0)
                throw new ArgumentException("Tube capacity must be greater than zero.", nameof(tubeCapacity));
            if (initialTubes == null)
                throw new ArgumentNullException(nameof(initialTubes));

            _tubeCapacity = tubeCapacity;
            _moveHistory = new Stack<MoveRecord>();

            // Deep copy so external changes don't affect board state
            _tubes = new List<List<int>>(initialTubes.Count);
            for (int i = 0; i < initialTubes.Count; i++)
            {
                _tubes.Add(new List<int>(initialTubes[i]));
            }
        }

        /// <summary>
        /// Belirtilen tüpteki topları döndürür (readonly kopya).
        /// </summary>
        public IReadOnlyList<int> GetTube(int tubeIndex)
        {
            ValidateTubeIndex(tubeIndex);
            return _tubes[tubeIndex].AsReadOnly();
        }

        /// <summary>
        /// Belirtilen tüpteki top sayısını döndürür.
        /// </summary>
        public int GetBallCount(int tubeIndex)
        {
            ValidateTubeIndex(tubeIndex);
            return _tubes[tubeIndex].Count;
        }

        /// <summary>
        /// Belirtilen tüpün en üstündeki topun colorId'sini döndürür.
        /// Tüp boşsa -1 döner.
        /// </summary>
        public int PeekTop(int tubeIndex)
        {
            ValidateTubeIndex(tubeIndex);
            var tube = _tubes[tubeIndex];
            return tube.Count > 0 ? tube[tube.Count - 1] : -1;
        }

        /// <summary>
        /// source → target hamlesinin geçerli olup olmadığını kontrol eder.
        /// </summary>
        public bool CanMove(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _tubes.Count) return false;
            if (targetIndex < 0 || targetIndex >= _tubes.Count) return false;
            if (sourceIndex == targetIndex) return false;

            var source = _tubes[sourceIndex];
            var target = _tubes[targetIndex];

            // Source boş → geçersiz
            if (source.Count == 0) return false;

            // Target dolu → geçersiz
            if (target.Count >= _tubeCapacity) return false;

            // Target boş → geçerli
            if (target.Count == 0) return true;

            // Renk eşleşmesi kontrol
            int sourceTopColor = source[source.Count - 1];
            int targetTopColor = target[target.Count - 1];
            return sourceTopColor == targetTopColor;
        }

        /// <summary>
        /// Hamleyi uygular. Başarılıysa true, geçersizse false döner.
        /// Geçerli hamle move history'ye kaydedilir.
        /// </summary>
        public bool Move(int sourceIndex, int targetIndex)
        {
            if (!CanMove(sourceIndex, targetIndex))
                return false;

            var source = _tubes[sourceIndex];
            int colorId = source[source.Count - 1];
            source.RemoveAt(source.Count - 1);
            _tubes[targetIndex].Add(colorId);

            _moveHistory.Push(new MoveRecord(sourceIndex, targetIndex, colorId));
            return true;
        }

        /// <summary>
        /// Son hamleyi geri alır. Geri alınacak hamle yoksa false döner.
        /// </summary>
        public bool Undo()
        {
            if (_moveHistory.Count == 0)
                return false;

            var lastMove = _moveHistory.Pop();
            var target = _tubes[lastMove.TargetIndex];

            // Topu hedeften al, kaynağa geri koy
            target.RemoveAt(target.Count - 1);
            _tubes[lastMove.SourceIndex].Add(lastMove.ColorId);
            return true;
        }

        /// <summary>
        /// Oyunun çözülüp çözülmediğini kontrol eder.
        /// Kazanma koşulu: boş olmayan her tüp dolu (kapasite kadar top) VE tek renkli.
        /// </summary>
        public bool IsSolved()
        {
            for (int i = 0; i < _tubes.Count; i++)
            {
                var tube = _tubes[i];

                // Boş tüp → sorun yok, atla
                if (tube.Count == 0)
                    continue;

                // Dolu değilse → çözülmemiş
                if (tube.Count != _tubeCapacity)
                    return false;

                // Tek renk mi kontrol et
                int firstColor = tube[0];
                for (int j = 1; j < tube.Count; j++)
                {
                    if (tube[j] != firstColor)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Hamle geçmişini temizler (restart/yeni seviye için).
        /// </summary>
        public void ClearHistory()
        {
            _moveHistory.Clear();
        }

        private void ValidateTubeIndex(int index)
        {
            if (index < 0 || index >= _tubes.Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Tube index {index} is out of range. Board has {_tubes.Count} tubes.");
        }
    }
}
