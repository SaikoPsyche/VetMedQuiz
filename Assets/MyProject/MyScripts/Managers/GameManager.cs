using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Go to the first scene
    public void HomeScreen()
    {
        SceneManager.LoadScene(0);
    }

    // Get the build Index of the active scene (Start Screen SCene) then add 1 and load that scene to start the quiz.
    public void StartGame()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(buildIndex);

        EventManager.StartQuiz();
    }

    // Get the build Index of the active scene then load that scene to restart scene.
    public void Restart()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(buildIndex);
    }



    public void CloseApp()
    {
        Application.Quit();
    }
}
