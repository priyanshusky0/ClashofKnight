using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    //Reference from Unity IDE
    public GameObject chesspiece;
    public AudioClip clip;
    public AudioListener Listener;

    //Matrices needed, positions of each of the GameObjects
    //Also separate arrays for the players in order to easily keep track of them all
    private GameObject[,] positions;
    private GameObject[] playerBlack = new GameObject[1];
    private GameObject[] playerWhite = new GameObject[1];

    //Stable references to the two knights (the player arrays get overwritten
    //by the "cross" trail markers after the first move, so we keep our own).
    private GameObject whiteKnight;
    private GameObject blackKnight;

    //current turn
    private string currentPlayer = "white";

    //Game Ending
    private bool gameOver = false;

    // ----- Board sizing -----
    // How many cells per side. Read from the menu slider (4..8), default 8.
    private int boardSize = 8;
    // Total width/height of the board in world units. 0.66 * 8 keeps the 8x8
    // board looking exactly like the old hard-coded layout.
    private const float BoardWorldSize = 5.28f;
    // The cell size the original sprites/offsets were tuned for.
    private const float ReferenceCellSize = 0.66f;

    // ----- Game mode -----
    // Set from the main menu. Static so it survives the scene load.
    public static bool VsComputer = false;
    // True when this client is in an online (Photon) match.
    public static bool Online = false;
    // Colour the computer controls when VsComputer is true.
    public const string ComputerPlayer = "black";
    // Search depth for the computer. Higher = stronger but slower.
    public int aiDepth = 6;

    //Unity calls this right when the game starts
    public void Start()
    {
        var slider_text = FindObjectOfType<sliderScript>();
        boardSize = (slider_text != null && slider_text.value >= 2) ? slider_text.value : 8;

#if PHOTON_UNITY_NETWORKING
        //In an online match both clients must use the host's chosen board size
        if (Online && PhotonNetworkManager.Instance != null)
            boardSize = PhotonNetworkManager.Instance.GetBoardSize();
#endif

        positions = new GameObject[boardSize, boardSize];

        //Draw a board that actually matches the chosen size
        DrawBoard();

        whiteKnight = Create("white_knight", boardSize - 1, 0);
        blackKnight = Create("black_knight", 0, boardSize - 1);
        playerWhite = new GameObject[] { whiteKnight };
        playerBlack = new GameObject[] { blackKnight };

        //Set all piece positions on the positions board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    // ---------- Coordinate system ----------

    public int GetBoardSize()
    {
        return boardSize;
    }

    public float GetCellSize()
    {
        return BoardWorldSize / boardSize;
    }

    // How much to scale a sprite so it keeps the same relative size on any board.
    public float GetPieceScaleFactor()
    {
        return GetCellSize() / ReferenceCellSize;
    }

    // Convert a board cell to a centred world position.
    public Vector3 BoardToWorld(int x, int y, float z)
    {
        float cell = GetCellSize();
        float wx = (x - (boardSize - 1) / 2f) * cell;
        float wy = (y - (boardSize - 1) / 2f) * cell;
        return new Vector3(wx, wy, z);
    }

    // Build a checkerboard texture sized to the current board and drop it on
    // the existing "board" sprite so what you see matches the logical grid.
    private void DrawBoard()
    {
        GameObject boardObj = GameObject.Find("board");
        if (boardObj == null) return;

        const int cellPx = 64;
        int size = boardSize * cellPx;

        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        //Refined walnut/cream palette with a subtle inner edge on each cell
        Color light = new Color(0.925f, 0.855f, 0.710f);
        Color dark = new Color(0.475f, 0.345f, 0.235f);

        for (int cy = 0; cy < boardSize; cy++)
        {
            for (int cx = 0; cx < boardSize; cx++)
            {
                Color baseCol = ((cx + cy) % 2 == 0) ? dark : light;
                for (int py = 0; py < cellPx; py++)
                {
                    for (int px = 0; px < cellPx; px++)
                    {
                        //Thin darker seam between cells for definition
                        bool edge = px < 2 || py < 2 || px >= cellPx - 2 || py >= cellPx - 2;
                        Color col = edge ? baseCol * 0.9f : baseCol;
                        col.a = 1f;
                        tex.SetPixel(cx * cellPx + px, cy * cellPx + py, col);
                    }
                }
            }
        }
        tex.Apply();

        Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size / BoardWorldSize);

        SpriteRenderer sr = boardObj.GetComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.sortingOrder = 0;
        boardObj.transform.localScale = Vector3.one;
        boardObj.transform.position = Vector3.zero;

        //Clean, calm backdrop instead of plain black
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.09f, 0.10f, 0.13f, 1f);
        }
    }

    // ---------- Piece helpers ----------

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        //Overwrites either empty space or whatever was there
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        if (currentPlayer == "white")
        {
            playerWhite = new GameObject[] { Create("cross", x, y) };
        }
        else
        {
            playerBlack = new GameObject[] { Create("cross", x, y) };
        }
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn()
    {
        if (currentPlayer == "white")
        {
            currentPlayer = "black";
        }
        else
        {
            currentPlayer = "white";
        }
    }

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            SceneManager.LoadScene("Game 1"); //Restarts the game by loading the scene over again
        }
    }

    // Called locally when a player is found to have no moves. In an online
    // match we also tell the opponent so both screens show the result.
    public void Winner(string playerWinner)
    {
        ShowWinner(playerWinner);

#if PHOTON_UNITY_NETWORKING
        if (Online && PhotonNetworkManager.Instance != null)
            PhotonNetworkManager.Instance.SendWin(playerWinner);
#endif
    }

    // Just display the result (used directly when a win arrives over the network)
    public void ShowWinner(string playerWinner)
    {
        gameOver = true;

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<AudioSource>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " LOSES THE GAME";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }

    // ---------- Computer opponent ----------

    // Called by MovePlate after a human move completes.
    public void OnHumanMoveComplete()
    {
        if (gameOver) return;
        if (Online) return; //no local AI in online matches
        if (VsComputer && currentPlayer == ComputerPlayer)
        {
            StartCoroutine(ComputerTurnRoutine());
        }
    }

    // Apply a move received from the network to the matching knight.
    public void NetworkMove(string color, int x, int y)
    {
        if (gameOver) return;
        GameObject piece = (color == "white") ? whiteKnight : blackKnight;
        if (piece != null) PerformMove(piece, x, y);
    }

    private IEnumerator ComputerTurnRoutine()
    {
        //Small pause so the move feels natural
        yield return new WaitForSeconds(0.4f);
        if (gameOver) yield break;
        DoComputerMove();
    }

    // Knight move offsets
    private static readonly int[] KnightDX = { 1, -1, 2, 2, 1, -1, -2, -2 };
    private static readonly int[] KnightDY = { 2, 2, 1, -1, -2, -2, 1, -1 };

    private void DoComputerMove()
    {
        int n = boardSize;

        //Snapshot current occupancy (any non-null cell is blocked)
        bool[,] occ = new bool[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                occ[i, j] = positions[i, j] != null;

        Chessman me = blackKnight.GetComponent<Chessman>();
        Chessman them = whiteKnight.GetComponent<Chessman>();
        int mx = me.GetXBoard(), my = me.GetYBoard();
        int tx = them.GetXBoard(), ty = them.GetYBoard();

        //Pick the best move via depth-limited search
        int bestScore = int.MinValue;
        int bestX = -1, bestY = -1;

        for (int k = 0; k < 8; k++)
        {
            int nx = mx + KnightDX[k];
            int ny = my + KnightDY[k];
            if (!InBounds(nx, ny, n) || occ[nx, ny]) continue;

            occ[nx, ny] = true;
            int score = -Negamax(occ, tx, ty, nx, ny, aiDepth - 1, n);
            occ[nx, ny] = false;

            if (score > bestScore)
            {
                bestScore = score;
                bestX = nx;
                bestY = ny;
            }
        }

        //No legal move -> the computer loses
        if (bestX == -1)
        {
            Winner(ComputerPlayer);
            return;
        }

        PerformMove(blackKnight, bestX, bestY);

        //If the human now has no move, end the game immediately for a nicer UX
        if (!HasAnyMove(tx, ty))
        {
            Winner("white");
        }
    }

    // Perform a move for a piece, mirroring MovePlate.OnMouseUp exactly so the
    // left-behind cell stays blocked (via the cross trail).
    public void PerformMove(GameObject piece, int nx, int ny)
    {
        Chessman cm = piece.GetComponent<Chessman>();

        SetPositionEmpty(cm.GetXBoard(), cm.GetYBoard());

        cm.SetXBoard(nx);
        cm.SetYBoard(ny);
        cm.SetCoords();

        SetPosition(piece);
        NextTurn();
    }

    private bool HasAnyMove(int x, int y)
    {
        for (int k = 0; k < 8; k++)
        {
            int nx = x + KnightDX[k];
            int ny = y + KnightDY[k];
            if (InBounds(nx, ny, boardSize) && positions[nx, ny] == null)
                return true;
        }
        return false;
    }

    private static bool InBounds(int x, int y, int n)
    {
        return x >= 0 && y >= 0 && x < n && y < n;
    }

    private static int Mobility(bool[,] occ, int x, int y, int n)
    {
        int count = 0;
        for (int k = 0; k < 8; k++)
        {
            int nx = x + KnightDX[k];
            int ny = y + KnightDY[k];
            if (InBounds(nx, ny, n) && !occ[nx, ny]) count++;
        }
        return count;
    }

    // Negamax from the point of view of the side to move (at meX,meY).
    private int Negamax(bool[,] occ, int meX, int meY, int themX, int themY, int depth, int n)
    {
        //Gather this side's moves
        int myMoves = 0;
        for (int k = 0; k < 8; k++)
        {
            int nx = meX + KnightDX[k];
            int ny = meY + KnightDY[k];
            if (InBounds(nx, ny, n) && !occ[nx, ny]) myMoves++;
        }

        //No move -> this side loses. Prefer losing later / winning sooner.
        if (myMoves == 0) return -100000 - depth;

        if (depth <= 0)
        {
            return Mobility(occ, meX, meY, n) - Mobility(occ, themX, themY, n);
        }

        int best = int.MinValue;
        for (int k = 0; k < 8; k++)
        {
            int nx = meX + KnightDX[k];
            int ny = meY + KnightDY[k];
            if (!InBounds(nx, ny, n) || occ[nx, ny]) continue;

            occ[nx, ny] = true;
            int val = -Negamax(occ, themX, themY, nx, ny, depth - 1, n);
            occ[nx, ny] = false;

            if (val > best) best = val;
        }
        return best;
    }
}
