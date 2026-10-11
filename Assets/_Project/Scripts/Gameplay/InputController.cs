using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();

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

            Vector2 screenPos = _positionAction.ReadValue<Vector2>();
            int pointerId = -1;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                pointerId = touchscreen.primaryTouch.touchId.ReadValue();
            }
            else if (Mouse.current != null)
            {
                pointerId = Mouse.current.deviceId;
            }

            if (IsPointerOverUI(pointerId, screenPos))
                return;

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

        private bool IsPointerOverUI(int pointerId, Vector2 screenPos)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            if (eventSystem.IsPointerOverGameObject(pointerId))
                return true;

            _uiRaycastResults.Clear();
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                pointerId = pointerId,
                position = screenPos
            };
            eventSystem.RaycastAll(pointerData, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
        }
    }
}
