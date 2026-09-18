using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgAudio;
    [SerializeField] private AudioSource clickAudio;
    [SerializeField] private AudioSource correctAnsAudio;
    [SerializeField] private AudioSource incorrectAnsAudio;

    private void OnEnable()
    {
        EventManager.OnCorrectAnswer += PlayAnsAudio;

        // Added 2026-09-18 by Claude Code: the UI Toolkit buttons cannot call these
        // directly the way the old uGUI UnityEvents did.
        EventManager.OnPlayClick += PlayClickAudio;
        EventManager.OnMusicMuted += SetMusicMuted;
        EventManager.OnClickMuted += SetClickMuted;
    }

    public void PlayClickAudio()
    {
        if (clickAudio != null) clickAudio.Play();
    }

    private void SetMusicMuted(bool muted)
    {
        if (muted) MuteBGAudio();
        else PlayBgAudio();
    }

    private void SetClickMuted(bool muted)
    {
        if (muted) MuteClickAudio();
        else UnMuteClickAudio();
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
        EventManager.OnPlayClick -= PlayClickAudio;
        EventManager.OnMusicMuted -= SetMusicMuted;
        EventManager.OnClickMuted -= SetClickMuted;
    }
}
