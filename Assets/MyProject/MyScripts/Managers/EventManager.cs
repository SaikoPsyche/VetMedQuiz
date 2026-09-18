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

    // Added 2026-09-18 by Claude Code: the UI Toolkit buttons raise these instead of
    // holding a SoundManager reference. The old uGUI buttons called SoundManager
    // directly through UnityEvents, which is not available to a VisualElement.
    public static event Action OnPlayClick;
    public static event Action<bool> OnMusicMuted;
    public static event Action<bool> OnClickMuted;

    public static void StartQuiz() => OnStartQuiz?.Invoke();
    public static void AddTester(string name, int difficulty) => OnAddTester?.Invoke(name, difficulty);
    public static void CorrectAnswer(bool? correct) => OnCorrectAnswer?.Invoke(correct);
    public static void SaveData(List<TestTaker> testTakers) => OnSaveData?.Invoke(testTakers);
    public static void ToggleSettings() => OnToggleSettings?.Invoke();
    public static void PlayClick() => OnPlayClick?.Invoke();
    public static void SetMusicMuted(bool muted) => OnMusicMuted?.Invoke(muted);
    public static void SetClickMuted(bool muted) => OnClickMuted?.Invoke(muted);
}
