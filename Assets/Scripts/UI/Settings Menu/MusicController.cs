using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [SerializeField] private float sliderValue;

    public void Start()
    {
        slider.value = PlayerPrefs.GetFloat("musicSliderValue");
    }

    public void ChangeSlider(float value)
    {
        sliderValue = value;
        PlayerPrefs.SetFloat("musicSliderValue", sliderValue);
    }
}
