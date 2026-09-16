[System.Serializable]
public class QuizQuestion
{
    public string question;
    public AnswerOptions answers;
    public int correctAnswerIndex;

    // Added 2026-09-16 by Claude Code: how long this one question is expected to
    // take to read and answer. QuizManager sums these across the questions it
    // draws to set the clock for the run, so a quiz of harder questions is
    // given proportionally more time.
    public int secondsToAnswer;
}
