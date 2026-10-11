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

        public void Setup(int colorId, Color color)
        {
            ColorId = colorId;
            transform.localScale = Vector3.one * VisualScale;
            
            if (_sr == null)
            {
                _sr = gameObject.AddComponent<SpriteRenderer>();
                _sr.sprite = SpriteFactory.GetBallSprite();
                _sr.sortingOrder = 1; // Tüpün önünde görünsün

                CreateLayer("Shadow", SpriteFactory.GetBallShadowSprite(), 2);
                CreateLayer("Highlight", SpriteFactory.GetBallHighlightSprite(), 3);
                CreateLayer("Accent", SpriteFactory.GetBallAccentSprite(), 4);
            }

            _sr.color = color;
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
