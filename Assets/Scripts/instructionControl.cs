using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class instructionControl : MonoBehaviour
{
    [SerializeField] GameObject Rules;
   
    [SerializeField] GameObject canvas;


    public void onRulePage()
    {
        canvas.SetActive(false);
        Rules.SetActive(true);
    
    }

    public void onTapBack()
    {
        Rules.SetActive(false);
        canvas.SetActive(true);
    }

}
