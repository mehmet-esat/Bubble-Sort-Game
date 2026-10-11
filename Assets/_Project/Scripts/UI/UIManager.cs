using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ColorSortPuzzle
{
    public class UIManager : MonoBehaviour
    {
        public const float BottomBarHeightPixels = 180f;

        private GameManager _gameManager;
        private Text _levelText;
        private Button _homeButton;
        private Button _restartButton;
        private Button _undoButton;
        private Button _addTubeButton;

        private GameObject _bottomBarGO;
        private MainMenuPanel _mainMenuPanel;
        private LevelSelectPanel _levelSelectPanel;

        public static float GetBottomBarScreenFraction()
        {
            float safeAreaBottomPixels = Screen.safeArea.yMin;
            float safeAreaFraction = Screen.height > 0 ? safeAreaBottomPixels / Screen.height : 0f;
            float barFraction = BottomBarHeightPixels / 1920f;
            return Mathf.Clamp(safeAreaFraction + barFraction, 0.08f, 0.28f);
        }

        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            EnsureEventSystem();
            BuildUI();
        }

        public void ShowMainMenu()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.gameObject.SetActive(true);
            if (_levelSelectPanel != null) _levelSelectPanel.gameObject.SetActive(false);
            if (_bottomBarGO != null) _bottomBarGO.SetActive(false);
        }

        public void ShowLevelSelect(int totalLevels, int highestUnlockedIndex)
        {
            if (_mainMenuPanel != null) _mainMenuPanel.gameObject.SetActive(false);
            if (_levelSelectPanel != null)
            {
                _levelSelectPanel.Refresh(totalLevels, highestUnlockedIndex);
                _levelSelectPanel.gameObject.SetActive(true);
            }
            if (_bottomBarGO != null) _bottomBarGO.SetActive(false);
        }

        public void ShowGameplay()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.gameObject.SetActive(false);
            if (_levelSelectPanel != null) _levelSelectPanel.gameObject.SetActive(false);
            if (_bottomBarGO != null) _bottomBarGO.SetActive(true);
        }

        public void SetLevelNumber(int levelNumber)
        {
            if (_levelText != null)
            {
                _levelText.text = levelNumber.ToString();
            }
        }

        private void Update()
        {
            if (_gameManager == null) return;

            if (_homeButton != null)
                _homeButton.interactable = _gameManager.CanGoToMenu;

            if (_restartButton != null)
                _restartButton.interactable = _gameManager.CanRestart;

            if (_undoButton != null)
                _undoButton.interactable = _gameManager.CanUndo;

            if (_addTubeButton != null)
                _addTubeButton.interactable = _gameManager.CanAddTube;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null && Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void BuildUI()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                Debug.LogWarning("UIManager: LegacyRuntime.ttf font bulunamadi!");
            }

            // 1. Main Menu Paneli
            GameObject menuGO = new GameObject("MainMenuPanel");
            menuGO.transform.SetParent(transform, false);
            _mainMenuPanel = menuGO.AddComponent<MainMenuPanel>();
            _mainMenuPanel.Setup(font);
            _mainMenuPanel.OnPlayClicked += _gameManager.OnPlayFromMenu;
            _mainMenuPanel.OnLevelsClicked += _gameManager.ShowLevelSelect;

            // 2. Level Select Paneli
            GameObject levelSelectGO = new GameObject("LevelSelectPanel");
            levelSelectGO.transform.SetParent(transform, false);
            _levelSelectPanel = levelSelectGO.AddComponent<LevelSelectPanel>();
            _levelSelectPanel.Setup(font);
            _levelSelectPanel.OnBackClicked += _gameManager.GoToMenu;
            _levelSelectPanel.OnLevelSelected += _gameManager.StartSpecificLevel;

            // 3. Gameplay Alt Barı
            BuildBottomBar(font);

            // Başlangıçta paneller kapalı, GameManager tarafından belirlenecek
            _mainMenuPanel.gameObject.SetActive(false);
            _levelSelectPanel.gameObject.SetActive(false);
            _bottomBarGO.SetActive(false);
        }

        private void BuildBottomBar(Font font)
        {
            float safeBottomRatio = Screen.height > 0 ? Screen.safeArea.yMin / Screen.height : 0f;
            float safeBottomCanvas = safeBottomRatio * 1920f;
            float totalBarHeight = BottomBarHeightPixels + safeBottomCanvas;

            _bottomBarGO = new GameObject("BottomBar", typeof(RectTransform), typeof(Image));
            _bottomBarGO.transform.SetParent(transform, false);

            Image barImage = _bottomBarGO.GetComponent<Image>();
            barImage.color = new Color(0.08f, 0.20f, 0.42f, 0.95f);

            RectTransform barRect = _bottomBarGO.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.sizeDelta = new Vector2(0f, totalBarHeight);

            // Buton ve İçerik Kapsayıcısı (Safe area içine oturur)
            GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            contentGO.transform.SetParent(_bottomBarGO.transform, false);

            RectTransform contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.offsetMin = new Vector2(20f, safeBottomCanvas + 10f);
            contentRect.offsetMax = new Vector2(-20f, -10f);

            HorizontalLayoutGroup layout = contentGO.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 1. Home Butonu (4B'de aktif, Menüye döner)
            _homeButton = CreateButton(contentGO.transform, "HomeButton", "Home", 170f, 110f, font, () =>
            {
                if (_gameManager != null) _gameManager.GoToMenu();
            });

            // 2. Restart Butonu
            _restartButton = CreateButton(contentGO.transform, "RestartButton", "Restart", 180f, 110f, font, () =>
            {
                if (_gameManager != null) _gameManager.TryRestart();
            });

            // 3. Seviye Numarası Yazısı (1 tabanlı)
            _levelText = CreateLabel(contentGO.transform, "LevelLabel", "1", 150f, 110f, font);

            // 4. Undo Butonu
            _undoButton = CreateButton(contentGO.transform, "UndoButton", "Undo", 170f, 110f, font, () =>
            {
                if (_gameManager != null) _gameManager.TryUndo();
            });

            // 5. Tüp Ekle Butonu
            _addTubeButton = CreateButton(contentGO.transform, "AddTubeButton", "+Tube", 180f, 110f, font, () =>
            {
                if (_gameManager != null) _gameManager.TryAddTube();
            });
        }

        private Button CreateButton(Transform parent, string name, string labelText, float width, float height, Font font, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);

            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.18f, 0.46f, 0.82f, 1.0f);
            img.raycastTarget = true;

            Button btn = btnGO.GetComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 0.9f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            if (action != null)
            {
                btn.onClick.AddListener(action);
            }

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);

            RectTransform textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text txt = textGO.GetComponent<Text>();
            txt.font = font;
            txt.text = labelText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btn;
        }

        private Text CreateLabel(Transform parent, string name, string defaultText, float width, float height, Font font)
        {
            GameObject labelGO = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(parent, false);

            RectTransform rt = labelGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            Text txt = labelGO.GetComponent<Text>();
            txt.font = font;
            txt.text = defaultText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 44;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return txt;
        }
    }
}
