using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chessman : MonoBehaviour
{
    
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    //Position for this Chesspiece on the Board
    //The correct position will be set later
    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    private string player;

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_knight;
    public Sprite white_knight;


    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();

        //Choose correct sprite based on piece's name
        switch (this.name)
        {
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
        }
    }

    public void SetCoords()
    {
        Game sc = controller.GetComponent<Game>();

        //Position and size are derived from the current board size so pieces
        //always land on the centre of their cell, whatever the board size.
        this.transform.position = sc.BoardToWorld(xBoard, yBoard, -1.0f);

        float f = sc.GetPieceScaleFactor();
        this.transform.localScale = new Vector3(3f * f, 3f * f, 1f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    public string GetPlayer()
    {
        return player;
    }

    private void OnMouseUp()
    {
        Game game = controller.GetComponent<Game>();
        bool canInteract = !game.IsGameOver() && game.GetCurrentPlayer() == player;

#if PHOTON_UNITY_NETWORKING
        //Online: you may only move your own colour
        if (Game.Online && PhotonNetworkManager.MyColor != player)
            canInteract = false;
#endif

        if (canInteract)
        {
            //Remove all moveplates relating to previously selected piece
            DestroyMovePlates();

            //Create new MovePlates
            InitiateMovePlates();
        }
    }

    public void DestroyMovePlates()
    {
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }

    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
        }
    }

    /*    public void LineMovePlate(int xIncrement, int yIncrement)
        {
            Game sc = controller.GetComponent<Game>();

            int x = xBoard + xIncrement;
            int y = yBoard + yIncrement;

            while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
            {
                MovePlateSpawn(x, y);
                x += xIncrement;
                y += yIncrement;
            }

            if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }*/

    private bool anyFreePositions = false;
    public void LMovePlate()
    {
        anyFreePositions = false;
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
        if (!anyFreePositions) {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>().Winner(player);

        }
    }

    /*    public void SurroundMovePlate()
        {
            PointMovePlate(xBoard, yBoard + 1);
            PointMovePlate(xBoard, yBoard - 1);
            PointMovePlate(xBoard - 1, yBoard + 0);
            PointMovePlate(xBoard - 1, yBoard - 1);
            PointMovePlate(xBoard - 1, yBoard + 1);
            PointMovePlate(xBoard + 1, yBoard + 0);
            PointMovePlate(xBoard + 1, yBoard - 1);
            PointMovePlate(xBoard + 1, yBoard + 1);
        }
    */
    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);               
            }
            /*else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }*/
        }
    }

  //spawn new piece

 

//spawn new piece

    /* public void PawnMovePlate(int x, int y)
     {
         Game sc = controller.GetComponent<Game>();
         if (sc.PositionOnBoard(x, y))
         {
             if (sc.GetPosition(x, y) == null)
             {
                 MovePlateSpawn(x, y);
             }

             if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null && sc.GetPosition(x + 1, y).GetComponent<Chessman>().player != player)
             {
                 MovePlateAttackSpawn(x + 1, y);
             }

             if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null && sc.GetPosition(x - 1, y).GetComponent<Chessman>().player != player)
             {
                 MovePlateAttackSpawn(x - 1, y);
             }
         }
     }*/

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        Game sc = controller.GetComponent<Game>();

        //Centre the move plate on its cell and scale it to match the board size
        GameObject mp = Instantiate(movePlate, sc.BoardToWorld(matrixX, matrixY, -3.0f), Quaternion.identity);

        float f = sc.GetPieceScaleFactor();
        mp.transform.localScale = new Vector3(3.5f * f, 3.5f * f, 1f);

        //Modern translucent highlight instead of a solid white square
        SpriteRenderer mpsr = mp.GetComponent<SpriteRenderer>();
        if (mpsr != null) mpsr.color = new Color(0.25f, 0.78f, 0.45f, 0.55f);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
        anyFreePositions = true;
    }

}

/*public void checkForRemove()
{
    if (x + y = 1)
    {
        Destroy(movePlate);
    }
}*/

/*    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
}*/
