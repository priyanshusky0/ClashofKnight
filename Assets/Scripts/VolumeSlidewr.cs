using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlidewr : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("musicVolumer"))
        {
            PlayerPrefs.SetFloat("musicVolumer", 1);
            load();
        }
        else
        {
            load();
        }
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void load()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolumer");
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("musicVolumer", volumeSlider.value);
    }
}
