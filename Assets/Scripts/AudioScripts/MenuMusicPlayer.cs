using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusicPlayer : MusicPlayer
{
    //This class is responsible for playing music in menu scenes
    [SerializeField] private AudioSource music;




    public override void ChangeVolume(float newVolume)
    {
        music.volume = newVolume;
    }


}
