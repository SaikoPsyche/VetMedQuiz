using System;
using System.IO;
using UnityEngine;

// Added 2026-09-16 by Claude Code.
//
// The single owner of playerData.json. Reading and writing used to be spread
// across TesterDataManager and QuizManager, and the two sides had drifted apart:
// TesterDataManager wrote the roster with a BinaryFormatter while QuizManager read
// it back with JsonUtility, so a file written by one could never be loaded by the
// other. Putting the path and the format in one place makes that mismatch
// impossible to reintroduce.
//
// BinaryFormatter is deliberately not used here. It is obsolete in current .NET
// and is unsafe to point at a file a user can edit.
public static class TesterDataStore
{
    private const string FileName = "playerData.json";

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    // Always returns a usable roster. A missing, empty, or unreadable file is not an
    // error worth stopping for; it just means nobody has taken a quiz yet.
    public static TestTakerRoster Load()
    {
        if (!File.Exists(FilePath)) return new TestTakerRoster();

        try
        {
            string json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json)) return new TestTakerRoster();

            TestTakerRoster roster = JsonUtility.FromJson<TestTakerRoster>(json);

            // A file written by an older build can parse without throwing and still
            // come back null, or with no list inside it.
            if (roster?.testTakers == null) return new TestTakerRoster();

            return roster;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not read {FilePath}, starting a new roster. {ex.Message}");
            return new TestTakerRoster();
        }
    }

    public static void Save(TestTakerRoster roster)
    {
        if (roster == null) return;

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(roster, true));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not write {FilePath}. {ex.Message}");
        }
    }

    // The tester currently taking a quiz is the most recently added one. Returns
    // null when nobody has been added yet, which callers are expected to handle.
    public static TestTaker CurrentTester()
    {
        return MostRecent(Load());
    }

    public static TestTaker MostRecent(TestTakerRoster roster)
    {
        int count = roster?.testTakers?.Count ?? 0;

        return count == 0 ? null : roster.testTakers[count - 1];
    }
}
