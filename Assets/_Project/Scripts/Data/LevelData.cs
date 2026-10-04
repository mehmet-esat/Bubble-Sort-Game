using System;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Tek bir seviyenin verisi. Inspector'dan düzenlenir.
    /// İç içe dizi yerine TubeSetup wrapper kullanır.
    /// </summary>
    [CreateAssetMenu(fileName = "Level_01", menuName = "Color Sort Puzzle/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Her tüpün kapasitesi (varsayılan 4)")]
        [SerializeField] private int tubeCapacity = 4;

        [Tooltip("Tüplerin başlangıç durumu. Her TubeSetup'ta balls dizisi tüpün alttan üste sıralı toplarıdır.")]
        [SerializeField] private TubeSetup[] tubes;

        public int TubeCapacity => tubeCapacity;
        public TubeSetup[] Tubes => tubes;

        /// <summary>
        /// Tek bir tüpün başlangıç top dizilimi.
        /// int[] balls: colorId değerleri, index 0 = dip, son index = tepe.
        /// Boş tüp için balls boş dizi olmalı.
        /// </summary>
        [Serializable]
        public class TubeSetup
        {
            [Tooltip("Tüpteki topların renk ID'leri (alttan üste). Boş tüp için boş bırakın.")]
            public int[] balls = Array.Empty<int>();
        }
    }
}
