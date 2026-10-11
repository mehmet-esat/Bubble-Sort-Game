using UnityEngine;

namespace ColorSortPuzzle
{
    public static class SpriteFactory
    {
        private static Sprite _ballSprite;
        private static Sprite _ballShadowSprite;
        private static Sprite _ballHighlightSprite;
        private static Sprite _ballAccentSprite;
        private static Sprite _tubeSprite;
        private static Sprite _backgroundSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            _ballSprite = null;
            _ballShadowSprite = null;
            _ballHighlightSprite = null;
            _ballAccentSprite = null;
            _tubeSprite = null;
            _backgroundSprite = null;
        }

        /// <summary>
        /// Renk katmanının kullandığı yüksek çözünürlüklü beyaz daire sprite'ı.
        /// </summary>
        public static Sprite GetBallSprite()
        {
            if (_ballSprite != null) return _ballSprite;

            const int radius = 115;
            const int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x - size * 0.5f + 0.5f;
                    float cy = y - size * 0.5f + 0.5f;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);

                    if (dist > radius)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    byte alpha = 255;
                    if (dist > radius - 1f)
                    {
                        // Anti-aliasing yalnızca en dış 1px kenarda uygulanır.
                        alpha = (byte)Mathf.RoundToInt(255f * (radius - dist));
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 256f);
            return _ballSprite;
        }

        /// <summary>
        /// Merkezde saydam, sağ-alt yönünde yoğunlaşan siyah radial gölge katmanı.
        /// </summary>
        public static Sprite GetBallShadowSprite()
        {
            if (_ballShadowSprite != null) return _ballShadowSprite;

            const int size = 256;
            const float radius = 115f;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Vector2 shadowCenter = new Vector2(radius * 0.22f, -radius * 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x - size * 0.5f + 0.5f, y - size * 0.5f + 0.5f);
                    float circleDistance = point.magnitude;
                    if (circleDistance > radius)
                        continue;

                    float shadowDistance = Vector2.Distance(point, shadowCenter);
                    float shadowAlpha = Mathf.SmoothStep(0f, 0.45f, Mathf.InverseLerp(radius * 0.30f, radius * 1.12f, shadowDistance));
                    if (circleDistance > radius - 1f)
                        shadowAlpha *= radius - circleDistance;

                    pixels[y * size + x] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(shadowAlpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            _ballShadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 256f);
            return _ballShadowSprite;
        }

        /// <summary>
        /// Sol-üstte yumuşak kenarlı, hafif eğik oval beyaz ana parlama.
        /// </summary>
        public static Sprite GetBallHighlightSprite()
        {
            if (_ballHighlightSprite != null) return _ballHighlightSprite;

            const int size = 256;
            const float radius = 115f;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[size * size];

            Vector2 center = new Vector2(-radius * 0.40f, radius * 0.40f);
            const float cos = 0.819152f;
            const float sin = -0.573576f;
            Vector2 halfSize = new Vector2(radius * 0.28f, radius * 0.14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - size * 0.5f + 0.5f - center.x;
                    float dy = y - size * 0.5f + 0.5f - center.y;
                    float u = dx * cos - dy * sin;
                    float v = dx * sin + dy * cos;
                    float normalizedDistance = Mathf.Sqrt((u * u) / (halfSize.x * halfSize.x) + (v * v) / (halfSize.y * halfSize.y));
                    float alpha = 1f - Mathf.SmoothStep(0.68f, 1f, normalizedDistance);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 200f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            _ballHighlightSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 256f);
            return _ballHighlightSprite;
        }

        /// <summary>
        /// Ana parlamanın yanındaki küçük beyaz nokta ile alt kenardaki ince açık rim ışığı.
        /// </summary>
        public static Sprite GetBallAccentSprite()
        {
            if (_ballAccentSprite != null) return _ballAccentSprite;

            const int size = 256;
            const float radius = 115f;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[size * size];
            Vector2 dotCenter = new Vector2(-radius * 0.17f, radius * 0.20f);
            float dotRadius = radius * 0.06f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x - size * 0.5f + 0.5f, y - size * 0.5f + 0.5f);
                    float dotAlpha = 1f - Mathf.SmoothStep(dotRadius * 0.55f, dotRadius, Vector2.Distance(point, dotCenter));

                    float circleDistance = point.magnitude;
                    float angle = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
                    if (angle < 0f) angle += 360f;
                    float rimAlpha = 0f;
                    if (angle >= 200f && angle <= 340f && circleDistance >= radius * 0.88f && circleDistance <= radius * 0.96f)
                    {
                        float radial = Mathf.InverseLerp(radius * 0.88f, radius * 0.96f, circleDistance);
                        rimAlpha = Mathf.Sin(radial * Mathf.PI) * 0.24f;
                    }

                    float alpha = Mathf.Max(dotAlpha * 0.70f, rimAlpha);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            _ballAccentSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 256f);
            return _ballAccentSprite;
        }

        /// <summary>
        /// İnce soğuk beyaz/gri konturlu, yuvarlak alt köşeli, üstü açık cam tüp sprite'ı (120x430 px).
        /// İçi tamamen şeffaftır; arka planı olduğu gibi gösterir.
        /// </summary>
        public static Sprite GetTubeSprite()
        {
            if (_tubeSprite != null) return _tubeSprite;

            int width = 120;
            int height = 430;
            float wallThickness = 7.5f;
            float cornerRadius = 38f; // Belirgin yuvarlatılmış U-şekli alt köşe

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[width * height];
            Color32 transparent = new Color32(0, 0, 0, 0);

            // İnce soğuk beyaz/gri cam kontur rengi
            Color32 wallColor = new Color32(210, 220, 225, 195);

            float xMin = 1f;
            float xMax = width - 1f;
            float yMin = 1f;

            float rOuter = cornerRadius;
            float rInner = cornerRadius - wallThickness;

            float xMinIn = xMin + wallThickness;
            float xMaxIn = xMax - wallThickness;
            float yMinIn = yMin + wallThickness;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // 1. Dış sınır SDF (dOuter <= 0 tüpün içi/duvarı)
                    float dOuter;
                    if (py < yMin + rOuter)
                    {
                        if (px < xMin + rOuter)
                        {
                            float dx = px - (xMin + rOuter);
                            float dy = py - (yMin + rOuter);
                            dOuter = Mathf.Sqrt(dx * dx + dy * dy) - rOuter;
                        }
                        else if (px > xMax - rOuter)
                        {
                            float dx = px - (xMax - rOuter);
                            float dy = py - (yMin + rOuter);
                            dOuter = Mathf.Sqrt(dx * dx + dy * dy) - rOuter;
                        }
                        else
                        {
                            dOuter = yMin - py;
                        }
                    }
                    else
                    {
                        dOuter = Mathf.Max(xMin - px, px - xMax);
                    }

                    if (dOuter > 0.5f)
                    {
                        pixels[y * width + x] = transparent;
                        continue;
                    }

                    float outerAlphaMult = Mathf.Clamp01(0.5f - dOuter);

                    // 2. İç sınır SDF (dInner < 0 tüpün iç boşluğu)
                    float dInner;
                    if (py < yMinIn + rInner)
                    {
                        if (px < xMinIn + rInner)
                        {
                            float dx = px - (xMinIn + rInner);
                            float dy = py - (yMinIn + rInner);
                            dInner = Mathf.Sqrt(dx * dx + dy * dy) - rInner;
                        }
                        else if (px > xMaxIn - rInner)
                        {
                            float dx = px - (xMaxIn - rInner);
                            float dy = py - (yMinIn + rInner);
                            dInner = Mathf.Sqrt(dx * dx + dy * dy) - rInner;
                        }
                        else
                        {
                            dInner = yMinIn - py;
                        }
                    }
                    else
                    {
                        dInner = Mathf.Max(xMinIn - px, px - xMaxIn);
                    }

                    if (dInner <= 0f)
                    {
                        // Tüp içi tamamen saydam; yalnızca dış çerçeve çizilir.
                        pixels[y * width + x] = transparent;
                    }
                    else
                    {
                        // Duvar konturu
                        byte a = (byte)Mathf.RoundToInt(wallColor.a * outerAlphaMult);
                        pixels[y * width + x] = new Color32(wallColor.r, wallColor.g, wallColor.b, a);
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _tubeSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 100f);
            return _tubeSprite;
        }

        /// <summary>
        /// Gri tamir/kesim matı hissi için tekrar eden ince ızgaralı arka plan sprite'ı (128x128 px).
        /// </summary>
        public static Sprite GetBackgroundSprite()
        {
            if (_backgroundSprite != null) return _backgroundSprite;

            int width = 128;
            int height = 128;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color32[] pixels = new Color32[width * height];

            Color32 matColor = new Color32(63, 67, 73, 255);
            Color32 minorLineColor = new Color32(82, 87, 94, 255);
            Color32 majorLineColor = new Color32(112, 117, 124, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool majorLine = x % 64 == 0 || y % 64 == 0;
                    bool minorLine = x % 16 == 0 || y % 16 == 0;
                    pixels[y * width + x] = majorLine ? majorLineColor : (minorLine ? minorLineColor : matColor);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _backgroundSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _backgroundSprite;
        }
    }
}
