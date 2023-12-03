using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<TestTaker> OnSaveData;
    public static event Action<string> OnAddTester;
    public static event Action OnStartQuiz;

    public static void AddTester(string name) => OnAddTester?.Invoke(name);
    public static void SaveData(TestTaker testTaker) => OnSaveData?.Invoke(testTaker);

    public static void StartQuiz() => OnStartQuiz?.Invoke();
}
