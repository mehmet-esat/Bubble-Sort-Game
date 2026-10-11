using System;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Resources/Levels altındaki tüm seviyeleri okur ve sırayla yönetir.
    /// Oyuncunun ilerlemesini LevelProgressState ile yönetir, kalıcı kaydı PlayerPrefs ile saklar.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private LevelData[] _levels;
        private LevelProgressState _progressState;

        private const string PREF_CURRENT_LEVEL = "CurrentLevelIndex";

        public int LevelCount => _levels != null ? _levels.Length : 0;
        public int HighestUnlockedIndex => _progressState != null ? _progressState.HighestUnlockedIndex : 0;
        public int PlayingIndex => _progressState != null ? _progressState.PlayingIndex : 0;

        public void Initialize()
        {
            _levels = Resources.LoadAll<LevelData>("Levels");
            
            Array.Sort(_levels, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            
            if (_levels.Length == 0)
            {
                Debug.LogWarning("LevelManager: Resources/Levels klasöründe hiç seviye bulunamadı!");
            }

            int savedIndex = PlayerPrefs.GetInt(PREF_CURRENT_LEVEL, 0);
            _progressState = new LevelProgressState(savedIndex, _levels.Length);
        }

        public LevelData GetCurrentLevel()
        {
            if (_levels == null || _levels.Length == 0 || _progressState == null)
                return null;

            int index = _progressState.PlayingIndex;
            if (index < 0 || index >= _levels.Length)
                return null;

            return _levels[index];
        }

        public bool SetPlayingLevel(int index)
        {
            if (_progressState == null) return false;
            return _progressState.SetPlayingIndex(index);
        }

        public bool IsLevelUnlocked(int index)
        {
            if (_progressState == null) return false;
            return _progressState.IsLevelUnlocked(index);
        }

        public void CompleteCurrentLevel()
        {
            if (_progressState == null) return;

            bool unlockedNext = _progressState.CompleteCurrentLevel();
            if (unlockedNext)
            {
                PlayerPrefs.SetInt(PREF_CURRENT_LEVEL, _progressState.HighestUnlockedIndex);
                PlayerPrefs.Save();
            }
        }

        public int GetCurrentDisplayLevelNumber()
        {
            if (_progressState == null) return 1;
            return _progressState.GetDisplayLevelNumber();
        }
    }
}
