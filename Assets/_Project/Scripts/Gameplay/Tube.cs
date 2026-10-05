using System.Collections.Generic;
using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Görsel Tüp sınıfı. İçindeki topları barındırır, hizalamayı ve raycast için Collider'ı sağlar.
    /// </summary>
    public class Tube : MonoBehaviour
    {
        public int TubeIndex { get; private set; }
        
        private readonly List<Ball> _balls = new List<Ball>();
        private SpriteRenderer _sr;
        private BoxCollider2D _col;

        public void Setup(int tubeIndex)
        {
            TubeIndex = tubeIndex;
            
            if (_sr == null)
            {
                _sr = gameObject.AddComponent<SpriteRenderer>();
                _sr.sprite = SpriteFactory.GetTubeSprite();
                _sr.sortingOrder = 0;
            }
            
            if (_col == null)
            {
                _col = gameObject.AddComponent<BoxCollider2D>();
                _col.size = _sr.sprite.bounds.size;
                // Pivot (0.5, 0) olduğu için collider'ın merkezi (0, yükseklik/2) olmalı
                _col.offset = new Vector2(0, _col.size.y / 2f); 
            }
        }

        /// <summary>
        /// Animasyonsuz anında top ekler (Seviye başlangıcı için).
        /// </summary>
        public void AddBallInstant(Ball ball)
        {
            _balls.Add(ball);
            ball.transform.position = GetSlotPosition(_balls.Count - 1);
        }

        /// <summary>
        /// Animasyonlu şekilde top ekler (Hamle yapıldığında).
        /// </summary>
        public void AddBallAnimated(Ball ball, System.Action onComplete)
        {
            int targetIndex = _balls.Count;
            _balls.Add(ball);
            ball.MoveTo(GetSlotPosition(targetIndex), onComplete);
        }

        public Ball RemoveTopBall()
        {
            if (_balls.Count == 0) return null;
            var ball = _balls[_balls.Count - 1];
            _balls.RemoveAt(_balls.Count - 1);
            return ball;
        }

        public Ball GetTopBall()
        {
            if (_balls.Count == 0) return null;
            return _balls[_balls.Count - 1];
        }

        /// <summary>
        /// İçindeki x. topun (alttan üste) dünya koordinatlarındaki yerini hesaplar.
        /// </summary>
        public Vector3 GetSlotPosition(int index)
        {
            // Tube pivot (0,0) alt orta nokta.
            // Alt duvar 0.1 birim (10px). Top yarıçapı 0.5 birim (50px).
            // Merkez Y konumu = 0.1 + 0.5 + index * 1.0 (her top 1.0 birim çapında)
            float localY = 0.6f + index * 1.0f;
            return transform.position + new Vector3(0, localY, 0);
        }

        /// <summary>
        /// Tüp seçildiğinde en üstteki topu hafif havaya kaldırır.
        /// </summary>
        public void SelectAnimate()
        {
            var top = GetTopBall();
            if (top != null)
            {
                top.transform.position = GetSlotPosition(_balls.Count - 1) + new Vector3(0, 0.5f, 0);
            }
        }

        /// <summary>
        /// Tüp seçimi bırakıldığında topu eski yerine indirir.
        /// </summary>
        public void DeselectAnimate()
        {
            var top = GetTopBall();
            if (top != null)
            {
                top.transform.position = GetSlotPosition(_balls.Count - 1);
            }
        }
    }
}
