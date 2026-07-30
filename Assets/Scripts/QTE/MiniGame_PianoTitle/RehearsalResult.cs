/// <summary>
/// Immutable summary of a completed rehearsal. Produced by the mini-game and consumed
/// by the outer game layer (e.g. converted into a Music stat gain). The mini-game does
/// not know about artists or stats; it only reports how well the rehearsal went.
/// </summary>
public struct RehearsalResult
{
    /// <summary>Number of notes hit within the perfect window.</summary>
    public int PerfectCount;

    /// <summary>Number of notes hit within the good window.</summary>
    public int GoodCount;

    /// <summary>Number of notes missed (never hit or hit far off).</summary>
    public int MissCount;

    /// <summary>Total number of notes in the track.</summary>
    public int TotalNotes;

    /// <summary>Points earned this rehearsal.</summary>
    public int Score;

    /// <summary>Maximum points achievable (all perfect).</summary>
    public int MaxScore;

    /// <summary>Normalized success ratio from 0 (all missed) to 1 (all perfect).</summary>
    public float Accuracy;
}
