using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorSortPuzzle
{
    public class LevelSelectPanel : MonoBehaviour
    {
        public Action OnBackClicked;
        public Action<int> OnLevelSelected;

        private Font _font;
        private Transform _gridContent;
        private readonly List<Button> _levelButtons = new List<Button>();

        public void Setup(Font font)
        {
            _font = font;

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

            float safeTopRatio = Screen.height > 0 ? (Screen.height - Screen.safeArea.yMax) / Screen.height : 0f;
            float safeTopCanvas = safeTopRatio * 1920f;
            float safeBottomRatio = Screen.height > 0 ? Screen.safeArea.yMin / Screen.height : 0f;
            float safeBottomCanvas = safeBottomRatio * 1920f;

            // Üst Başlık ve Geri Butonu Alanı (Header)
            GameObject headerGO = new GameObject("Header", typeof(RectTransform));
            headerGO.transform.SetParent(transform, false);

            RectTransform headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -safeTopCanvas - 10f);
            headerRect.sizeDelta = new Vector2(0f, 140f);

            // Back Butonu
            Button backBtn = CreateButton(headerGO.transform, "BackButton", "Back", 180f, 90f, _font, () =>
            {
                OnBackClicked?.Invoke();
            });
            RectTransform backRt = backBtn.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0f, 0.5f);
            backRt.anchorMax = new Vector2(0f, 0.5f);
            backRt.pivot = new Vector2(0f, 0.5f);
            backRt.anchoredPosition = new Vector2(40f, 0f);

            // Başlık Metni
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(headerGO.transform, false);

            RectTransform titleRt = titleGO.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(400f, 100f);
            titleRt.anchoredPosition = Vector2.zero;

            Text titleTxt = titleGO.GetComponent<Text>();
            titleTxt.font = _font;
            titleTxt.text = "LEVELS";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.fontSize = 50;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = Color.white;
            titleTxt.raycastTarget = false;

            // Scroll View Kökü
            GameObject scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGO.transform.SetParent(transform, false);

            RectTransform scrollRt = scrollGO.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(0f, safeBottomCanvas + 20f);
            scrollRt.offsetMax = new Vector2(0f, -safeTopCanvas - 160f);

            ScrollRect scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 35f;

            // Viewport (RectMask2D ile sınırlanır)
            GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGO.transform.SetParent(scrollGO.transform, false);

            RectTransform viewRt = viewportGO.GetComponent<RectTransform>();
            viewRt.anchorMin = Vector2.zero;
            viewRt.anchorMax = Vector2.one;
            viewRt.offsetMin = Vector2.zero;
            viewRt.offsetMax = Vector2.zero;

            scrollRect.viewport = viewRt;

            // Grid İçeriği (Content)
            GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);

            RectTransform contentRt = contentGO.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            scrollRect.content = contentRt;

            GridLayoutGroup grid = contentGO.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(190f, 190f);
            grid.spacing = new Vector2(25f, 25f);
            grid.padding = new RectOffset(60, 60, 20, 100);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = contentGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            _gridContent = contentGO.transform;
        }

        public void Refresh(int totalLevels, int highestUnlockedIndex)
        {
            // İhtiyaç kadar buton oluştur
            while (_levelButtons.Count < totalLevels)
            {
                int levelIndex = _levelButtons.Count;
                Button btn = CreateGridLevelButton(_gridContent, levelIndex, _font);
                _levelButtons.Add(btn);
            }

            // Durumları güncelle
            for (int i = 0; i < _levelButtons.Count; i++)
            {
                if (i < totalLevels)
                {
                    _levelButtons[i].gameObject.SetActive(true);
                    bool isUnlocked = (i <= highestUnlockedIndex);
                    _levelButtons[i].interactable = isUnlocked;
                }
                else
                {
                    _levelButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private Button CreateGridLevelButton(Transform parent, int levelIndex, Font font)
        {
            GameObject btnGO = new GameObject($"LevelBtn_{levelIndex + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);

            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.18f, 0.46f, 0.82f, 1.0f);
            img.raycastTarget = true;

            Button btn = btnGO.GetComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 0.9f, 1f);
            colors.disabledColor = new Color(0.35f, 0.38f, 0.45f, 0.7f);
            btn.colors = colors;

            int capturedIndex = levelIndex;
            btn.onClick.AddListener(() =>
            {
                OnLevelSelected?.Invoke(capturedIndex);
            });

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);

            RectTransform textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text txt = textGO.GetComponent<Text>();
            txt.font = font;
            txt.text = (levelIndex + 1).ToString();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 44;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btn;
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
    }
}
