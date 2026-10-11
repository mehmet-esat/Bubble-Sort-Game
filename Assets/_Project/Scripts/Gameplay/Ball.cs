using System.Collections;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Görsel Top sınıfı. Yalnızca görünüm ve basit hareket animasyonundan sorumludur.
    /// </summary>
    public class Ball : MonoBehaviour
    {
        private const float VisualScale = 0.75f;

        private SpriteRenderer _sr;
        private Coroutine _moveCoroutine;
        
        public int ColorId { get; private set; }

        public void Setup(int colorId, Color color, Sprite ballSprite = null)
        {
            ColorId = colorId;
            transform.localScale = Vector3.one * VisualScale;
            
            if (_sr == null)
            {
                _sr = gameObject.AddComponent<SpriteRenderer>();
                _sr.sortingOrder = 0; // Katman 1: Taban (0)
            }

            if (ballSprite != null)
            {
                // Kullanıcının sağladığı gerçek 3D top dokusu kullanılır (Tüm gölge/parlamalar içinde hazır)
                _sr.sprite = ballSprite;
                _sr.color = Color.white;
            }
            else
            {
                // Fallback: 5 katmanlı prosedürel küre
                _sr.sprite = SpriteFactory.GetBallBaseSprite();
                _sr.color = color;

                CreateLayer("Edge", SpriteFactory.GetBallEdgeSprite(), 1);        // Katman 2: Kenar çizgisi (1)
                CreateLayer("Rim", SpriteFactory.GetBallRimSprite(), 2);          // Katman 3: Alt rim ışığı (2)
                CreateLayer("Glow", SpriteFactory.GetBallGlowSprite(), 3);        // Katman 5: Hafif iç parıltı (3)
                CreateLayer("Highlight", SpriteFactory.GetBallHighlightSprite(), 4); // Katman 4: Ana hilal parlama + küçük nokta (4)
            }
        }

        private void CreateLayer(string layerName, Sprite sprite, int sortingOrder)
        {
            GameObject layerGO = new GameObject(layerName);
            layerGO.transform.SetParent(transform, false);

            SpriteRenderer layerRenderer = layerGO.AddComponent<SpriteRenderer>();
            layerRenderer.sprite = sprite;
            layerRenderer.color = Color.white;
            layerRenderer.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// Topu hedefe doğru doğrusal olarak hareket ettirir (Zıplama efekti dahil).
        /// </summary>
        public void MoveTo(Vector3 targetPos, System.Action onComplete)
        {
            if (_moveCoroutine != null) 
                StopCoroutine(_moveCoroutine);
                
            _moveCoroutine = StartCoroutine(MoveCoroutine(targetPos, onComplete));
        }

        private IEnumerator MoveCoroutine(Vector3 targetPos, System.Action onComplete)
        {
            float speed = 15f;
            Vector3 startPos = transform.position;
            
            // Ters U şeklinde hareket (Tüpten çık, yatay git, diğer tüpe in)
            // p1: Mevcut bulunduğu sütunda havaya kalk
            Vector3 p1 = new Vector3(startPos.x, Mathf.Max(startPos.y, targetPos.y) + 1.5f, startPos.z);
            // p2: Havada hedef sütunun üzerine gel
            Vector3 p2 = new Vector3(targetPos.x, p1.y, targetPos.z);
            
            yield return MoveToPos(p1, speed);
            yield return MoveToPos(p2, speed);
            yield return MoveToPos(targetPos, speed);

            onComplete?.Invoke();
        }

        private IEnumerator MoveToPos(Vector3 target, float speed)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }
    }
}
