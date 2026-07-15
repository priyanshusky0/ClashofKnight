#if PHOTON_UNITY_NETWORKING
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A clean, modern "Play Online" panel built entirely at runtime.
// Lets a player HOST (get a shareable code) or JOIN with a friend's code.
public class OnlineMenu : MonoBehaviour
{
    private GameObject root;
    private TMP_InputField codeInput;
    private TMP_Text statusLabel;
    private TMP_Text codeLabel;

    public static OnlineMenu Open(int boardSize)
    {
        GameObject go = new GameObject("OnlineMenu");
        OnlineMenu m = go.AddComponent<OnlineMenu>();
        m.Build(boardSize);
        return m;
    }

    private int boardSize;

    private void Build(int bs)
    {
        boardSize = bs;

        // Overlay canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Dim background
        GameObject dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(transform, false);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0.02f, 0.03f, 0.05f, 0.72f);
        UITheme.Stretch(dimImg.rectTransform);

        // Card
        Image card = UITheme.CreatePanel(transform, UITheme.Surface);
        root = card.gameObject;
        RectTransform cardRt = card.rectTransform;
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(460, 10);
        cardRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 26, 26);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter fitter = card.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Row(UITheme.CreateLabel(card.transform, "PLAY ONLINE", 34, UITheme.Gold, FontStyles.Bold), 46);
        Row(UITheme.CreateLabel(card.transform, "Host a game and share the code, or\njoin your friend's code.", 17, UITheme.TextMuted), 46);

        Button create = UITheme.CreateButton(card.transform, "CREATE GAME", UITheme.Gold, UITheme.InkOnGold, OnCreate);
        Row(create, 56);

        codeLabel = UITheme.CreateLabel(card.transform, "", 40, UITheme.Gold, FontStyles.Bold);
        Row(codeLabel, 50);
        codeLabel.gameObject.SetActive(false);

        Row(UITheme.CreateLabel(card.transform, "— or join with a code —", 15, UITheme.TextMuted), 24);

        codeInput = UITheme.CreateInputField(card.transform, "ENTER CODE");
        Row(codeInput, 56);

        Button join = UITheme.CreateButton(card.transform, "JOIN GAME", UITheme.Emerald, Color.white, OnJoin);
        Row(join, 56);

        statusLabel = UITheme.CreateLabel(card.transform, "", 16, UITheme.TextHi);
        Row(statusLabel, 40);

        Button back = UITheme.CreateButton(card.transform, "BACK", UITheme.SurfaceHi, UITheme.TextHi, Close, 20);
        Row(back, 44);

        PhotonNetworkManager.OnStatusMessage = SetStatus;
        PhotonNetworkManager.OnRoomCodeReady = ShowCode;
    }

    private static void Row(Component c, float height)
    {
        LayoutElement le = c.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
    }

    private void EnsureManager()
    {
        if (PhotonNetworkManager.Instance == null)
            new GameObject("PhotonNetworkManager").AddComponent<PhotonNetworkManager>();
    }

    private void OnCreate()
    {
        EnsureManager();
        SetStatus("Connecting...");
        PhotonNetworkManager.Instance.CreateGame(boardSize);
    }

    private void OnJoin()
    {
        string code = codeInput != null ? codeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(code)) { SetStatus("Enter a code first."); return; }
        EnsureManager();
        SetStatus("Connecting...");
        PhotonNetworkManager.Instance.JoinGame(code, boardSize);
    }

    private void SetStatus(string msg)
    {
        if (statusLabel != null) statusLabel.text = msg;
    }

    private void ShowCode(string code)
    {
        if (codeLabel != null)
        {
            codeLabel.text = code;
            codeLabel.gameObject.SetActive(true);
        }
    }

    private void Close()
    {
        PhotonNetworkManager.OnStatusMessage = null;
        PhotonNetworkManager.OnRoomCodeReady = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (PhotonNetworkManager.OnStatusMessage == SetStatus) PhotonNetworkManager.OnStatusMessage = null;
        if (PhotonNetworkManager.OnRoomCodeReady == ShowCode) PhotonNetworkManager.OnRoomCodeReady = null;
    }
}
#endif
