namespace ColorSortPuzzle
{
    /// <summary>
    /// Seviye ilerleme ve kilit mantığını yöneten saf C# sınıfı.
    /// UnityEngine bağımlılığı yoktur, EditMode testleri ile bağımsız doğrulanabilir.
    /// </summary>
    public class LevelProgressState
    {
        public int HighestUnlockedIndex { get; private set; }
        public int PlayingIndex { get; private set; }
        public int TotalLevels { get; private set; }
        public bool IsOnLastLevel => TotalLevels > 0 && PlayingIndex == TotalLevels - 1;

        public LevelProgressState(int savedHighestUnlockedIndex, int totalLevels)
        {
            TotalLevels = totalLevels > 0 ? totalLevels : 0;
            if (TotalLevels == 0)
            {
                HighestUnlockedIndex = 0;
                PlayingIndex = 0;
                return;
            }

            HighestUnlockedIndex = ClampIndex(savedHighestUnlockedIndex);
            PlayingIndex = HighestUnlockedIndex;
        }

        public bool SetPlayingIndex(int index)
        {
            if (!IsLevelUnlocked(index))
                return false;

            PlayingIndex = index;
            return true;
        }

        public bool IsLevelUnlocked(int index)
        {
            if (TotalLevels == 0) return false;
            return index >= 0 && index <= HighestUnlockedIndex && index < TotalLevels;
        }

        /// <summary>
        /// Mevcut oynanan seviyeyi tamamlar.
        /// highestUnlockedIndex SADECE playingIndex == highestUnlockedIndex ise 1 artar ve son indeksi aşamaz.
        /// Eski bir seviyeyi bitirmek ilerlemeyi artırmaz.
        /// Son seviye tamamlandığında playingIndex 0'a döner, highestUnlockedIndex değişmez.
        /// İlerleme arttıysa (kaydedilmesi gerekiyorsa) true döner.
        /// </summary>
        public bool CompleteCurrentLevel()
        {
            if (TotalLevels == 0)
                return false;

            bool isLastLevel = (PlayingIndex == TotalLevels - 1);
            bool unlockedNext = false;

            if (PlayingIndex == HighestUnlockedIndex && HighestUnlockedIndex < TotalLevels - 1)
            {
                HighestUnlockedIndex++;
                unlockedNext = true;
            }

            if (isLastLevel)
            {
                PlayingIndex = 0;
            }
            else
            {
                PlayingIndex++;
            }

            return unlockedNext;
        }

        public int GetDisplayLevelNumber()
        {
            if (TotalLevels == 0) return 0;
            return PlayingIndex + 1;
        }

        private int ClampIndex(int index)
        {
            if (TotalLevels == 0) return 0;
            if (index < 0) return 0;
            if (index >= TotalLevels) return TotalLevels - 1;
            return index;
        }
    }
}
