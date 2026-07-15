using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{

    // Add the extra mode buttons on load. Wrapped so a UI hiccup never blocks play.
    private void Start()
    {
        try { CreateModeButtons(); } catch (System.Exception e) { Debug.LogWarning(e); }
    }

    private void CreateModeButtons()
    {
        // The Play button is named "Play Button " (note the trailing space)
        GameObject playBtn = GameObject.Find("Play Button ");
        if (playBtn == null) playBtn = GameObject.Find("Play Button");
        if (playBtn == null) return;

        // Avoid creating duplicates on scene reload
        if (GameObject.Find("VsComputer Button") != null) return;

        RectTransform src = playBtn.GetComponent<RectTransform>();
        float y = src.anchoredPosition.y;

        // Collect the buttons we want in the Play row
        List<GameObject> row = new List<GameObject> { playBtn };
        SetLabel(playBtn, "2 PLAYER");

        GameObject aiBtn = CloneButton(playBtn, "VsComputer Button", "VS AI", playVsComputer);
        row.Add(aiBtn);

#if PHOTON_UNITY_NETWORKING
        GameObject onlineBtn = CloneButton(playBtn, "Online Button", "ONLINE", playOnline);
        row.Add(onlineBtn);
#endif

        // Lay them out evenly across the Play row (no vertical collisions)
        int n = row.Count;
        float width = (n >= 3) ? 165f : src.sizeDelta.x;
        float height = 50f;
        float spacing = width + 22f;
        float startX = -spacing * (n - 1) / 2f;
        for (int i = 0; i < n; i++)
        {
            RectTransform rt = row[i].GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(startX + spacing * i, y);
        }
    }

    // Clone the Play button, relabel it and rewire its click to `action`.
    private GameObject CloneButton(GameObject playBtn, string name, string label, UnityAction action)
    {
        GameObject clone = Instantiate(playBtn);
        clone.name = name;
        clone.transform.SetParent(playBtn.transform.parent, false);

        SetLabel(clone, label);

        Button b = clone.GetComponent<Button>();
        if (b != null)
        {
            // Disable the copied "playGame" persistent call, then add ours
            for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                b.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }
        return clone;
    }

    private void SetLabel(GameObject buttonObj, string label)
    {
        TMP_Text tmp = buttonObj.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            // Shrink to fit the button width instead of overflowing
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            if (tmp.fontSize > tmp.fontSizeMax) tmp.fontSizeMax = tmp.fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            return;
        }

        Text legacy = buttonObj.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            legacy.text = label;
            legacy.resizeTextForBestFit = true;
            legacy.alignment = TextAnchor.MiddleCenter;
        }
    }

    public void playGame()
    {
        // Default "Play" button keeps the classic 2-player behaviour
        Game.VsComputer = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Hook a "2 Player" button to this
    public void playTwoPlayer()
    {
        Game.VsComputer = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Hook a "vs Computer" button to this
    public void playVsComputer()
    {
        Game.VsComputer = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Opens the online panel (host with a code, or join a friend's code)
    public void playOnline()
    {
#if PHOTON_UNITY_NETWORKING
        Game.VsComputer = false;
        var slider = FindObjectOfType<sliderScript>();
        int bs = (slider != null && slider.value >= 2) ? slider.value : 8;
        OnlineMenu.Open(bs);
#else
        Debug.LogWarning("Online play requires importing Photon PUN 2.");
#endif
    }

    public void quitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}

