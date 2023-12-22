using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusicManager : MusicPlayer
{ 
    //This class is responsible for playing music during the main game
    //It plays a different song based on who's turn it is

    [SerializeField] private AudioSource playerMusic;
    [SerializeField] private AudioSource computerMusic;

    

    // Start is called before the first frame update
    /*
    protected override void Start()
    {
        LocateSettings();
        ChangeVolume(audioSettings.Loudness);
    }*/

    public void PlayPlayerMusic()
    {
        computerMusic.Stop();
        playerMusic.Play();
        
    }

    public void PlayComputerMusic()
    {
        playerMusic.Stop();
        computerMusic.Play();
    }

    public void StopMusic()
    {
        playerMusic.Stop();
        computerMusic.Stop();
    }

    public override void ChangeVolume(float newVolume)
    {
        playerMusic.volume = newVolume;
        computerMusic.volume = newVolume;
    }
}
