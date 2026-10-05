using UnityEngine;

namespace ColorSortPuzzle
{
    public static class SpriteFactory
    {
        private static Sprite _ballSprite;
        private static Sprite _tubeSprite;

        public static Sprite GetBallSprite()
        {
            if (_ballSprite != null) return _ballSprite;
            
            // İç genişlik 100px. %90'ı için yarıçap 45 yapıyoruz.
            int radius = 45; 
            int size = 100; // Doku boyutu 100x100 kalıyor, top ortada 90px olacak
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear; // Daha yumuşak kenarlar
            
            Color32[] pixels = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Merkeze olan uzaklık (50 merkez, çünkü doku 100x100)
                    float cx = x - 50f + 0.5f;
                    float cy = y - 50f + 0.5f;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);
                    
                    // Anti-aliasing (Yumuşak kenar)
                    if (dist <= radius - 1f) 
                    {
                        pixels[y * size + x] = new Color32(255, 255, 255, 255);
                    }
                    else if (dist <= radius) 
                    {
                        float alpha = radius - dist; // 0 ile 1 arası
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255 * alpha));
                    }
                    else 
                    {
                        pixels[y * size + x] = transparent;
                    }
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            
            _ballSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _ballSprite;
        }

        public static Sprite GetTubeSprite()
        {
            if (_tubeSprite != null) return _tubeSprite;
            
            int width = 120;
            int height = 430; 
            int wallThickness = 10;
            float cornerRadius = 10f; // Alt köşeleri yuvarlatmak için
            
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            
            Color32[] pixels = new Color32[width * height];
            Color32 transparent = new Color32(0, 0, 0, 0);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isWall = x < wallThickness || x >= width - wallThickness || y < wallThickness;
                    
                    if (isWall)
                    {
                        float alphaMult = 1f;
                        bool draw = true;
                        
                        // Sol alt köşe yuvarlatma
                        if (x < cornerRadius && y < cornerRadius)
                        {
                            float cx = cornerRadius - x - 0.5f;
                            float cy = cornerRadius - y - 0.5f;
                            float cdist = Mathf.Sqrt(cx * cx + cy * cy);
                            
                            if (cdist > cornerRadius) draw = false;
                            else if (cdist > cornerRadius - 1f) alphaMult = cornerRadius - cdist;
                        }
                        // Sağ alt köşe yuvarlatma
                        else if (x >= width - cornerRadius && y < cornerRadius)
                        {
                            float cx = x - (width - cornerRadius) + 0.5f;
                            float cy = cornerRadius - y - 0.5f;
                            float cdist = Mathf.Sqrt(cx * cx + cy * cy);
                            
                            if (cdist > cornerRadius) draw = false;
                            else if (cdist > cornerRadius - 1f) alphaMult = cornerRadius - cdist;
                        }
                        
                        if (draw)
                        {
                            pixels[y * width + x] = new Color32(200, 200, 200, (byte)(100 * alphaMult));
                        }
                        else
                        {
                            pixels[y * width + x] = transparent;
                        }
                    }
                    else
                    {
                        pixels[y * width + x] = transparent;
                    }
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            
            _tubeSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 100f);
            return _tubeSprite;
        }
    }
}
