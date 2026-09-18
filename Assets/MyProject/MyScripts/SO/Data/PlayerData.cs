using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string PlayerName;
    public int PlayerLevel;
    public int PlayerScore;
    const int TOTAL_QUESTIONS = 10;
    public string EndDisplayText;

    public void UpdateEndDisplayText()
    {
        UpdateEndDisplayText(TOTAL_QUESTIONS);
    }

    // Added 2026-09-18 by Claude Code: QuizManager.questionsPerQuiz is settable and
    // clamps to the size of the bank, so the quiz is not always ten questions long.
    // Taking the total from the caller keeps the end text honest when it is not.
    public void UpdateEndDisplayText(int totalQuestions)
    {
        if (totalQuestions <= 0) totalQuestions = TOTAL_QUESTIONS;

        string who = string.IsNullOrWhiteSpace(PlayerName) ? "Player" : PlayerName;

        EndDisplayText = $"{who} earned a score of {PlayerScore}/{totalQuestions}!";
    }
}
