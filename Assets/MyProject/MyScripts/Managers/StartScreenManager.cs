using TMPro;
using UnityEngine;

// Changed 2026-09-16 by Claude Code. Neither button event on this screen resolved,
// so EventManager.AddTester was never called and nothing was ever saved:
//
//   * The Student and Vet Tech buttons call SetDifficulty(int), passing 0 and 1.
//     No such method existed on this class, or anywhere in the project, so Unity
//     silently dropped the call.
//   * The same buttons then call SaveData with no argument, but the only SaveData
//     here took an int. The signatures did not match, so that call was dropped too.
//
// The methods below match what StartScreen.unity already asks for, which fixes the
// wiring without touching the scene.
public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject levelScreen;

    private int _difficulty;
    private bool _difficultyChosen;

    // Wired to the Student (0) and Vet Tech (1) buttons, which fire this before
    // SaveData so the choice is known by the time the record is written.
    public void SetDifficulty(int difficulty)
    {
        _difficulty = difficulty;
        _difficultyChosen = true;
    }

    // Wired to the difficulty buttons, and also to the button that opens the
    // difficulty screen. On that first button no difficulty has been picked yet, so
    // there is nothing worth recording and this does nothing.
    public void SaveData()
    {
        if (!_difficultyChosen) return;

        EventManager.AddTester(TesterName(), _difficulty);
    }

    public void LoadLevelScreen()
    {
        levelScreen.SetActive(true);
    }

    private string TesterName()
    {
        if (nameText == null) return string.Empty;

        // TextMeshPro leaves a zero width space behind in an untouched field, which
        // is not whitespace as far as string.IsNullOrWhiteSpace is concerned and
        // would otherwise be saved as the player's name.
        return nameText.text?.Replace("​", string.Empty);
    }
}
