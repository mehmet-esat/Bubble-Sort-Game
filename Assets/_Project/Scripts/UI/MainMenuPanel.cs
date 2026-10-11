using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorSortPuzzle
{
    public class MainMenuPanel : MonoBehaviour
    {
        public Action OnPlayClicked;
        public Action OnLevelsClicked;

        private Button _playButton;
        private Button _levelsButton;

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
            bg.color = new Color(0.04f, 0.12f, 0.28f, 1f);
            bg.raycastTarget = false; // Panel arka planı raycast almasın

            // İçerik Kapsayıcısı (Dikey ortalanmış)
            GameObject contentGO = new GameObject("MenuContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contentGO.transform.SetParent(transform, false);

            RectTransform contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(600f, 600f);

            VerticalLayoutGroup layout = contentGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 36f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Başlık
            CreateTitle(contentGO.transform, "BALL SORT", font);

            // Play Butonu
            _playButton = CreateButton(contentGO.transform, "PlayButton", "Play", 420f, 130f, font, () =>
            {
                OnPlayClicked?.Invoke();
            });

            // Levels Butonu
            _levelsButton = CreateButton(contentGO.transform, "LevelsButton", "Levels", 420f, 130f, font, () =>
            {
                OnLevelsClicked?.Invoke();
            });
        }

        private void CreateTitle(Transform parent, string titleText, Font font)
        {
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(parent, false);

            RectTransform rt = titleGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(560f, 120f);

            Text txt = titleGO.GetComponent<Text>();
            txt.font = font;
            txt.text = titleText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 58;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;
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
