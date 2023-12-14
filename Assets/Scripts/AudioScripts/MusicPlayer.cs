using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public abstract class MusicPlayer : MonoBehaviour
{

    protected AudioSettings audioSettings;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        LocateSettings();
        ChangeVolume(audioSettings.Loudness);
    }


    protected void LocateSettings()
    {
        audioSettings = GameObject.Find(AUDIOPATH).GetComponent<AudioSettings>();
    }


    public abstract void ChangeVolume(float newVolume);
}
