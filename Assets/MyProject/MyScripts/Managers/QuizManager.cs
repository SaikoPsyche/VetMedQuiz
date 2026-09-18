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
//
// 2026-09-18 - Claude Code
//   * Ported off uGUI. The Button[] / TextMeshProUGUI / SetActive fields are gone;
//     UIManager owns the views and this owns the quiz. The logic below - the draw,
//     the timer, the scoring - is unchanged by the port.
//   * BeginQuiz         Added, replacing the work Awake used to do. All three
//                       scenes are now one, so Awake ran before the player had
//                       chosen a difficulty: the bank was picked from whatever was
//                       last saved to disk and the clock started during the
//                       welcome screen. The quiz now starts when a level is
//                       chosen, and the clock only runs while it is running.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    // Changed 2026-09-16 by Claude Code: BuildQuizTimer overwrites this at startup
    // with the time the drawn questions are actually expected to need. The value
    // set in the inspector is only used as a fallback if no questions load.
    [SerializeField] private float questionTime;
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

    private const string PlayerAssetName = "Player";

    private UIManager _ui;
    private PlayerData _player;
    private TextAsset _vetMedText;

    // Added 2026-09-18 by Claude Code: the clock must not tick on the welcome or
    // level screens, which share this scene now.
    private bool _quizRunning;

    // Changed 2026-09-16 by Claude Code: was a QuizData holding the entire bank.
    // This is now only the questions drawn for this run.
    private List<QuizQuestion> _questions = new List<QuizQuestion>();
    private int score;
    private int _currentQuestionIndex;

    // Added 2026-09-16 by Claude Code: QuestionTimer runs from Update, so once the
    // clock hit zero it called SaveFinalScore on every frame, rewriting the save
    // file sixty-odd times a second. This latches the end of the quiz.
    private bool _quizFinished;

    // Changed 2026-09-18 by Claude Code: Awake used to load a bank, draw the
    // questions and start the clock. With one scene that all happened before the
    // player had picked anything, so it now only resolves collaborators and waits
    // for BeginQuiz.
    private void Awake()
    {
        _ui = GetComponent<UIManager>();

        if (_ui == null)
            Debug.LogError("QuizManager expects a UIManager on the same GameObject.");

        _player = Resources.Load<PlayerData>(PlayerAssetName);
    }

    private void Update()
    {
        if (_quizRunning) QuestionTimer();
    }

    // Added 2026-09-18 by Claude Code. Called by UIManager once a difficulty is
    // chosen, which is the first moment both the bank and the player are known.
    public void BeginQuiz(int difficulty)
    {
        score = 0;
        _currentQuestionIndex = 0;
        _quizFinished = false;

        LoadQuizDifficulty(difficulty);
        BuildQuestionSet();
        BuildQuizTimer();

        _quizRunning = _questions.Count > 0;

        ShowQuestionTime();
        ShowQuestions();
    }

    // Changed 2026-09-18 by Claude Code: the difficulty now arrives from the button
    // that was pressed rather than being read back out of the save file, which only
    // worked while the start screen was a separate scene loaded earlier.
    private void LoadQuizDifficulty(int difficulty)
    {
        _vetMedText = difficulty == 1
            ? Resources.Load<TextAsset>("VetMedQuestions_VetTech")
            : Resources.Load<TextAsset>("VetMedQuestions");
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

        _ui?.ShowQuestion(currentQuestion.question, OptionsOf(currentQuestion));
    }

    // Changed 2026-09-18 by Claude Code: replaces ShowAnswers, which wrote into four
    // uGUI buttons. Options C and D are absent on True/False questions, and come
    // back as empty entries that UIManager takes out of the layout.
    private static IReadOnlyList<string> OptionsOf(QuizQuestion question)
    {
        AnswerOptions answers = question.answers;

        return new[]
        {
            answers != null ? answers.A : null,
            answers != null ? answers.B : null,
            answers != null ? answers.C : null,
            answers != null ? answers.D : null,
        };
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

        // Changed 2026-09-18 by Claude Code: writes to the Timer label in
        // QuizScreen.uxml, whose own default is "00:00", so the "secs" suffix the
        // uGUI version carried is dropped to match.
        _ui?.ShowTime($"{minutes:00}:{seconds:00}");
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
    // Changed 2026-09-18 by Claude Code. The end text is no longer written straight
    // into a label: the Score label in End Document.uxml is data-bound to
    // PlayerData.EndDisplayText, so updating the ScriptableObject drives the display.
    // The roster on disk is still written, because a ScriptableObject does not
    // persist its runtime values in a build.
    private void SaveFinalScore()
    {
        // Reachable from both the last answer and the clock expiring, and the end
        // screen should only be raised once.
        if (_quizFinished) return;

        _quizFinished = true;
        _quizRunning = false;

        int answered = _questions.Count;
        int percent = CalculateScorePercent();

        if (_player != null)
        {
            _player.PlayerScore = score;
            _player.UpdateEndDisplayText(answered);
        }

        TestTakerRoster roster = TesterDataStore.Load();
        TestTaker tester = TesterDataStore.MostRecent(roster);

        if (tester != null)
        {
            tester.score = percent;
            EventManager.SaveData(roster.testTakers);
        }

        _ui?.ShowEndScreen();
    }
}
