using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneBack : MonoBehaviour
{
    public void playGame()
    {
        Destroy(FindObjectOfType<sliderScript>());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    }
}
