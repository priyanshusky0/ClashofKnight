using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sliderScript : MonoBehaviour
{
    public Slider slider;
    public int value;

    public Text sliderText;

    private void Awake()
    {
       
        DontDestroyOnLoad(this.gameObject);

    }

    private void Start()
    {
        if (slider != null)
        {
            UpdateText(slider.value);
            slider.onValueChanged.AddListener(UpdateText);
        }
        
    }

    void UpdateText(float val)
    {
        sliderText.text = slider.value.ToString();
        value = (int)slider.value;
    }
}
