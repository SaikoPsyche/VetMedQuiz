using System.Collections.Generic;
using UnityEngine;

// Changed 2026-09-16 by Claude Code. This class had three faults that together
// meant nothing was ever saved:
//
//   1. testTakers was declared but never assigned, so the first AddTester call
//      threw a NullReferenceException on List.Add.
//   2. SaveTesterData serialized with a BinaryFormatter, but TestTaker was not
//      marked [Serializable], so the write would have thrown even had it been
//      reached, and QuizManager read the file back as JSON regardless.
//   3. The roster was held in a field on a MonoBehaviour that exists in both
//      scenes, so moving from the start screen to the quiz created a fresh
//      instance with an empty list.
//
// The roster now lives on disk and is read through TesterDataStore whenever it is
// needed, which removes all three.
public class TesterDataManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.OnAddTester += AddTester;
        EventManager.OnSaveData += SaveTesterData;
    }

    private void OnDisable()
    {
        EventManager.OnAddTester -= AddTester;
        EventManager.OnSaveData -= SaveTesterData;
    }

    public void AddTester(string testerName, int difficulty)
    {
        TestTakerRoster roster = TesterDataStore.Load();

        roster.testTakers.Add(new TestTaker(testerName) { difficulty = difficulty });

        TesterDataStore.Save(roster);
    }

    public void SaveTesterData(List<TestTaker> testers)
    {
        if (testers == null) return;

        TesterDataStore.Save(new TestTakerRoster { testTakers = testers });
    }
}
