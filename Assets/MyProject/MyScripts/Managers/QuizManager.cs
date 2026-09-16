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
    [SerializeField] private int questionsPerQuiz = DefaultQuestionsPerQuiz;

    private const int DefaultQuestionsPerQuiz = 10;

    private TextAsset _vetMedText;
    private List<QuizQuestion> _questions = new List<QuizQuestion>();
    private int score;
    private int _currentQuestionIndex;

    private void Awake()
    {
        LoadQuizDifficulty();
        BuildQuestionSet();
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

    // Draws this run's questions from the full bank. Called once, in Awake, so the
    // selection stays fixed for the rest of the quiz. Restarting reloads the scene,
    // which runs this again and deals a new set.
    private void BuildQuestionSet()
    {
        _questions = new List<QuizQuestion>();

        if (_vetMedText == null)
        {
            Debug.LogError("Unable to load JSON file.");
            return;
        }

        QuizData quizData = JsonUtility.FromJson<QuizData>(_vetMedText.text);

        if (quizData == null || quizData.questions == null || quizData.questions.Count == 0)
        {
            Debug.LogError($"No questions were found in {_vetMedText.name}.");
            return;
        }

        // Shuffle a copy so the loaded asset is never reordered.
        List<QuizQuestion> pool = new List<QuizQuestion>(quizData.questions);

        // A partial Fisher-Yates shuffle: only draw as many as this quiz needs.
        int wanted = questionsPerQuiz > 0 ? questionsPerQuiz : DefaultQuestionsPerQuiz;
        int drawCount = Mathf.Min(wanted, pool.Count);

        for (int i = 0; i < drawCount; i++)
        {
            int swapIndex = UnityEngine.Random.Range(i, pool.Count);

            QuizQuestion picked = pool[swapIndex];
            pool[swapIndex] = pool[i];
            pool[i] = picked;

            _questions.Add(picked);
        }
    }

    private void ShowQuestions()
    {
        if (_currentQuestionIndex >= _questions.Count) return;

        QuizQuestion currentQuestion = _questions[_currentQuestionIndex];

        questionText.text = currentQuestion.question;

        ShowAnswers();
    }

    private void ShowAnswers()
    {
        var currentQuestion = _questions[_currentQuestionIndex];

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
        if (_currentQuestionIndex >= _questions.Count) return;

        var currentQuestion = _questions[_currentQuestionIndex];

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
        if (_currentQuestionIndex < _questions.Count) ShowQuestions();

        // If the current question index exeeds the total number of questions,
        // show the end screen with the users name and score.
        else
        {
            SaveFinalScore();
        }
    }

    // Percent is derived from the number of questions actually asked, so the
    // question banks can grow without the final score going over 100%.
    private int CalculateScorePercent()
    {
        int totalQuestions = _questions?.Count ?? 0;

        if (totalQuestions <= 0) return 0;

        return Mathf.RoundToInt((float)score / totalQuestions * 100f);
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
            scoreText.text = $"Player earned a score of {CalculateScorePercent()}%!";

        // Show Final Score Screen and Corrections
        endGameScreen.SetActive(true);
        quiz.SetActive(false);
    }
}
