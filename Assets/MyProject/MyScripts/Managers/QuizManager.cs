// ----------------------------------------------------------------------------
// Change history
//
// 2026-09-16 - Claude Code
//   * questionsPerQuiz   Added. Serialized quiz length (default 10) replacing a
//                        hard coded question count. Clamped to the bank size.
//   * BuildQuestionSet   Added. Draws this run's questions at random from the
//                        full bank and is called once, from Awake.
//   * ShowQuestions      Changed. No longer re-parses the JSON asset on every
//                        question; it only displays the question already drawn.
//   * CheckAnswer        Changed. Added a bounds guard for a failed load.
//   * CalculateScorePercent
//                        Added. Replaces a hard coded "score * 10" percentage.
//   * BuildQuizTimer     Added. Sets the clock from the sum of the drawn
//                        questions' secondsToAnswer, plus a cushion, rounded to
//                        the nearest 30s, instead of one fixed time per quiz.
//   * ShowQuestionTime   Changed. Now renders minutes as well as seconds, which
//                        it has to once a quiz can run past a minute.
//   * LoadQuizDifficulty Changed. Reads the saved tester through TesterDataStore,
//                        and falls back to the student bank when there is none
//                        instead of loading no questions at all.
//   * SaveFinalScore     Changed. Was unreachable in the normal case and wrote
//                        nothing; now records the score and runs exactly once.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private GameObject quiz;
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TextMeshProUGUI questionText;

    // Changed 2026-09-16 by Claude Code: BuildQuizTimer overwrites this at startup
    // with the time the drawn questions are actually expected to need. The value
    // set in the inspector is only used as a fallback if no questions load.
    [SerializeField] private float questionTime;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button[] answerButtons;
    // Added 2026-09-16 by Claude Code: the quiz length used to be implied by the
    // size of the JSON file. Now that the banks hold more than one quiz worth of
    // questions, the length is set here and the questions are drawn at random.
    [SerializeField] private int questionsPerQuiz = DefaultQuestionsPerQuiz;

    // Added 2026-09-16 by Claude Code: slack added on top of the summed question
    // times, to cover reading the screen and settling in at the start of a run.
    [SerializeField] private float timeCushionSeconds = DefaultTimeCushionSeconds;

    // Added 2026-09-16 by Claude Code: the clock is rounded to a multiple of this
    // so players start on a round number rather than an arbitrary 4m55s.
    [SerializeField] private float timerRoundingSeconds = DefaultTimerRoundingSeconds;

    private const int DefaultQuestionsPerQuiz = 10;
    private const float DefaultTimeCushionSeconds = 15f;
    private const float DefaultTimerRoundingSeconds = 30f;

    // Used only if a question carries no secondsToAnswer, so an older or hand
    // edited bank cannot leave the quiz with almost no time on the clock.
    private const int FallbackSecondsPerQuestion = 20;

    private TextAsset _vetMedText;

    // Changed 2026-09-16 by Claude Code: was a QuizData holding the entire bank.
    // This is now only the questions drawn for this run.
    private List<QuizQuestion> _questions = new List<QuizQuestion>();
    private int score;
    private int _currentQuestionIndex;

    // Added 2026-09-16 by Claude Code: QuestionTimer runs from Update, so once the
    // clock hit zero it called SaveFinalScore on every frame, rewriting the save
    // file sixty-odd times a second. This latches the end of the quiz.
    private bool _quizFinished;

    private void Awake()
    {
        LoadQuizDifficulty();

        // Added 2026-09-16 by Claude Code: draw the questions once, up front,
        // then size the clock to the questions that were actually drawn.
        BuildQuestionSet();
        BuildQuizTimer();
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

    // Changed 2026-09-16 by Claude Code: this read the save file by hand, parsing a
    // whole roster as a single TestTaker, and left _vetMedText null whenever the file
    // was missing or unreadable, which meant no questions loaded at all. It now reads
    // through TesterDataStore and always ends up with a bank, so opening the quiz
    // scene on its own gives a playable quiz instead of an empty one.
    private void LoadQuizDifficulty()
    {
        TestTaker tester = TesterDataStore.CurrentTester();

        switch (tester?.difficulty)
        {
            case 1:
                _vetMedText = Resources.Load<TextAsset>("VetMedQuestions_VetTech");
                break;
            default:
                _vetMedText = Resources.Load<TextAsset>("VetMedQuestions");
                break;
        }
    }

    // Added 2026-09-16 by Claude Code.
    //
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

    // Added 2026-09-16 by Claude Code.
    //
    // The quiz clock is the sum of the drawn questions' own estimates plus a
    // cushion, so a run of short recall questions gets less time than a run that
    // happens to deal several dosage calculations. The result is rounded to a
    // round number of seconds, because starting on 3m00s reads better to a player
    // than starting on the 2m55s the estimates happen to add up to.
    private void BuildQuizTimer()
    {
        // Nothing was drawn, so leave the inspector value as the fallback.
        if (_questions.Count == 0) return;

        float total = 0f;

        foreach (QuizQuestion question in _questions)
        {
            total += question.secondsToAnswer > 0
                ? question.secondsToAnswer
                : FallbackSecondsPerQuestion;
        }

        questionTime = RoundToNearest(total + timeCushionSeconds, timerRoundingSeconds);
    }

    // Added 2026-09-16 by Claude Code.
    //
    // Rounding can only ever take the cushion back, never bite into the questions'
    // own estimates: the most it can subtract is half the step, and the step is
    // twice the cushion.
    private float RoundToNearest(float seconds, float step)
    {
        if (step <= 0f) return seconds;

        return Mathf.Round(seconds / step) * step;
    }

    // Changed 2026-09-16 by Claude Code: this used to call JsonUtility.FromJson on
    // every question, which re-read the whole asset each time and would have
    // re-rolled the random draw between questions. Loading and drawing now happen
    // once in BuildQuestionSet, and this only displays what was already drawn.
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
        // Find the current question
        // Guard added 2026-09-16 by Claude Code: without it, clicking an answer
        // after a failed load threw a NullReferenceException.
        if (_currentQuestionIndex >= _questions.Count) return;

        var currentQuestion = _questions[_currentQuestionIndex];

        // If the answer index given is the same as the current question's current answer index,
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

        // If the current question index exceeds the total number of questions,
        // show the end screen with the users name and score.
        else
        {
            SaveFinalScore();
        }
    }

    // Added 2026-09-16 by Claude Code, replacing "score * 10" in SaveFinalScore.
    //
    // Percent is derived from the number of questions actually asked, so the
    // question banks can grow without the final score going over 100%.
    private int CalculateScorePercent()
    {
        int totalQuestions = _questions?.Count ?? 0;

        if (totalQuestions <= 0) return 0;

        return Mathf.RoundToInt((float)score / totalQuestions * 100f);
    }

    // Changed 2026-09-16 by Claude Code: this showed only "00:" plus the seconds
    // within the current minute, so a quiz timed at 3m52s displayed "00:52secs".
    // Quizzes now run past a minute, so the minutes have to be shown.
    private void ShowQuestionTime()
    {
        int remaining = Mathf.Max(0, Mathf.CeilToInt(questionTime));
        int minutes = remaining / 60;
        int seconds = remaining % 60;

        // Update the UI Text to display the remaining time
        timerText.text = $"{minutes:00}:{seconds:00} secs";
    }

    // Changed 2026-09-16 by Claude Code: added the _quizFinished latch and clamped
    // the clock at zero, so time running out ends the quiz once rather than calling
    // SaveFinalScore on every frame from then on.
    private void QuestionTimer()
    {
        if (_quizFinished) return;

        if (questionTime > 0)
        {
            questionTime -= Time.deltaTime;

            if (questionTime < 0) questionTime = 0;

            ShowQuestionTime();
        }
        else
        {
            ShowQuestionTime();
            SaveFinalScore();
        }
    }

    // Changed 2026-09-16 by Claude Code. As written this method could not do its
    // job: when a save file existed it tried to BinaryFormatter.Deserialize a file
    // that was meant to hold JSON, threw, and was swallowed by the catch, leaving
    // scoreText untouched. The score was only ever displayed by the else branch,
    // which runs when there is NO save file, so the display was inverted and the
    // score was never written back. The percentage also came from "score * 10",
    // which was only correct while every quiz was exactly 10 questions long.
    private void SaveFinalScore()
    {
        // Reachable from both the last answer and the clock expiring, and the end
        // screen should only be raised once.
        if (_quizFinished) return;

        _quizFinished = true;

        int percent = CalculateScorePercent();

        TestTakerRoster roster = TesterDataStore.Load();
        TestTaker tester = TesterDataStore.MostRecent(roster);

        if (tester != null)
        {
            tester.score = percent;
            EventManager.SaveData(roster.testTakers);

            scoreText.text = $"{tester.testerName} earned a score of {percent}%!";
        }
        else
        {
            // Nobody came through the start screen, so there is no name to show.
            scoreText.text = $"Player earned a score of {percent}%!";
        }

        // Show Final Score Screen and Corrections
        endGameScreen.SetActive(true);
        quiz.SetActive(false);
    }
}
