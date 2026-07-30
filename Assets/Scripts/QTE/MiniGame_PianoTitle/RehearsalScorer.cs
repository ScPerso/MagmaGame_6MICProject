/// <summary>
/// Accumulates note judgements and turns them into a <see cref="RehearsalResult"/>.
/// Kept as a plain class (no Unity dependency) so the scoring rules stay isolated and
/// easy to tune or test. Perfect hits are weighted far above good ones to reward timing.
/// </summary>
public class RehearsalScorer
{
    /// <summary>Points granted for a Perfect hit.</summary>
    public const int PerfectScore = 100;

    /// <summary>Points granted for a Good hit.</summary>
    public const int GoodScore = 30;

    /// <summary>Points granted for a Miss.</summary>
    public const int MissScore = 0;

    private int perfectCount;
    private int goodCount;
    private int missCount;
    private int score;

    /// <summary>Points accumulated so far this rehearsal.</summary>
    public int CurrentScore => score;

    /// <summary>
    /// Registers a single note judgement.
    /// </summary>
    /// <param name="result">The judgement to add to the running totals.</param>
    public void Register(JudgementResult result)
    {
        switch (result)
        {
            case JudgementResult.Perfect:
                perfectCount++;
                score += PerfectScore;
                break;

            case JudgementResult.Good:
                goodCount++;
                score += GoodScore;
                break;

            default:
                missCount++;
                score += MissScore;
                break;
        }
    }

    /// <summary>
    /// Clears all accumulated totals for a fresh rehearsal.
    /// </summary>
    public void Reset()
    {
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;
        score = 0;
    }

    /// <summary>
    /// Builds the final result from the accumulated judgements.
    /// </summary>
    /// <returns>The rehearsal summary.</returns>
    public RehearsalResult BuildResult()
    {
        int totalNotes = perfectCount + goodCount + missCount;
        int maxScore = totalNotes * PerfectScore;

        return new RehearsalResult
        {
            PerfectCount = perfectCount,
            GoodCount = goodCount,
            MissCount = missCount,
            TotalNotes = totalNotes,
            Score = score,
            MaxScore = maxScore,
            Accuracy = maxScore > 0 ? (float)score / maxScore : 0f
        };
    }
}
