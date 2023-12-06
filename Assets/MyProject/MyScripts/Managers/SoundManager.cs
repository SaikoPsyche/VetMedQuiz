using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgMusic;
    [SerializeField] private AudioSource clickSound;

    public void PlayClickSound()
    {
        clickSound.Play();
    }

    public void MuteClickSound()
    {
        clickSound.mute = true;
    }

    public void MuteBGAudio()
    {
        bgMusic.mute = true;
    }
}
