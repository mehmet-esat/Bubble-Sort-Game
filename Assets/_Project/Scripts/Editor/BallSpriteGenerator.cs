using System.IO;
using UnityEditor;
using UnityEngine;

namespace ColorSortPuzzle.Editor
{
    /// <summary>
    /// 512x512 şeffaf arka planlı, prosedürel Ball sprite'larını PNG olarak üreten ve
    /// import ayarlarını (Sprite, Bilinear, None compression, MipMap kapalı, PPU 512)
    /// otomatik yapılandıran Editor script'i.
    /// Referans görsele (ball_ref.png) birebir uygun katmanları dışa aktarır.
    /// </summary>
    public static class BallSpriteGenerator
    {
        private const string OutputFolder = "Assets/Sprites";

        [MenuItem("Tools/Color Sort/Generate Ball Sprites")]
        public static void GenerateAll()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            SpriteFactory.ClearCache();

            // 1. KATMAN 1 - Taban (Nötr gri gradyan, SpriteRenderer.color ile renklendirilir)
            Sprite baseSprite = SpriteFactory.GetBallBaseSprite();
            SaveSpriteToPNG(baseSprite, Path.Combine(OutputFolder, "Ball.png"));

            // 2. KATMAN 2 - Kenar Çizgisi (r = 0.95 - 1.0 siyah halka, alpha %35)
            Sprite edgeSprite = SpriteFactory.GetBallEdgeSprite();
            SaveSpriteToPNG(edgeSprite, Path.Combine(OutputFolder, "Ball_Edge.png"));

            // 3. KATMAN 3 - Alt Rim Işığı (235°-305° beyaz yay, alpha %35)
            Sprite rimSprite = SpriteFactory.GetBallRimSprite();
            SaveSpriteToPNG(rimSprite, Path.Combine(OutputFolder, "Ball_Rim.png"));

            // 4. KATMAN 5 - Hafif İç Parıltı (yarıçap 0.5 beyaz radyal gradyan, alpha %12)
            Sprite glowSprite = SpriteFactory.GetBallGlowSprite();
            SaveSpriteToPNG(glowSprite, Path.Combine(OutputFolder, "Ball_Glow.png"));

            // 5. KATMAN 4 - Ana Parlama (Hilal şerit %95 + küçük yuvarlak parlama %80)
            Sprite highlightSprite = SpriteFactory.GetBallHighlightSprite();
            SaveSpriteToPNG(highlightSprite, Path.Combine(OutputFolder, "Ball_Highlight.png"));

            // 6. Karşılaştırma ve İnceleme için Kompozit Önizleme (Referans renkle birleştirilmiş tek top)
            GenerateCompositePreview(Path.Combine(OutputFolder, "Ball_Composite_Preview.png"));

            // 7. Tüm renklerin aynı parlama pozisyonuna sahip olduğunu doğrulayan toplu önizleme
            GenerateAllColorsPreview(Path.Combine(OutputFolder, "Ball_AllColors_Preview.png"));

            // 8. Image 2 referanslı Tüp sprite'ını dışa aktar
            Sprite tubeSprite = SpriteFactory.GetTubeSprite();
            SaveSpriteToPNG(tubeSprite, Path.Combine(OutputFolder, "Tube.png"));

            AssetDatabase.Refresh();

            // Import ayarlarını doğrula ve uygula
            ConfigureImporter(Path.Combine(OutputFolder, "Ball.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_Edge.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_Rim.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_Glow.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_Highlight.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Tube.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Background.png"));
            ConfigureImporter("Assets/_Project/Resources/background.png");
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_Composite_Preview.png"));
            ConfigureImporter(Path.Combine(OutputFolder, "Ball_AllColors_Preview.png"));

            // 8. Yeni Ball assetleri import ayarlarını yapılandır
            string ballsDir = "Assets/_Project/Resources/Balls";
            if (Directory.Exists(ballsDir))
            {
                string[] ballFiles = Directory.GetFiles(ballsDir, "*.png");
                foreach (string ballPath in ballFiles)
                {
                    ConfigureImporter(ballPath);
                }
            }

            // 9. ColorDefinition.asset içine ball sprite referanslarını bağla
            WireBallSpritesToColorDefinition();

            // 10. Yeni assetlerin 4x2 önizlemesini kaydet
            GenerateNewAssetsPreview(Path.Combine(OutputFolder, "Ball_NewAssets_Preview.png"));

            AssetDatabase.SaveAssets();

            Debug.Log("<color=green><b>[BallSpriteGenerator]</b> Tüm top sprite'ları başarıyla üretildi ve Assets/Sprites/ klasörüne kaydedildi!</color>");
        }

        private static void SaveSpriteToPNG(Sprite sprite, string filePath)
        {
            if (sprite == null || sprite.texture == null)
            {
                Debug.LogError($"[BallSpriteGenerator] Sprite veya texture null: {filePath}");
                return;
            }

            byte[] bytes = sprite.texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        private static void ConfigureImporter(string assetPath)
        {
            string unityPath = assetPath.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[BallSpriteGenerator] TextureImporter bulunamadı: {unityPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 512f;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 4;
            importer.alphaIsTransparency = true;

            // Standalone ve Android platformlarında da Compression: None (Uncompressed RGBA32) garanti edilir
            TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
            defaultSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(defaultSettings);

            TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            standaloneSettings.overridden = true;
            standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
            standaloneSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(standaloneSettings);

            importer.SaveAndReimport();
        }

        /// <summary>
        /// Referans görseldeki gibi (örneğin turuncu renkle) tüm katmanları piksel piksel harmanlayarak
        /// yan yana karşılaştırma yapabileceğimiz tam bir kompozit PNG üretir.
        /// </summary>
        private static void GenerateCompositePreview(string filePath)
        {
            const int size = SpriteFactory.BallTextureSize;
            Texture2D composite = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color32[] basePixels = SpriteFactory.GetBallBaseSprite().texture.GetPixels32();
            Color32[] edgePixels = SpriteFactory.GetBallEdgeSprite().texture.GetPixels32();
            Color32[] rimPixels = SpriteFactory.GetBallRimSprite().texture.GetPixels32();
            Color32[] glowPixels = SpriteFactory.GetBallGlowSprite().texture.GetPixels32();
            Color32[] hlPixels = SpriteFactory.GetBallHighlightSprite().texture.GetPixels32();

            Color32[] result = new Color32[size * size];

            // Referans görseldeki turuncu renk (#F58220 civarı parlak turuncu)
            Color sampleTint = new Color(0.96f, 0.51f, 0.12f, 1.0f);

            for (int i = 0; i < size * size; i++)
            {
                Color32 b = basePixels[i];
                if (b.a == 0)
                {
                    result[i] = new Color32(0, 0, 0, 0);
                    continue;
                }

                // Katman 1: Taban * Tint (Grayscale * Tint)
                float baseGray = b.r / 255f;
                float r = baseGray * sampleTint.r;
                float g = baseGray * sampleTint.g;
                float bl = baseGray * sampleTint.b;
                float a = b.a / 255f;

                // Katman 2: Kenar (Siyah alpha blending)
                float edgeA = (edgePixels[i].a / 255f);
                r = r * (1f - edgeA);
                g = g * (1f - edgeA);
                bl = bl * (1f - edgeA);

                // Katman 3: Alt Rim (Beyaz additive / alpha blend)
                float rimA = rimPixels[i].a / 255f;
                r = Mathf.Lerp(r, 1f, rimA);
                g = Mathf.Lerp(g, 1f, rimA);
                bl = Mathf.Lerp(bl, 1f, rimA);

                // Katman 5: İç parıltı (Beyaz alpha blend)
                float glowA = glowPixels[i].a / 255f;
                r = Mathf.Lerp(r, 1f, glowA);
                g = Mathf.Lerp(g, 1f, glowA);
                bl = Mathf.Lerp(bl, 1f, glowA);

                // Katman 4: Ana Parlama Hilal + Nokta (Beyaz alpha blend)
                float hlA = hlPixels[i].a / 255f;
                r = Mathf.Lerp(r, 1f, hlA);
                g = Mathf.Lerp(g, 1f, hlA);
                bl = Mathf.Lerp(bl, 1f, hlA);

                result[i] = new Color32(
                    (byte)Mathf.RoundToInt(Mathf.Clamp01(r) * 255f),
                    (byte)Mathf.RoundToInt(Mathf.Clamp01(g) * 255f),
                    (byte)Mathf.RoundToInt(Mathf.Clamp01(bl) * 255f),
                    (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f)
                );
            }

            composite.SetPixels32(result);
            composite.Apply();

            byte[] bytes = composite.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// Projedeki tüm 8 rengin aynı parlama pozisyonuna, aynı hilal/dot efektine sahip olduğunu
        /// yan yana doğrulayan 4x2 ızgaralı önizleme görseli üretir.
        /// </summary>
        private static void GenerateAllColorsPreview(string filePath)
        {
            Color[] colors = new Color[]
            {
                new Color(0.976f, 0.145f, 0.220f, 1f), // Red
                new Color(1.000f, 0.541f, 0.000f, 1f), // Orange
                new Color(1.000f, 0.847f, 0.000f, 1f), // Yellow
                new Color(0.129f, 0.847f, 0.286f, 1f), // Green
                new Color(0.000f, 0.851f, 0.784f, 1f), // Cyan
                new Color(0.082f, 0.439f, 1.000f, 1f), // Blue
                new Color(0.486f, 0.133f, 0.910f, 1f), // Purple
                new Color(0.980f, 0.227f, 0.682f, 1f)  // Pink
            };

            const int ballSize = SpriteFactory.BallTextureSize; // 512
            const int cols = 4;
            const int rows = 2;
            int totalWidth = cols * ballSize;
            int totalHeight = rows * ballSize;

            Texture2D gridTex = new Texture2D(totalWidth, totalHeight, TextureFormat.RGBA32, false);
            Color32[] gridPixels = new Color32[totalWidth * totalHeight];
            Color32 bg = new Color32(32, 34, 38, 255); // Koyu arka plan
            for (int i = 0; i < gridPixels.Length; i++) gridPixels[i] = bg;

            Color32[] basePixels = SpriteFactory.GetBallBaseSprite().texture.GetPixels32();
            Color32[] edgePixels = SpriteFactory.GetBallEdgeSprite().texture.GetPixels32();
            Color32[] rimPixels = SpriteFactory.GetBallRimSprite().texture.GetPixels32();
            Color32[] glowPixels = SpriteFactory.GetBallGlowSprite().texture.GetPixels32();
            Color32[] hlPixels = SpriteFactory.GetBallHighlightSprite().texture.GetPixels32();

            for (int c = 0; c < colors.Length; c++)
            {
                int col = c % cols;
                int row = 1 - (c / cols); // 0 alt, 1 üst satır
                int startX = col * ballSize;
                int startY = row * ballSize;

                Color tint = colors[c];

                for (int y = 0; y < ballSize; y++)
                {
                    for (int x = 0; x < ballSize; x++)
                    {
                        int srcIdx = y * ballSize + x;
                        Color32 b = basePixels[srcIdx];
                        if (b.a == 0) continue;

                        float baseGray = b.r / 255f;
                        float r = baseGray * tint.r;
                        float g = baseGray * tint.g;
                        float bl = baseGray * tint.b;
                        float a = b.a / 255f;

                        // Katman 2: Kenar
                        float edgeA = edgePixels[srcIdx].a / 255f;
                        r *= (1f - edgeA);
                        g *= (1f - edgeA);
                        bl *= (1f - edgeA);

                        // Katman 3: Alt Rim
                        float rimA = rimPixels[srcIdx].a / 255f;
                        r = Mathf.Lerp(r, 1f, rimA);
                        g = Mathf.Lerp(g, 1f, rimA);
                        bl = Mathf.Lerp(bl, 1f, rimA);

                        // Katman 5: İç parıltı
                        float glowA = glowPixels[srcIdx].a / 255f;
                        r = Mathf.Lerp(r, 1f, glowA);
                        g = Mathf.Lerp(g, 1f, glowA);
                        bl = Mathf.Lerp(bl, 1f, glowA);

                        // Katman 4: Ana Parlama Hilal + Nokta
                        float hlA = hlPixels[srcIdx].a / 255f;
                        r = Mathf.Lerp(r, 1f, hlA);
                        g = Mathf.Lerp(g, 1f, hlA);
                        bl = Mathf.Lerp(bl, 1f, hlA);

                        int dstX = startX + x;
                        int dstY = startY + y;
                        int dstIdx = dstY * totalWidth + dstX;

                        // Arka plan üzerine harmanla
                        Color32 bgPx = gridPixels[dstIdx];
                        byte finalR = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.r, Mathf.Clamp01(r) * 255f, a));
                        byte finalG = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.g, Mathf.Clamp01(g) * 255f, a));
                        byte finalB = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.b, Mathf.Clamp01(bl) * 255f, a));

                        gridPixels[dstIdx] = new Color32(finalR, finalG, finalB, 255);
                    }
                }
            }

            gridTex.SetPixels32(gridPixels);
            gridTex.Apply();

            byte[] bytes = gridTex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        private static void WireBallSpritesToColorDefinition()
        {
            ColorDefinition colorDef = AssetDatabase.LoadAssetAtPath<ColorDefinition>("Assets/_Project/Scripts/Data/ColorDefinition.asset");
            if (colorDef == null) return;

            SerializedObject so = new SerializedObject(colorDef);
            SerializedProperty colorsProp = so.FindProperty("colors");
            if (colorsProp != null && colorsProp.isArray)
            {
                for (int i = 0; i < colorsProp.arraySize; i++)
                {
                    SerializedProperty entry = colorsProp.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = entry.FindPropertyRelative("name");
                    SerializedProperty spriteProp = entry.FindPropertyRelative("ballSprite");
                    if (nameProp != null && spriteProp != null)
                    {
                        string colorName = nameProp.stringValue.ToLowerInvariant().Trim();
                        string spritePath = $"Assets/_Project/Resources/Balls/ball_{colorName}.png";
                        Sprite ballSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (ballSprite != null)
                        {
                            spriteProp.objectReferenceValue = ballSprite;
                        }
                    }
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(colorDef);
            }
        }

        private static void GenerateNewAssetsPreview(string filePath)
        {
            string[] colorNames = new string[] { "red", "orange", "yellow", "green", "cyan", "blue", "purple", "pink" };
            const int ballSize = 512;
            const int cols = 4;
            const int rows = 2;
            int totalWidth = cols * ballSize;
            int totalHeight = rows * ballSize;

            Texture2D gridTex = new Texture2D(totalWidth, totalHeight, TextureFormat.RGBA32, false);
            Color32[] gridPixels = new Color32[totalWidth * totalHeight];
            Color32 bg = new Color32(32, 34, 38, 255);
            for (int i = 0; i < gridPixels.Length; i++) gridPixels[i] = bg;

            for (int c = 0; c < colorNames.Length; c++)
            {
                int col = c % cols;
                int row = 1 - (c / cols);
                int startX = col * ballSize;
                int startY = row * ballSize;

                string assetPath = $"Assets/_Project/Resources/Balls/ball_{colorNames[c]}.png";
                if (!File.Exists(assetPath)) continue;

                byte[] fileData = File.ReadAllBytes(assetPath);
                Texture2D ballTex = new Texture2D(ballSize, ballSize, TextureFormat.RGBA32, false);
                ballTex.LoadImage(fileData);
                Color32[] ballPixels = ballTex.GetPixels32();

                for (int y = 0; y < ballSize; y++)
                {
                    for (int x = 0; x < ballSize; x++)
                    {
                        Color32 src = ballPixels[y * ballSize + x];
                        if (src.a == 0) continue;

                        int dstX = startX + x;
                        int dstY = startY + y;
                        int dstIdx = dstY * totalWidth + dstX;

                        Color32 bgPx = gridPixels[dstIdx];
                        float a = src.a / 255f;
                        byte finalR = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.r, src.r, a));
                        byte finalG = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.g, src.g, a));
                        byte finalB = (byte)Mathf.RoundToInt(Mathf.Lerp(bgPx.b, src.b, a));

                        gridPixels[dstIdx] = new Color32(finalR, finalG, finalB, 255);
                    }
                }
            }

            gridTex.SetPixels32(gridPixels);
            gridTex.Apply();

            byte[] bytes = gridTex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }
    }
}
