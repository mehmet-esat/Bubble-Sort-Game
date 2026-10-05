using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Yeni Input System ile dokunma/tıklama girişini okur.
    /// Ekrana dokunulan pozisyondan 2D Raycast gönderip hedeflenen tüpü bulur.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        private InputAction _tapAction;
        private InputAction _positionAction;

        public event Action<int> OnTubeTapped;

        private void OnEnable()
        {
            _tapAction = new InputAction("Tap", InputActionType.Button, "<Pointer>/press");
            _positionAction = new InputAction("Position", InputActionType.Value, "<Pointer>/position");
            
            _tapAction.performed += OnTapPerformed;
            _tapAction.Enable();
            _positionAction.Enable();
        }

        private void OnDisable()
        {
            _tapAction.performed -= OnTapPerformed;
            _tapAction.Disable();
            _positionAction.Disable();
            _tapAction.Dispose();
            _positionAction.Dispose();
        }

        private void OnTapPerformed(InputAction.CallbackContext context)
        {
            if (Camera.main == null) return;

            // Tıklanılan ekran koordinatını al
            Vector2 screenPos = _positionAction.ReadValue<Vector2>();
            
            // Ekran koordinatını dünya koordinatına çevir
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            
            // 2D Raycast at (Sadece X,Y düzleminde)
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                // Çarpışan objede Tube var mı?
                var tube = hit.collider.GetComponent<Tube>();
                if (tube != null)
                {
                    OnTubeTapped?.Invoke(tube.TubeIndex);
                }
            }
        }
    }
}
