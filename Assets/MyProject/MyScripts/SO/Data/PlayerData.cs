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
        EndDisplayText = $"{PlayerName} earned a score of {PlayerScore}/{TOTAL_QUESTIONS}!";
    }
}
