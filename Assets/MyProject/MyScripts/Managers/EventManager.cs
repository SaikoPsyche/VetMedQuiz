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

    public static void StartQuiz() => OnStartQuiz?.Invoke();
    public static void AddTester(string name, int difficulty) => OnAddTester?.Invoke(name, difficulty);
    public static void CorrectAnswer(bool? correct) => OnCorrectAnswer?.Invoke(correct);
    public static void SaveData(List<TestTaker> testTakers) => OnSaveData?.Invoke(testTakers);
}
