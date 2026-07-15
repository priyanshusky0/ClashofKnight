using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A small runtime UI toolkit so we can build clean, modern controls from code
// (rounded cards, styled buttons, input fields) without hand-editing scenes.
public static class UITheme
{
    // ---- Palette ("Clash of Knight": obsidian + gold + emerald) ----
    public static readonly Color Bg        = new Color32(0x12, 0x14, 0x1C, 0xFF);
    public static readonly Color Surface   = new Color32(0x1C, 0x20, 0x30, 0xFF);
    public static readonly Color SurfaceHi = new Color32(0x27, 0x2C, 0x40, 0xFF);
    public static readonly Color Border    = new Color32(0x3A, 0x41, 0x5C, 0xFF);
    public static readonly Color Gold      = new Color32(0xE8, 0xB0, 0x4B, 0xFF);
    public static readonly Color GoldHi    = new Color32(0xF3, 0xC4, 0x6C, 0xFF);
    public static readonly Color Emerald   = new Color32(0x3F, 0xA6, 0x6A, 0xFF);
    public static readonly Color EmeraldHi = new Color32(0x53, 0xC0, 0x82, 0xFF);
    public static readonly Color TextHi    = new Color32(0xF2, 0xF3, 0xF7, 0xFF);
    public static readonly Color TextMuted = new Color32(0x9B, 0xA1, 0xB0, 0xFF);
    public static readonly Color InkOnGold = new Color32(0x1A, 0x14, 0x02, 0xFF);

    // ---- Light "editorial chess" palette (menu) ----
    public static readonly Color Paper  = new Color32(0xF4, 0xF2, 0xEC, 0xFF);
    public static readonly Color Card   = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public static readonly Color Line   = new Color32(0xE6, 0xE2, 0xD8, 0xFF);
    public static readonly Color Ink    = new Color32(0x20, 0x24, 0x2E, 0xFF);
    public static readonly Color Muted  = new Color32(0x8B, 0x8F, 0x99, 0xFF);
    public static readonly Color Brass  = new Color32(0xB8, 0x86, 0x3B, 0xFF);
    public static readonly Color Blue   = new Color32(0x3E, 0x63, 0xDD, 0xFF);
    public static readonly Color Violet = new Color32(0x7A, 0x5C, 0xF0, 0xFF);
    public static readonly Color Green  = new Color32(0x2E, 0x9E, 0x6B, 0xFF);

    private static Sprite _rounded;

    public static LayoutElement Fixed(Component c, float height, float minWidth = 0f)
    {
        LayoutElement le = c.gameObject.GetComponent<LayoutElement>();
        if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
        if (minWidth > 0f) { le.preferredWidth = minWidth; le.minWidth = minWidth; }
        return le;
    }

    public static Slider CreateSlider(Transform parent, float min, float max, float value,
        bool whole, Color accent, System.Action<float> onChanged)
    {
        GameObject root = new GameObject("Slider", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        Slider slider = root.AddComponent<Slider>();

        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.sprite = RoundedSprite(); bgImg.type = Image.Type.Sliced; bgImg.color = Line;
        RectTransform bgRt = bgImg.rectTransform;
        bgRt.anchorMin = new Vector2(0f, 0.5f); bgRt.anchorMax = new Vector2(1f, 0.5f);
        bgRt.sizeDelta = new Vector2(0f, 7f); bgRt.anchoredPosition = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.5f); faRt.anchorMax = new Vector2(1f, 0.5f);
        faRt.sizeDelta = new Vector2(-18f, 7f); faRt.anchoredPosition = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = RoundedSprite(); fillImg.type = Image.Type.Sliced; fillImg.color = accent;
        fillImg.rectTransform.sizeDelta = new Vector2(18f, 7f);

        GameObject hsa = new GameObject("Handle Slide Area", typeof(RectTransform));
        hsa.transform.SetParent(root.transform, false);
        RectTransform hsaRt = hsa.GetComponent<RectTransform>();
        hsaRt.anchorMin = new Vector2(0f, 0f); hsaRt.anchorMax = new Vector2(1f, 1f);
        hsaRt.sizeDelta = new Vector2(-18f, 0f); hsaRt.anchoredPosition = Vector2.zero;

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(hsa.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.sprite = RoundedSprite(); handleImg.type = Image.Type.Sliced; handleImg.color = Card;
        handleImg.rectTransform.sizeDelta = new Vector2(20f, 20f);

        slider.fillRect = fillImg.rectTransform;
        slider.handleRect = handleImg.rectTransform;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min; slider.maxValue = max; slider.wholeNumbers = whole;
        slider.value = value;
        if (onChanged != null) slider.onValueChanged.AddListener(v => onChanged(v));
        return slider;
    }

    // A white, 9-sliced rounded-rectangle sprite tinted via Image.color.
    public static Sprite RoundedSprite()
    {
        if (_rounded != null) return _rounded;

        int r = 20;
        int size = r * 2 + 8;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f;
                float cx = Mathf.Clamp(fx, r, size - r);
                float cy = Mathf.Clamp(fy, r, size - r);
                float dx = fx - cx, dy = fy - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        _rounded = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
        return _rounded;
    }

    private static TMP_FontAsset Font => TMP_Settings.defaultFontAsset;

    public static void Stretch(RectTransform rt, float pad = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    public static Image CreatePanel(Transform parent, Color color)
    {
        GameObject go = new GameObject("Panel", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = RoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        return img;
    }

    public static TMP_Text CreateLabel(Transform parent, string text, int size, Color color,
        FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.font = Font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        return t;
    }

    public static Button CreateButton(Transform parent, string label, Color fill, Color textColor,
        System.Action onClick, int fontSize = 26)
    {
        GameObject go = new GameObject("Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = RoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = fill;

        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        cb.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        cb.selectedColor = Color.white;
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        if (onClick != null) b.onClick.AddListener(() => onClick());

        TMP_Text t = CreateLabel(go.transform, label, fontSize, textColor, FontStyles.Bold);
        Stretch(t.rectTransform);
        t.enableAutoSizing = true;
        t.fontSizeMin = 12;
        t.fontSizeMax = fontSize;

        return b;
    }

    public static TMP_InputField CreateInputField(Transform parent, string placeholder, int fontSize = 30)
    {
        GameObject root = new GameObject("InputField", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        Image bg = root.AddComponent<Image>();
        bg.sprite = RoundedSprite();
        bg.type = Image.Type.Sliced;
        bg.color = Bg;

        TMP_InputField input = root.AddComponent<TMP_InputField>();

        GameObject area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(root.transform, false);
        RectTransform areaRt = area.GetComponent<RectTransform>();
        Stretch(areaRt, 12f);

        TMP_Text ph = CreateLabel(area.transform, placeholder, fontSize, TextMuted, FontStyles.Normal, TextAlignmentOptions.Center);
        Stretch(ph.rectTransform);

        TMP_Text txt = CreateLabel(area.transform, "", fontSize, TextHi, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(txt.rectTransform);

        input.textViewport = areaRt;
        input.textComponent = txt;
        input.placeholder = ph;
        input.fontAsset = Font;
        input.characterLimit = 6;
        input.onFocusSelectAll = true;
        input.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
        input.text = "";

        return input;
    }
}
