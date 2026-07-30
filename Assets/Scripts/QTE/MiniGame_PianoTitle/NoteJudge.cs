using UnityEngine;

/// <summary>
/// Pure timing evaluation for the rehearsal mini-game.
/// It converts a timing error into a <see cref="JudgementResult"/> and has no
/// dependency on any visual, so it can be unit-tested and reused as-is.
/// </summary>
public static class NoteJudge
{
    /// <summary>
    /// Absolute timing error, in seconds, within which a hit is Perfect.
    /// This also defines the note's highlight (glow) window: a note glows exactly
    /// while it is within this window, so clicking during the glow yields a Perfect.
    /// The full glow duration is therefore twice this value.
    /// </summary>
    public const float PerfectWindowSeconds = 0.6f;

    /// <summary>
    /// Absolute timing error, in seconds, within which a hit is Good.
    /// Any error above this threshold is judged as a Miss.
    /// </summary>
    public const float GoodWindowSeconds = 1.0f;

    /// <summary>
    /// Judges a hit from the time between the input and the note target time.
    /// </summary>
    /// <param name="timingErrorSeconds">
    /// Signed or unsigned time difference, in seconds, between the input and the
    /// note target time. Only its magnitude is used.
    /// </param>
    /// <returns>The resulting judgement for the given timing error.</returns>
    public static JudgementResult Judge(float timingErrorSeconds)
    {
        float error = Mathf.Abs(timingErrorSeconds);

        if (error <= PerfectWindowSeconds)
        {
            return JudgementResult.Perfect;
        }

        if (error <= GoodWindowSeconds)
        {
            return JudgementResult.Good;
        }

        return JudgementResult.Miss;
    }

    /// <summary>
    /// Returns whether a note is still within a judgeable window for the given error.
    /// Useful to decide if a late note should be flagged as a definitive Miss.
    /// </summary>
    /// <param name="timingErrorSeconds">Time difference in seconds.</param>
    /// <returns>True while the note can still be hit for a Good or Perfect result.</returns>
    public static bool IsWithinHitWindow(float timingErrorSeconds)
    {
        return Mathf.Abs(timingErrorSeconds) <= GoodWindowSeconds;
    }
}
