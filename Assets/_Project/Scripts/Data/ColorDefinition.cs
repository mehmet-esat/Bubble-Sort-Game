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

        private Sprite[] _cachedSprites;

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

        /// <summary>
        /// ColorId'ye karşılık gelen top sprite'ını döndürür.
        /// Inspector'dan atanmışsa onu, atanmamışsa Resources/Balls/ball_{name} yolundan otomatik yükler.
        /// </summary>
        public Sprite GetSprite(int colorId)
        {
            if (colors == null || colorId < 0 || colorId >= colors.Length)
                return null;

            if (colors[colorId].ballSprite != null)
                return colors[colorId].ballSprite;

            if (_cachedSprites == null || _cachedSprites.Length != colors.Length)
                _cachedSprites = new Sprite[colors.Length];

            if (_cachedSprites[colorId] == null && !string.IsNullOrEmpty(colors[colorId].name))
            {
                string colorName = colors[colorId].name.ToLowerInvariant().Trim();
                _cachedSprites[colorId] = Resources.Load<Sprite>($"Balls/ball_{colorName}");
            }

            return _cachedSprites[colorId];
        }

        [Serializable]
        public class ColorEntry
        {
            public string name;
            public Color color = Color.white;
            public Sprite ballSprite;
        }
    }
}
