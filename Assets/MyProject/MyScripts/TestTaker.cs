using System;

// Changed 2026-09-16 by Claude Code: added [Serializable], without which neither
// JsonUtility nor the old BinaryFormatter could write this type at all, and dropped
// the unused UnityEngine / SceneManagement / IO / Generic usings.
[Serializable]
public class TestTaker
{
    public string testerName;
    public int difficulty;

    // Stored as a percentage from 0 to 100, so the record stays meaningful even if
    // the number of questions per quiz changes.
    public int score;

    // Needed so JsonUtility can construct the object before filling its fields.
    public TestTaker() { }

    public TestTaker(string testerName = DefaultName)
    {
        // The start screen hands over whatever is in the name box, which is often
        // blank or whitespace if the player skipped it.
        this.testerName = string.IsNullOrWhiteSpace(testerName) ? DefaultName : testerName.Trim();
    }

    private const string DefaultName = "Scholar";
}
