using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject levelScreen;

    public void SaveData(int difficulty)
    {
        EventManager.AddTester(nameText.text, difficulty);
    }

    public void LoadLevelScreen()
    {
        levelScreen.SetActive(true);
    }
}
