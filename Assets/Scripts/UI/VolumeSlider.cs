using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UniversalConstants;

public class VolumeSlider : MonoBehaviour
{

    [SerializeField] private MusicPlayer music;
    [SerializeField] private Slider slider;
    private AudioSettings audioSettings;
    private bool ready = false;

    


    void Start()
    {
        audioSettings = GameObject.Find(AUDIOPATH).GetComponent<AudioSettings>();
        slider.value = audioSettings.Loudness;
        ready = true;
    }

    public void ChangeVolume(float newVolume)
    {
        if (ready)
        {
            music.ChangeVolume(newVolume);
            audioSettings.RecordNewVolume(newVolume);
        }
        
    }

    
}
