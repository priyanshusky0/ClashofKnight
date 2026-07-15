// Online multiplayer via Photon PUN 2.
// This whole file is inert until you import PUN 2 (which defines
// PHOTON_UNITY_NETWORKING), so the project still compiles without it.
#if PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static PhotonNetworkManager Instance;

    // The colour this client controls ("white" = host/first, "black" = joiner)
    public static string MyColor = "white";

    // Photon event codes
    private const byte MoveEventCode = 1;
    private const byte WinEventCode = 2;

    private int pendingBoardSize = 8;
    private bool isMatchmaking;

    // Room-code matchmaking
    private enum Pending { None, QuickMatch, Create, Join }
    private Pending pending = Pending.None;
    private string pendingCode = "";

    // The code others use to join the room we created
    public static string RoomCode = "";

    // UI subscribes to these to show status / the generated code in the menu.
    public static System.Action<string> OnStatusMessage;
    public static System.Action<string> OnRoomCodeReady;

    private TMPro.TMP_Text statusText;

    // NOTE: MonoBehaviourPunCallbacks already registers this object as a
    // callback target in its own OnEnable (that registration also covers
    // IOnEventCallback). Do NOT call PhotonNetwork.AddCallbackTarget again
    // here, or every callback (OnConnectedToMaster, ...) fires twice.

    // Pin every client to ONE region. With "Best Region" each client pings and
    // may pick a different region, so a room created in region A can't be found
    // by a joiner in region B ("no game found"). Change this to the code nearest
    // you if you like: "in" (India), "asia" (Singapore), "eu", "us", "usw"...
    private const string FixedRegion = "in";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Both clients must load the game scene together
        PhotonNetwork.AutomaticallySyncScene = true;
        // Force a shared region so room codes always resolve to the same place
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = FixedRegion;
    }

    // ---------- Matchmaking ----------

    public void StartQuickMatch(int boardSize)
    {
        BeginConnect(Pending.QuickMatch, boardSize, "");
    }

    // Host a private game; a shareable code is generated.
    public void CreateGame(int boardSize)
    {
        BeginConnect(Pending.Create, boardSize, GenerateCode());
    }

    // Join a friend's private game by code.
    public void JoinGame(string code, int boardSize)
    {
        BeginConnect(Pending.Join, boardSize, (code ?? "").Trim().ToUpperInvariant());
    }

    private void BeginConnect(Pending action, int boardSize, string code)
    {
        pending = action;
        pendingBoardSize = boardSize;
        pendingCode = code;
        Game.Online = true;

        if (PhotonNetwork.InRoom) { PhotonNetwork.LeaveRoom(); }

        ShowStatus("Connecting...");
        if (PhotonNetwork.IsConnectedAndReady)
            ExecutePending();
        else if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        // else: connecting; OnConnectedToMaster will run ExecutePending
    }

    public override void OnConnectedToMaster()
    {
        ExecutePending();
    }

    private void ExecutePending()
    {
        // Matchmaking must run on the Master Server; guard against re-entry
        if (isMatchmaking || PhotonNetwork.InRoom || pending == Pending.None) return;
        isMatchmaking = true;

        Hashtable props = new Hashtable { { "bs", pendingBoardSize } };
        string[] lobbyProps = { "bs" };

        switch (pending)
        {
            case Pending.QuickMatch:
                ShowStatus("Looking for a match...");
                PhotonNetwork.JoinRandomOrCreateRoom(expectedMaxPlayers: 2, roomOptions:
                    new RoomOptions { MaxPlayers = 2, CustomRoomProperties = props, CustomRoomPropertiesForLobby = lobbyProps });
                break;

            case Pending.Create:
                ShowStatus("Creating game...");
                RoomCode = pendingCode;
                PhotonNetwork.CreateRoom(pendingCode, new RoomOptions
                {
                    MaxPlayers = 2,
                    IsVisible = false, // code-only, not in random matchmaking
                    CustomRoomProperties = props,
                    CustomRoomPropertiesForLobby = lobbyProps
                });
                break;

            case Pending.Join:
                ShowStatus("Joining " + pendingCode + "...");
                PhotonNetwork.JoinRoom(pendingCode);
                break;
        }
    }

    public override void OnCreatedRoom()
    {
        // We are the host; surface the code so it can be shared
        RoomCode = PhotonNetwork.CurrentRoom.Name;
        if (OnRoomCodeReady != null) OnRoomCodeReady(RoomCode);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // Extremely rare code collision: pick a new code and retry
        isMatchmaking = false;
        pendingCode = GenerateCode();
        ExecutePending();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        isMatchmaking = false;
        pending = Pending.None;
        ShowStatus("No game found with code " + pendingCode);
    }

    public override void OnJoinedRoom()
    {
        isMatchmaking = false;
        pending = Pending.None;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            StartMatch();
        else
            ShowStatus("Share code: " + PhotonNetwork.CurrentRoom.Name + "   •   Waiting for opponent...");
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no ambiguous 0/O/1/I
        System.Text.StringBuilder sb = new System.Text.StringBuilder(4);
        for (int i = 0; i < 4; i++) sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        return sb.ToString();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            StartMatch();
    }

    private void StartMatch()
    {
        MyColor = PhotonNetwork.IsMasterClient ? "white" : "black";
        ShowStatus("");

        // Only the host triggers the scene load; AutomaticallySyncScene loads
        // the same scene on the other client.
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Game 1");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isMatchmaking = false;
        pending = Pending.None;
        Game.Online = false;
        ShowStatus("Disconnected: " + cause);
    }

    public int GetBoardSize()
    {
        if (PhotonNetwork.InRoom &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bs", out object v))
            return (int)v;
        return pendingBoardSize;
    }

    // ---------- Move / win sync ----------

    public void SendMove(string color, int x, int y)
    {
        // Sent to the opponent only; the mover applies the move locally.
        object[] data = new object[] { color, x, y };
        PhotonNetwork.RaiseEvent(MoveEventCode, data,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);
    }

    public void SendWin(string loser)
    {
        PhotonNetwork.RaiseEvent(WinEventCode, loser,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        Game game = FindObjectOfType<Game>();
        if (game == null) return;

        if (photonEvent.Code == MoveEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string color = (string)data[0];
            int x = (int)data[1];
            int y = (int)data[2];
            game.NetworkMove(color, x, y);
        }
        else if (photonEvent.Code == WinEventCode)
        {
            string loser = (string)photonEvent.CustomData;
            game.ShowWinner(loser);
        }
    }

    // ---------- Minimal on-screen status ----------

    private void ShowStatus(string msg)
    {
        // Prefer the menu panel if it's listening; otherwise use the overlay.
        if (OnStatusMessage != null)
        {
            OnStatusMessage(msg);
            return;
        }

        if (statusText == null)
        {
            GameObject canvasGo = new GameObject("NetStatusCanvas");
            DontDestroyOnLoad(canvasGo);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject txtGo = new GameObject("NetStatus");
            txtGo.transform.SetParent(canvasGo.transform, false);
            statusText = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
            statusText.font = TMPro.TMP_Settings.defaultFontAsset;
            statusText.fontSize = 28;
            statusText.alignment = TMPro.TextAlignmentOptions.Center;
            statusText.color = Color.white;

            RectTransform rt = statusText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 40);
            rt.sizeDelta = new Vector2(700, 60);
        }
        statusText.text = msg;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }
}
#endif
