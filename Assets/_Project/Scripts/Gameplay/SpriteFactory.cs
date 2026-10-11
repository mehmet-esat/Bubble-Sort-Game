using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Oyun için gerekli tüm prosedürel sprite'ları (top katmanları, tüp ve arka plan) üreten ve önbelleğe alan fabrika.
    /// Referans görsele (ball_ref.png) birebir uygun 512x512 piksel tabanlı 3D cam küre efekti sağlar.
    /// </summary>
    public static class SpriteFactory
    {
        public const int BallTextureSize = 512;
        public const float BallPixelRadius = 248.0f;
        public const float BallPPU = 512.0f;

        private static Sprite _ballBaseSprite;
        private static Sprite _ballEdgeSprite;
        private static Sprite _ballRimSprite;
        private static Sprite _ballGlowSprite;
        private static Sprite _ballHighlightSprite;
        private static Sprite _tubeSprite;
        private static Sprite _backgroundSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearCache()
        {
            _ballBaseSprite = null;
            _ballEdgeSprite = null;
            _ballRimSprite = null;
            _ballGlowSprite = null;
            _ballHighlightSprite = null;
            _tubeSprite = null;
            _backgroundSprite = null;
        }

        /// <summary>
        /// GLSL / Shading tarzı Hermite smoothstep fonksiyonu:
        /// x değerini [edge0, edge1] aralığından [0, 1] aralığına yumuşak bir S-eğrisiyle eşler.
        /// </summary>
        public static float GlslSmoothStep(float edge0, float edge1, float x)
        {
            if (edge1 <= edge0) return x >= edge0 ? 1f : 0f;
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// KATMAN 1 - Taban Sprite:
        /// 512x512 nötr gri gradyanlı daire. SpriteRenderer.color (tint) ile çarpılır.
        /// Işık merkezi (-0.30, +0.35) noktasında en açık, kenarlarda 0.70'e, sağ-alt (+0.6, -0.6) yönünde 0.50'ye kadar iner.
        /// </summary>
        public static Sprite GetBallBaseSprite()
        {
            if (_ballBaseSprite != null) return _ballBaseSprite;

            const int size = BallTextureSize;
            const float R = BallPixelRadius;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);

            Vector2 lightCenter = new Vector2(-0.30f, 0.35f);
            Vector2 darkDir = new Vector2(0.6f, -0.6f).normalized;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / R;
                    float ny = (y + 0.5f - size * 0.5f) / R;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);

                    if (r > 1.0f + 1.5f / R)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    // Anti-aliasing (1-2 px kenar yumuşatma)
                    float aa = Mathf.Clamp01((1.0f - r) * R + 0.5f);
                    if (aa <= 0.001f)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    Vector2 pos = new Vector2(nx, ny);
                    float distLight = Vector2.Distance(pos, lightCenter);

                    // Işık merkezinde parlaklık 1.15, kenarlara doğru 0.70'e düşer
                    float radialT = GlslSmoothStep(0f, 1.40f, distLight);
                    float baseBrightness = Mathf.Lerp(1.15f, 0.70f, radialT);

                    // Sağ-alt yönünde ek koyulaşma (0.50'ye kadar)
                    float darkDot = Mathf.Clamp01(Vector2.Dot(pos, darkDir));
                    float brightness = baseBrightness - darkDot * 0.20f;
                    brightness = Mathf.Clamp(brightness, 0.50f, 1.15f);

                    // Tint ile çarpılacak gri ton (1.15 değeri saf beyaz = 255'e normalize edilir)
                    float normGray = Mathf.Clamp01(brightness / 1.15f);
                    byte gray = (byte)Mathf.RoundToInt(normGray * 255f);
                    byte alpha = (byte)Mathf.RoundToInt(aa * 255f);

                    pixels[y * size + x] = new Color32(gray, gray, gray, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballBaseSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), BallPPU);
            return _ballBaseSprite;
        }

        /// <summary>
        /// KATMAN 2 - Kenar Çizgisi Sprite:
        /// r = 0.95 ile 1.0 arasında yumuşak kenarlı, siyah halka (alpha %35).
        /// </summary>
        public static Sprite GetBallEdgeSprite()
        {
            if (_ballEdgeSprite != null) return _ballEdgeSprite;

            const int size = BallTextureSize;
            const float R = BallPixelRadius;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / R;
                    float ny = (y + 0.5f - size * 0.5f) / R;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);

                    if (r < 0.93f || r > 1.0f + 1.5f / R)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    float innerFade = GlslSmoothStep(0.93f, 0.965f, r);
                    float outerFade = Mathf.Clamp01((1.0f - r) * R + 0.5f);
                    float ringAlpha = innerFade * outerFade * 0.35f;

                    byte a = (byte)Mathf.RoundToInt(ringAlpha * 255f);
                    pixels[y * size + x] = new Color32(0, 0, 0, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballEdgeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), BallPPU);
            return _ballEdgeSprite;
        }

        /// <summary>
        /// KATMAN 3 - Alt Rim Işığı Sprite:
        /// r = 0.80 ile 0.93 arası, 235° ile 305° alt yayı. Beyaz, alpha %35, uçlara doğru yumuşak fade.
        /// </summary>
        public static Sprite GetBallRimSprite()
        {
            if (_ballRimSprite != null) return _ballRimSprite;

            const int size = BallTextureSize;
            const float R = BallPixelRadius;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / R;
                    float ny = (y + 0.5f - size * 0.5f) / R;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);

                    if (r < 0.78f || r > 0.95f)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    float angle = Mathf.Atan2(ny, nx) * Mathf.Rad2Deg;
                    if (angle < 0f) angle += 360f;

                    if (angle < 235f || angle > 305f)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    // Açıya göre fade (270° alt merkezde maksimum, 235° ve 305°'e doğru 0)
                    float angleDiff = Mathf.Abs(angle - 270f);
                    float angleFade = 1f - GlslSmoothStep(0f, 35f, angleDiff);

                    // Yarıçapa göre yumuşak kenar (0.865f merkezli ince bant)
                    float rDist = Mathf.Abs(r - 0.865f);
                    float rFade = 1f - GlslSmoothStep(0.015f, 0.065f, rDist);

                    float rimAlpha = angleFade * rFade * 0.35f;
                    byte a = (byte)Mathf.RoundToInt(rimAlpha * 255f);

                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballRimSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), BallPPU);
            return _ballRimSprite;
        }

        /// <summary>
        /// KATMAN 5 - Hafif İç Parıltı Sprite:
        /// Merkez (-0.25, +0.30) etrafında yarıçap 0.5'lik beyaz radial gradient, alpha %12 -> 0.
        /// </summary>
        public static Sprite GetBallGlowSprite()
        {
            if (_ballGlowSprite != null) return _ballGlowSprite;

            const int size = BallTextureSize;
            const float R = BallPixelRadius;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);
            Vector2 glowCenter = new Vector2(-0.25f, 0.30f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / R;
                    float ny = (y + 0.5f - size * 0.5f) / R;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);

                    if (r > 1.0f + 1f / R)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    Vector2 pos = new Vector2(nx, ny);
                    float dist = Vector2.Distance(pos, glowCenter);

                    if (dist > 0.50f)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    float fade = 1f - GlslSmoothStep(0f, 0.50f, dist);
                    float edgeClip = Mathf.Clamp01((1.0f - r) * R + 0.5f);
                    float glowAlpha = fade * 0.12f * edgeClip;

                    byte a = (byte)Mathf.RoundToInt(glowAlpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), BallPPU);
            return _ballGlowSprite;
        }

        /// <summary>
        /// KATMAN 4 - Ana Parlama Sprite:
        /// 1) Hilal (kavisli şerit): r = 0.58-0.78, açı 115°-170°, ortası ~0.14 kalın, uçları sivri, alpha %95 -> 0.
        /// 2) Küçük yuvarlak parlama: merkez (-0.30, +0.28), yarıçap 0.07, alpha %80, yumuşak kenar.
        /// Her ikisi de beyazdır ve renkten etkilenmez.
        /// </summary>
        public static Sprite GetBallHighlightSprite()
        {
            if (_ballHighlightSprite != null) return _ballHighlightSprite;

            const int size = BallTextureSize;
            const float R = BallPixelRadius;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);

            const float arcCenterAngle = 142.5f;
            const float arcHalfSpan = 27.5f;
            const float arcCenterRadius = 0.68f;
            const float featherInUnits = 2.5f / R; // ~2.5 px feathering

            Vector2 dotCenter = new Vector2(-0.30f, 0.28f);
            const float dotRadius = 0.07f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / R;
                    float ny = (y + 0.5f - size * 0.5f) / R;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);

                    Vector2 pos = new Vector2(nx, ny);

                    // 1) Hilal parlama
                    float crescentAlpha = 0f;
                    float angle = Mathf.Atan2(ny, nx) * Mathf.Rad2Deg;
                    if (angle < 0f) angle += 360f;

                    if (angle >= 115f && angle <= 170f && r >= 0.55f && r <= 0.82f)
                    {
                        float angleDiff = Mathf.Abs(angle - arcCenterAngle);
                        float u = angleDiff / arcHalfSpan; // 0 (merkez) ile 1 (uçlar) arası
                        float taper = Mathf.Clamp01(1f - u * u); // Parabolik sivrilme
                        float halfThickness = 0.07f * taper;     // Ortada 0.14 tam kalınlık, uçta 0

                        if (halfThickness > 0.001f)
                        {
                            float dr = Mathf.Abs(r - arcCenterRadius);
                            float radFade = 1f - GlslSmoothStep(halfThickness - featherInUnits, halfThickness + featherInUnits, dr);
                            float tipFade = GlslSmoothStep(0f, 0.25f, taper);
                            crescentAlpha = radFade * tipFade * 0.95f;
                        }
                    }

                    // 2) Küçük yuvarlak parlama
                    float dotDist = Vector2.Distance(pos, dotCenter);
                    float dotAlpha = 0f;
                    if (dotDist <= dotRadius)
                    {
                        float dotFade = 1f - GlslSmoothStep(0.02f, dotRadius, dotDist);
                        dotAlpha = dotFade * 0.80f;
                    }

                    float finalAlpha = Mathf.Max(crescentAlpha, dotAlpha);
                    if (finalAlpha <= 0.001f)
                    {
                        pixels[y * size + x] = transparent;
                        continue;
                    }

                    byte a = (byte)Mathf.RoundToInt(finalAlpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            _ballHighlightSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), BallPPU);
            return _ballHighlightSprite;
        }

        /// <summary>
        /// Image 2 (ball_ref.png) referansına birebir uygun cam tüp sprite'ı (120x430 px).
        /// Alt tarafı tam U-şekli yarım daire kavis (radius 59px), üst köşeleri hafif yuvarlak (radius 14px).
        /// İçi tamamen şeffaftır, dış kontur rengi gümüşi cam (#969F95, alpha %90).
        /// </summary>
        public static Sprite GetTubeSprite()
        {
            if (_tubeSprite != null) return _tubeSprite;

            int width = 120;
            int height = 430;
            float wallThickness = 5.0f;
            float rBottomOuter = (width - 2f) * 0.5f; // 59f (tam yarıçaplı U-şekli alt kavis)
            float rBottomInner = rBottomOuter - wallThickness; // 54f
            float rTopOuter = 14f; // Üst köşeler için yuvarlama yarıçapı
            float rTopInner = Mathf.Max(1f, rTopOuter - wallThickness);

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[width * height];
            Color32 transparent = new Color32(0, 0, 0, 0);

            // Gümüşi cam kontur rengi (Referans görseldeki #969F95 tonu)
            Color32 wallColor = new Color32(150, 159, 149, 230);

            float xMin = 1f;
            float xMax = width - 1f;
            float yMin = 1f;
            float yMax = height - 1f;

            float xMinIn = xMin + wallThickness;
            float xMaxIn = xMax - wallThickness;
            float yMinIn = yMin + wallThickness;
            float yMaxIn = yMax - wallThickness;

            float bottomCenterY = yMin + rBottomOuter;
            float topCenterY = yMax - rTopOuter;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // 1. DIŞ SINIR SDF (dOuter <= 0 tüpün içinde)
                    float dOuter;
                    if (py < bottomCenterY)
                    {
                        // Alt U-kavisi (tam yarım daire)
                        float centerX = (xMin + xMax) * 0.5f;
                        float dx = px - centerX;
                        float dy = py - bottomCenterY;
                        dOuter = Mathf.Sqrt(dx * dx + dy * dy) - rBottomOuter;
                    }
                    else if (py > topCenterY)
                    {
                        // Üst yuvarlatılmış köşeler
                        if (px < xMin + rTopOuter)
                        {
                            float dx = px - (xMin + rTopOuter);
                            float dy = py - topCenterY;
                            dOuter = Mathf.Sqrt(dx * dx + dy * dy) - rTopOuter;
                        }
                        else if (px > xMax - rTopOuter)
                        {
                            float dx = px - (xMax - rTopOuter);
                            float dy = py - topCenterY;
                            dOuter = Mathf.Sqrt(dx * dx + dy * dy) - rTopOuter;
                        }
                        else
                        {
                            dOuter = py - yMax;
                        }
                    }
                    else
                    {
                        // Düz dikey duvarlar
                        dOuter = Mathf.Max(xMin - px, px - xMax);
                    }

                    if (dOuter > 0.5f)
                    {
                        pixels[y * width + x] = transparent;
                        continue;
                    }

                    float outerAlpha = Mathf.Clamp01(0.5f - dOuter);

                    // 2. İÇ SINIR SDF (dInner < 0 tüpün iç boşluğunda)
                    float dInner;
                    if (py < bottomCenterY)
                    {
                        float centerX = (xMin + xMax) * 0.5f;
                        float dx = px - centerX;
                        float dy = py - bottomCenterY;
                        dInner = Mathf.Sqrt(dx * dx + dy * dy) - rBottomInner;
                    }
                    else if (py > topCenterY)
                    {
                        float topCenterYIn = yMaxIn - rTopInner;
                        if (px < xMinIn + rTopInner)
                        {
                            float dx = px - (xMinIn + rTopInner);
                            float dy = py - topCenterYIn;
                            dInner = Mathf.Sqrt(dx * dx + dy * dy) - rTopInner;
                        }
                        else if (px > xMaxIn - rTopInner)
                        {
                            float dx = px - (xMaxIn - rTopInner);
                            float dy = py - topCenterYIn;
                            dInner = Mathf.Sqrt(dx * dx + dy * dy) - rTopInner;
                        }
                        else
                        {
                            dInner = py - yMaxIn;
                        }
                    }
                    else
                    {
                        dInner = Mathf.Max(xMinIn - px, px - xMaxIn);
                    }

                    if (dInner <= -0.5f)
                    {
                        // Tüpün içi tamamen şeffaf
                        pixels[y * width + x] = transparent;
                    }
                    else
                    {
                        // Kontur çizgisi (iç ve dış kenar anti-aliasing)
                        float innerAlpha = Mathf.Clamp01(dInner + 0.5f);
                        float strokeAlpha = Mathf.Min(outerAlpha, innerAlpha);
                        byte a = (byte)Mathf.RoundToInt(wallColor.a * strokeAlpha);
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
        /// Yıldızlı uzay / gece gökyüzü arka plan sprite'ı.
        /// Resources/background.png varsa onu yükler, yoksa prosedürel üretir.
        /// </summary>
        public static Sprite GetBackgroundSprite()
        {
            if (_backgroundSprite != null) return _backgroundSprite;

            Sprite loaded = Resources.Load<Sprite>("background");
            if (loaded != null)
            {
                _backgroundSprite = loaded;
                return _backgroundSprite;
            }

            // Fallback: 128x128 koyu uzay mavisi gradyan
            int width = 128;
            int height = 128;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[width * height];
            Color32 topCol = new Color32(13, 7, 95, 255);
            Color32 botCol = new Color32(48, 44, 119, 255);
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color32 col = Color32.Lerp(botCol, topCol, t);
                for (int x = 0; x < width; x++) pixels[y * width + x] = col;
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            _backgroundSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            return _backgroundSprite;
        }
    }
}
