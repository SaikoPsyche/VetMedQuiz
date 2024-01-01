using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgAudio;
    [SerializeField] private AudioSource clickAudio;
    [SerializeField] private AudioSource correctAnsAudio;
    [SerializeField] private AudioSource incorrectAnsAudio;

    private void OnEnable()
    {
        EventManager.OnCorrectAnswer += PlayAnsAudio;
    }

    public void PlayClickAudio()
    {
        clickAudio.Play();
    }

    public void UnMuteClickAudio()
    {
        clickAudio.mute = false;
        correctAnsAudio.mute = false;
        incorrectAnsAudio.mute = false;
    }

    public void PlayBgAudio()
    {
        bgAudio.mute = false;
    }

    public void MuteClickAudio()
    {
        clickAudio.mute = true;
        correctAnsAudio.mute = true;
        incorrectAnsAudio.mute = true;
    }

    public void MuteBGAudio()
    {
        bgAudio.mute = true;
    }

    private void PlayAnsAudio(bool? isCorrect)
    {
        switch(isCorrect)
        {
            case true: 
                correctAnsAudio.Play(); 
                break;
            case false: 
                incorrectAnsAudio.Play(); 
                break;
        }
    }

    /*public static void MasterVolumeSlider(float value)
    {
        AudioListener.volume = value;
    }*/

    private void OnDisable()
    {
        EventManager.OnCorrectAnswer -= PlayAnsAudio;
    }
}
