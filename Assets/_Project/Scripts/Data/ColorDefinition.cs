using System;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// ColorId → Color eşlemesi. Inspector'dan düzenlenir.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorDefinition", menuName = "Color Sort Puzzle/Color Definition")]
    public class ColorDefinition : ScriptableObject
    {
        [SerializeField] private ColorEntry[] colors;

        public int ColorCount => colors != null ? colors.Length : 0;

        /// <summary>
        /// ColorId'ye karşılık gelen rengi döndürür.
        /// Geçersiz id için magenta döner (hata rengi).
        /// </summary>
        public Color GetColor(int colorId)
        {
            if (colors == null || colorId < 0 || colorId >= colors.Length)
                return Color.magenta;

            return colors[colorId].color;
        }

        [Serializable]
        public class ColorEntry
        {
            public string name;
            public Color color = Color.white;
        }
    }
}
