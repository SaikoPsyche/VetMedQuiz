using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject settings;

    // Added 2026-09-16 by Claude Code: the settings button in this scene calls
    // ShowSettings directly, but the one on the start screen cannot reach across
    // scenes, so it raises an event that this picks up.
    private void OnEnable()
    {
        EventManager.OnToggleSettings += ShowSettings;
    }

    private void OnDisable()
    {
        EventManager.OnToggleSettings -= ShowSettings;
    }

    public void ShowSettings()
    {
        settings.SetActive(!settings.activeInHierarchy);
    }

    /*public void DisplayVolumeValue()
    {
        bgMusic.volume = BGMusicValue;
        uiSounds[0].volume = UISoundValue;
        uiSounds[1].volume = UISoundValue;
        uiSounds[2].volume = UISoundValue;

        //float toWholeNumber = 100;

        // float BGFullValue = BGMusicValue * toWholeNumber;
        BGMusicValueText.text = BGMusicValue.ToString("000");

        // float UISoundFullValue = UISoundValue * toWholeNumber;
        UISoundValueText.text = UISoundValue.ToString("000");
    }

    public void BGVolumeUp()
    {
        BGMusicValue += increment;

        if (BGMusicValue >= 1)
        {
            BGMusicValue = 1;
        }

    }

    public void BGVolumeDown()
    {
        BGMusicValue -= increment;

        if (BGMusicValue <= 0)
        {
            bgMusic.mute = true;
        }
    }*/
}
