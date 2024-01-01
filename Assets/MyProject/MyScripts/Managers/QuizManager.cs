using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private GameObject quiz;
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private float questionTime;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button[] answerButtons;

    private TextAsset _vetMedText;
    QuizData _quizQuestions;
    private int score;
    private int _currentQuestionIndex;

    private void Awake()
    {
        LoadQuizDifficulty();
    }

    // Start is called before the first frame update
    void Start()
    {
        ShowQuestions();
    }

    private void Update()
    {
        QuestionTimer();
    }

    private void LoadQuizDifficulty()
    {
        string filePath = Application.persistentDataPath + "/playerData.json";
        

        if (File.Exists(filePath))
        {
            try
            {
                string savedJsonData = File.ReadAllText(filePath);

                if (!string.IsNullOrEmpty(savedJsonData))
                {
                    TestTaker tester = JsonUtility.FromJson<TestTaker>(savedJsonData);
                    
                    switch (tester.difficulty)
                    {
                        case 0:
                            _vetMedText = Resources.Load<TextAsset>("VetMedQuestions");
                            break;
                        case 1:
                            _vetMedText = Resources.Load<TextAsset>("VetMedQuestions_VetTech");
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ", " + ex.StackTrace);
            }
        }
    }

    private void ShowQuestions()
    {
        if (_vetMedText != null)
        {
            _quizQuestions = JsonUtility.FromJson<QuizData>(_vetMedText.text);

            QuizQuestion currentQuestion = _quizQuestions.questions[_currentQuestionIndex];

            questionText.text = currentQuestion.question;

            ShowAnswers();
        }
        else
        {
            Debug.LogError("Unable to load JSON file.");
        }
    }

    private void ShowAnswers()
    {
        var currentQuestion = _quizQuestions.questions[_currentQuestionIndex];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                switch (i)
                {
                    case 0:
                        answerButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers.A;
                        break;
                    case 1:
                        answerButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers.B;
                        break;
                    case 2:
                        answerButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers.C;
                        break;
                    case 3:
                        answerButtons[3].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers.D;
                        break;
                    default:
                        answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                        break;
                }
            }
            else
            {
                Debug.LogError($"Answer Button {answerButtons[i]} can not be found.");
            }
        }
    }

    public void CheckAnswer(int answerIndex)
    {
        // Find the cureent question
        var currentQuestion = _quizQuestions.questions[_currentQuestionIndex];

        // If the answer index given is the same as the ccurrent question's current answer index,
        // increment the score.
        if (answerIndex == currentQuestion.correctAnswerIndex) score++;

        // Play click sound based on answer choice.
        bool isCorrect = answerIndex == currentQuestion.correctAnswerIndex;
        EventManager.CorrectAnswer(isCorrect);


        // Increment the current question index.
        _currentQuestionIndex++;

        // If the current question index is less than the total number of questions,
        // show the next question.
        if (_currentQuestionIndex < _quizQuestions.questions.Count) ShowQuestions();

        // If the current question index exeeds the total number of questions,
        // show the end screen with the users name and score.
        else
        {
            SaveFinalScore();
        }
    }

    private void ShowQuestionTime()
    {
        int seconds = Mathf.FloorToInt(questionTime % 60f);

        // Update the UI Text to display the remaining time
        timerText.text = "00:" + seconds + "secs";
    }

    private void QuestionTimer()
    {
        if (questionTime > 0)
        {
            questionTime -= Time.deltaTime;

            /*if (questionTime < 0)
                questionTime = 0;*/

            ShowQuestionTime();
        }
        else
        {
            SaveFinalScore();
        }
    }

    private void SaveFinalScore()
    {
        string filePath = Application.persistentDataPath + "/playerData.json";

        if (File.Exists(filePath))
        {
            try
            {
                /*string savedJsonData = File.ReadAllText(filePath);

                if (!string.IsNullOrEmpty(savedJsonData))
                {
                    TestTaker tester = JsonUtility.FromJson<TestTaker>(savedJsonData);

                    tester.score = this.score;
                    EventManager.SaveData(tester);

                    scoreText.text = $"{tester.testerName} earned a score of {score * 10}%!"; // score out of 10 questions * 10 gives percent.

                    savedJsonData = File.ReadAllText(filePath);
                    Debug.Log("Loaded: " + savedJsonData);
                }*/

                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(filePath, FileMode.Open);

                List<TestTaker> testTakers = (List<TestTaker>)formatter.Deserialize(fileStream);
                fileStream.Close();


            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ", " + ex.StackTrace);
            }

        }
        else
            scoreText.text = $"Player earned a score of {score * 10}%!";

        // Show Final Score Screen and Corrections
        endGameScreen.SetActive(true);
        quiz.SetActive(false);
    }
}
