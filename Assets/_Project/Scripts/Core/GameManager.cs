using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Ana oyun döngüsünü yönetir. Sahnedeki verileri okuyup LevelController'ı ayağa kaldırır.
    /// Faz 2'de manuel ScriptableObject bağlantısıyla çalışır.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Menu,
            Playing,
            LevelComplete
        }

        private GameState _currentState = GameState.Menu;
        public GameState CurrentState => _currentState;

        [Header("Dependencies")]
        [SerializeField] private InputController inputController;
        [SerializeField] private ColorDefinition colorDef;

        private LevelManager _levelManager;
        private LevelController _currentLevel;

        private void Start()
        {
            if (inputController == null)
                inputController = GetComponent<InputController>();

            if (colorDef == null)
            {
                Debug.LogError("<b>GameManager:</b> ColorDefinition atanmamış! Lütfen Inspector'dan bağlayın.");
                return;
            }

            // LevelManager'ı kur ve başlat
            _levelManager = gameObject.AddComponent<LevelManager>();
            _levelManager.Initialize();

            StartLevel();
        }

        public void StartLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }

            var levelData = _levelManager.GetCurrentLevel();
            if (levelData == null)
            {
                Debug.LogError("<b>GameManager:</b> Yüklenecek seviye bulunamadı! Lütfen Tools > Color Sort > Generate Levels ile seviye üretin.");
                return;
            }

            SetState(GameState.Playing);

            GameObject levelGO = new GameObject("LevelController");
            _currentLevel = levelGO.AddComponent<LevelController>();
            
            // Kazanma olayına abone ol
            _currentLevel.OnLevelCompleted += HandleLevelCompleted;
            
            _currentLevel.Setup(levelData, colorDef, inputController);
        }

        private void HandleLevelCompleted()
        {
            if (_currentLevel != null)
                _currentLevel.OnLevelCompleted -= HandleLevelCompleted;
                
            SetState(GameState.LevelComplete);
            
            _levelManager.CompleteCurrentLevel();
            
            // UI olmadığı için şimdilik otomatik olarak sonraki seviyeye geç
            StartLevel();
        }

        public void SetState(GameState newState)
        {
            _currentState = newState;
        }
    }
}
