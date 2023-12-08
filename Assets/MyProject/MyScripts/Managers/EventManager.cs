using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnStartQuiz;
    public static event Action<string> OnAddTester;
    public static event Action<int> OnSetDifficulty;
    public static event Action<bool> OnCorrectAnswer;
    public static event Action<TestTaker> OnSaveData;

    public static void StartQuiz() => OnStartQuiz?.Invoke();
    public static void AddTester(string name) => OnAddTester?.Invoke(name);
    public static void SetDifficulty(int difficulty) => OnSetDifficulty?.Invoke(difficulty);
    public static void CorrectAnswer(bool correct) => OnCorrectAnswer?.Invoke(correct);
    public static void SaveData(TestTaker testTaker) => OnSaveData?.Invoke(testTaker);
}
