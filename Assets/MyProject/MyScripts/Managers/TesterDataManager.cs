using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class TesterDataManager : MonoBehaviour
{
    private TestTaker tester;

    private void OnEnable()
    {
        EventManager.OnAddTester += AddTester;
        EventManager.OnSaveData += SaveTesterData;
    }

    public void SaveTesterData(TestTaker testTaker)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(testTaker);
            string filePath = Application.persistentDataPath + "/playerData.json";
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Saved: {jsonData} at {filePath}");
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message + ", " + ex.StackTrace);
        }
    }

    public void AddTester(string testerName)
    {
        tester = new TestTaker
        {
            level = SceneManager.GetActiveScene().buildIndex,
            testerName = testerName,
            score = 0
        };

        SaveTesterData(tester);
    }

    private void OnDisable()
    {
        EventManager.OnAddTester -= AddTester;
        EventManager.OnSaveData -= SaveTesterData;
    }
}
