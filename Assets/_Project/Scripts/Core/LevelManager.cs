using System;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Resources/Levels altındaki tüm seviyeleri okur ve sırayla yönetir.
    /// Oyuncunun mevcut ilerlemesini PlayerPrefs ile saklar (sadece index).
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private LevelData[] _levels;
        private int _currentIndex;

        private const string PREF_CURRENT_LEVEL = "CurrentLevelIndex";

        public void Initialize()
        {
            // Resources klasöründen tüm LevelData'ları yükle
            _levels = Resources.LoadAll<LevelData>("Levels");
            
            // İsme göre sırala (Level_001, Level_002 ...)
            Array.Sort(_levels, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            
            if (_levels.Length == 0)
            {
                Debug.LogWarning("LevelManager: Resources/Levels klasöründe hiç seviye bulunamadı!");
            }

            // Kayıtlı ilerlemeyi al
            _currentIndex = PlayerPrefs.GetInt(PREF_CURRENT_LEVEL, 0);
        }

        public LevelData GetCurrentLevel()
        {
            if (_levels == null || _levels.Length == 0) return null;
            
            // Eğer kayıtlı index toplam seviye sayısını aşarsa başa dön
            int safeIndex = _currentIndex % _levels.Length;
            return _levels[safeIndex];
        }

        public void CompleteCurrentLevel()
        {
            _currentIndex++;
            PlayerPrefs.SetInt(PREF_CURRENT_LEVEL, _currentIndex);
            PlayerPrefs.Save();
        }

        public int GetCurrentDisplayLevelNumber()
        {
            return _currentIndex + 1; // UI'da 0. seviye değil 1. seviye yazması için
        }
    }
}
