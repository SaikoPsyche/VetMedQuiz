using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Load Settings Additively to the current scene.
        LoadSettingsScene();
    }

    private void LoadSettingsScene()
    {
        try
        {
            int buildIndex = SceneManager.GetSceneByName("Settings").buildIndex;
            SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + " ," + ex.StackTrace);
        }
    }

    // Go to the first scene
    public void HomeScreen()
    {
        try
        {
            SceneManager.LoadScene(0);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + " ," + ex.StackTrace);
        }
        
    }


    // Get the build Index of the active scene then load that scene to restart scene.
    public void Restart()
    {
        try
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(buildIndex);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + " ," + ex.StackTrace);
        }
        
    }

    // Get the build Index of the active scene (Start Screen SCene) then add 1 and load that scene to start the quiz.
    public void StartGame()
    {
        try
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(buildIndex);

            EventManager.StartQuiz();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + " ," + ex.StackTrace);
        }
        
    }

    // Added 2026-09-16 by Claude Code: the settings button in StartScreen.unity is
    // wired to GameManager.Settings(), but no such method existed anywhere in the
    // project, so Unity dropped the call and the button did nothing.
    //
    // This does not load the Settings scene: LoadSettingsScene already adds it
    // additively from Start, so the panel is present and only needs toggling. The
    // toggle goes through EventManager because the panel lives in a different scene
    // and cannot be wired up by inspector reference from this one.
    public void Settings()
    {
        EventManager.ToggleSettings();
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
