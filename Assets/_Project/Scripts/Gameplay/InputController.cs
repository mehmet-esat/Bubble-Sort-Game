using UnityEngine;
using UnityEngine.InputSystem;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Dokunma/tıklama girişini okur. Faz 1 iskeleti.
    /// Faz 2'de tüp seçimi ve LevelController bağlantısı eklenecek.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        private InputAction _tapAction;

        private void OnEnable()
        {
            _tapAction = new InputAction("Tap", InputActionType.Button,
                "<Pointer>/press");
            _tapAction.performed += OnTapPerformed;
            _tapAction.Enable();
        }

        private void OnDisable()
        {
            _tapAction.performed -= OnTapPerformed;
            _tapAction.Disable();
            _tapAction.Dispose();
        }

        private void OnTapPerformed(InputAction.CallbackContext context)
        {
            // Faz 2'de: dokunulan pozisyondan raycast ile tüp bulma
            // ve LevelController'a iletme eklenecek.
        }
    }
}
