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
        private UIManager _uiManager;

        public bool CanRestart => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanRestart;
        public bool CanUndo => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanUndo;
        public bool CanAddTube => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanAddTube;

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

            // UIManager'ı kodla oluştur
            GameObject uiGO = new GameObject("UIManager");
            _uiManager = uiGO.AddComponent<UIManager>();
            _uiManager.Initialize(this);
            UpdateLevelDisplay();
        }

        public void StartLevel()
        {
            if (_currentLevel != null)
            {
                if (_currentLevel.IsAnimating)
                    return;

                _currentLevel.Cleanup();
                Destroy(_currentLevel.gameObject);
                _currentLevel = null;
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
            UpdateLevelDisplay();
        }

        public void TryRestart()
        {
            if (!CanRestart)
                return;

            StartLevel();
        }

        public void TryUndo()
        {
            if (!CanUndo)
                return;

            _currentLevel.TryUndo();
        }

        public void TryAddTube()
        {
            if (!CanAddTube)
                return;

            _currentLevel.TryAddTube();
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

        private void UpdateLevelDisplay()
        {
            if (_uiManager != null && _levelManager != null)
            {
                _uiManager.SetLevelNumber(_levelManager.GetCurrentDisplayLevelNumber());
            }
        }

        public void SetState(GameState newState)
        {
            _currentState = newState;
        }
    }
}
