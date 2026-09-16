using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnStartQuiz;
    public static event Action<string, int> OnAddTester;
    public static event Action<bool?> OnCorrectAnswer;
    public static event Action<List<TestTaker>> OnSaveData;

    // Added 2026-09-16 by Claude Code: lets any scene ask for the settings panel
    // without holding a reference to it. The Settings scene is loaded additively by
    // GameManager, so the panel is in the hierarchy but not reachable by inspector
    // reference from the scene the button lives in.
    public static event Action OnToggleSettings;

    public static void StartQuiz() => OnStartQuiz?.Invoke();
    public static void AddTester(string name, int difficulty) => OnAddTester?.Invoke(name, difficulty);
    public static void CorrectAnswer(bool? correct) => OnCorrectAnswer?.Invoke(correct);
    public static void SaveData(List<TestTaker> testTakers) => OnSaveData?.Invoke(testTakers);
    public static void ToggleSettings() => OnToggleSettings?.Invoke();
}
