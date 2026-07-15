using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showTex : MonoBehaviour
{
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        
        var position = FindObjectOfType<sliderScript>();
        text = GetComponent<Text>();
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
