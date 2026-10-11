using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorSortPuzzle
{
    public class LevelCompletePanel : MonoBehaviour
    {
        public Action OnNextLevel;
        public Action OnPlayAgain;
        public Action OnMenu;

        private Text _titleText;
        private Button _nextLevelButton;
        private Button _playAgainButton;
        private Button _menuButton;
        private bool _isActionHandled;

        public void Setup(Font font)
        {
            RectTransform panelRect = gameObject.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = gameObject.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = gameObject.GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.10f, 0.24f, 0.85f);
            bg.raycastTarget = false; // Panel arka plani raycast almasin (GameState girisi engeller)

            // Icerik Kapsayicisi (Dikey ortalanmis)
            GameObject contentGO = new GameObject("CompleteContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contentGO.transform.SetParent(transform, false);

            RectTransform contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(650f, 600f);

            VerticalLayoutGroup layout = contentGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 32f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Baslik
            _titleText = CreateTitle(contentGO.transform, "Level 1 Complete", font);

            // Next Level Butonu
            _nextLevelButton = CreateButton(contentGO.transform, "NextLevelButton", "Next Level", 440f, 130f, font, HandleNextLevelClicked);

            // Play Again Butonu (Son seviyede Next Level yerine gosterilir)
            _playAgainButton = CreateButton(contentGO.transform, "PlayAgainButton", "Play Again", 440f, 130f, font, HandlePlayAgainClicked);

            // Menu Butonu
            _menuButton = CreateButton(contentGO.transform, "MenuButton", "Menu", 440f, 130f, font, HandleMenuClicked);
        }

        public void Show(int completedLevelNumber, bool isLastLevel)
        {
            _isActionHandled = false;
            _titleText.text = isLastLevel ? "All Levels Complete" : $"Level {completedLevelNumber} Complete";

            _nextLevelButton.gameObject.SetActive(!isLastLevel);
            _playAgainButton.gameObject.SetActive(isLastLevel);

            _nextLevelButton.interactable = true;
            _playAgainButton.interactable = true;
            _menuButton.interactable = true;

            gameObject.SetActive(true);
        }

        private void HandleNextLevelClicked()
        {
            if (_isActionHandled) return;
            _isActionHandled = true;
            DisableAllButtons();
            gameObject.SetActive(false);
            OnNextLevel?.Invoke();
        }

        private void HandlePlayAgainClicked()
        {
            if (_isActionHandled) return;
            _isActionHandled = true;
            DisableAllButtons();
            gameObject.SetActive(false);
            OnPlayAgain?.Invoke();
        }

        private void HandleMenuClicked()
        {
            if (_isActionHandled) return;
            _isActionHandled = true;
            DisableAllButtons();
            gameObject.SetActive(false);
            OnMenu?.Invoke();
        }

        private void DisableAllButtons()
        {
            if (_nextLevelButton != null) _nextLevelButton.interactable = false;
            if (_playAgainButton != null) _playAgainButton.interactable = false;
            if (_menuButton != null) _menuButton.interactable = false;
        }

        private Text CreateTitle(Transform parent, string titleText, Font font)
        {
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(parent, false);

            RectTransform rt = titleGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(620f, 120f);

            Text txt = titleGO.GetComponent<Text>();
            txt.font = font;
            txt.text = titleText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 50;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return txt;
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
            txt.fontSize = 40;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btn;
        }
    }
}
