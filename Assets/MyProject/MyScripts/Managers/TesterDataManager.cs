using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class TesterDataManager : MonoBehaviour
{
    private List<TestTaker> testTakers;
    private string savePath;

    private void OnEnable()
    {
        EventManager.OnAddTester += AddTester;
        EventManager.OnSaveData += SaveTesterData;
    }

    /*public void SaveTesterData(TestTaker testTakers)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(testTakers);
            string filePath = Application.persistentDataPath + "/playerData.json";
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Saved: {jsonData} at {filePath}");
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message + ", " + ex.StackTrace);
        }
    }*/

    public void SaveTesterData(List<TestTaker> testers)
    {
        string filePath = Application.persistentDataPath + "/playerData.json";

        BinaryFormatter formatter = new();
        FileStream fileStream = new(filePath, FileMode.Create);

        formatter.Serialize(fileStream, testers);
        fileStream.Close();
    }

    public void AddTester(string testerName, int difficulty)
    {
        TestTaker tester = new(testerName)
        {
            difficulty = difficulty
        };
        testTakers.Add(tester);
        SaveTesterData(testTakers);
    }

    private void OnDisable()
    {
        EventManager.OnAddTester -= AddTester;
        EventManager.OnSaveData -= SaveTesterData;
    }
}
