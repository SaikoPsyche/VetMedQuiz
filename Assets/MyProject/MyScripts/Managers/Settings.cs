using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject settings;

    public void ShowSettings()
    {
        bool isActive = settings.activeInHierarchy;

        switch (isActive)
        {
            case true:
                settings.SetActive(!isActive);
                break;
            case false:
                settings.SetActive(!isActive);
                break;
        }
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
    }

    public void UISoundVolumeUp()
    {
        UISoundValue += increment;

        if (UISoundValue >= 1)
        {
            UISoundValue = 1;
        }
    }

    public void UISoundVolumeDown()
    {
        UISoundValue -= increment;

        if (UISoundValue <= 0)
        {
            uiSounds[0].mute = true;
            uiSounds[1].mute = true;
            uiSounds[2].mute = true;
        }
    }*/
}
