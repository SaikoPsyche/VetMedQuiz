using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    // Removed 2026-09-18 by Claude Code: Start used to additively load the Settings
    // scene at index 2. That scene no longer exists - its UI is now the Settings
    // Document inside this one - so the load ran on every launch and failed. The
    // settings panel is shown by UIManager in response to EventManager.ToggleSettings.

    // Unreachable as of 2026-09-18 (Claude Code): with all three screens merged into
    // this scene there is no scene 0 to return to, and nothing calls this any more.
    // Screen changes go through UIManager. Kept rather than deleted in case it is
    // wanted for a future multi-scene layout - it will not work as written.
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

    // Unreachable as of 2026-09-18 (Claude Code): same as HomeScreen. There is no
    // scene at buildIndex + 1 any more; UIManager shows the level screen instead.
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
