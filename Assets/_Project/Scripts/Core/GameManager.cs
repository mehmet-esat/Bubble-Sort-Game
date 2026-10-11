using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Ana oyun döngüsünü yönetir. Sahnedeki verileri okuyup LevelController'ı ayağa kaldırır.
    /// Faz 4B ile oyun Main Menu durumunda başlar.
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

        public bool CanGoToMenu => _currentLevel == null || !_currentLevel.IsAnimating;
        public bool CanRestart => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanRestart;
        public bool CanUndo => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanUndo;
        public bool CanAddTube => _currentState == GameState.Playing && _currentLevel != null && _currentLevel.CanAddTube;

        private void Start()
        {
            if (inputController == null)
                inputController = GetComponent<InputController>();

            if (inputController != null)
            {
                inputController.CanProcessInput = () => _currentState == GameState.Playing;
            }

            if (colorDef == null)
            {
                Debug.LogError("<b>GameManager:</b> ColorDefinition atanmamış! Lütfen Inspector'dan bağlayın.");
                return;
            }

            // LevelManager'ı kur ve başlat
            _levelManager = gameObject.AddComponent<LevelManager>();
            _levelManager.Initialize();

            // UIManager'ı kodla oluştur
            GameObject uiGO = new GameObject("UIManager");
            _uiManager = uiGO.AddComponent<UIManager>();
            _uiManager.Initialize(this);

            // Oyun Main Menu'de başlar
            GoToMenu();
        }

        public void OnPlayFromMenu()
        {
            if (_levelManager == null) return;
            StartSpecificLevel(_levelManager.HighestUnlockedIndex);
        }

        public void ShowLevelSelect()
        {
            if (!CanGoToMenu)
                return;

            DestroyCurrentLevel();
            SetState(GameState.Menu);

            if (_uiManager != null && _levelManager != null)
            {
                _uiManager.ShowLevelSelect(_levelManager.LevelCount, _levelManager.HighestUnlockedIndex);
            }
        }

        public void GoToMenu()
        {
            if (!CanGoToMenu)
                return;

            DestroyCurrentLevel();
            SetState(GameState.Menu);

            if (_uiManager != null)
            {
                _uiManager.ShowMainMenu();
            }
        }

        public void StartSpecificLevel(int levelIndex)
        {
            if (_currentLevel != null && _currentLevel.IsAnimating)
                return;

            if (_levelManager != null)
            {
                _levelManager.SetPlayingLevel(levelIndex);
            }

            StartLevelInternal();
        }

        public void StartLevel()
        {
            StartLevelInternal();
        }

        private void StartLevelInternal()
        {
            DestroyCurrentLevel();

            var levelData = _levelManager != null ? _levelManager.GetCurrentLevel() : null;
            if (levelData == null)
            {
                Debug.LogError("<b>GameManager:</b> Yüklenecek seviye bulunamadı! Lütfen Tools > Color Sort > Generate Levels ile seviye üretin.");
                return;
            }

            SetState(GameState.Playing);

            GameObject levelGO = new GameObject("LevelController");
            _currentLevel = levelGO.AddComponent<LevelController>();
            
            _currentLevel.OnLevelCompleted += HandleLevelCompleted;
            _currentLevel.Setup(levelData, colorDef, inputController);

            if (_uiManager != null)
            {
                _uiManager.ShowGameplay();
            }
            UpdateLevelDisplay();
        }

        private void DestroyCurrentLevel()
        {
            if (_currentLevel != null)
            {
                _currentLevel.Cleanup();
                Destroy(_currentLevel.gameObject);
                _currentLevel = null;
            }
        }

        public void TryRestart()
        {
            if (!CanRestart)
                return;

            StartLevelInternal();
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
            
            int completedLevelNumber = _levelManager != null ? _levelManager.GetCurrentDisplayLevelNumber() : 1;
            bool isLastLevel = _levelManager != null && _levelManager.IsOnLastLevel;

            if (_levelManager != null)
            {
                _levelManager.CompleteCurrentLevel();
            }

            if (_uiManager != null)
            {
                _uiManager.ShowLevelComplete(completedLevelNumber, isLastLevel);
            }
        }

        public void OnNextLevel()
        {
            if (_currentState != GameState.LevelComplete)
                return;

            StartLevelInternal();
        }

        public void OnPlayAgain()
        {
            if (_currentState != GameState.LevelComplete)
                return;

            StartSpecificLevel(0);
        }

        public void OnMenuFromComplete()
        {
            if (_currentState != GameState.LevelComplete)
                return;

            GoToMenu();
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
