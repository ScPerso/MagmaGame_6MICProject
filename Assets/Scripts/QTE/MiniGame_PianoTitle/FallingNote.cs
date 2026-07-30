using UnityEngine;

/// <summary>
/// Runtime instance of a single note. It owns the timing math (progress along its
/// full fall, and whether it is inside its perfect window) and delegates the visuals
/// to swappable components: movement via <see cref="INoteMovement"/> and the glow via
/// an optional <see cref="INoteHighlight"/>. The glow is driven from the exact same
/// window as the Perfect judgement, so what the player sees matches what is scored.
/// </summary>
public class FallingNote : MonoBehaviour
{
    private NoteData noteData;
    private float spawnSongTime;
    private float totalTravelTime;
    private INoteMovement movement;
    private INoteHighlight highlight;

    /// <summary>Lane index this note travels through.</summary>
    public int LaneIndex => noteData.laneIndex;

    /// <summary>Song time, in seconds, at which the note is Perfect (centre of the glow).</summary>
    public float TargetTime => noteData.beatTime;

    /// <summary>Whether this note has already been hit or missed.</summary>
    public bool IsResolved { get; private set; }

    /// <summary>
    /// Marks the note as hit or missed so it can no longer be judged, and stops its glow.
    /// </summary>
    public void Resolve()
    {
        IsResolved = true;

        highlight?.SetHighlight(false);
    }

    /// <summary>
    /// Initializes the note and configures its movement endpoints.
    /// </summary>
    /// <param name="note">The chart data for this note.</param>
    /// <param name="spawnSongTime">Song time, in seconds, at which the note appears at the top.</param>
    /// <param name="totalTravelTime">Time, in seconds, for the full top-to-bottom fall.</param>
    /// <param name="topPosition">World position at the top of the column.</param>
    /// <param name="bottomPosition">World position at the bottom of the column.</param>
    public void Initialize(
        NoteData note,
        float spawnSongTime,
        float totalTravelTime,
        Vector3 topPosition,
        Vector3 bottomPosition
    )
    {
        noteData = note;
        this.spawnSongTime = spawnSongTime;
        this.totalTravelTime = totalTravelTime;

        movement = GetComponent<INoteMovement>();
        highlight = GetComponent<INoteHighlight>();

        if (movement == null)
        {
            Debug.LogError(
                "FallingNote requires a component implementing INoteMovement."
            );

            return;
        }

        movement.Configure(topPosition, bottomPosition);
        highlight?.SetHighlight(false);
    }

    /// <summary>
    /// Advances the note visual (fall and glow) to match the given song time.
    /// </summary>
    /// <param name="songTime">Current song time in seconds.</param>
    public void Tick(float songTime)
    {
        if (movement == null)
        {
            return;
        }

        float progress = totalTravelTime > 0f
            ? (songTime - spawnSongTime) / totalTravelTime
            : 1f;

        movement.UpdateMovement(progress);

        if (highlight != null && !IsResolved)
        {
            bool inPerfectWindow =
                Mathf.Abs(songTime - noteData.beatTime) <= NoteJudge.PerfectWindowSeconds;

            highlight.SetHighlight(inPerfectWindow);
        }
    }

    /// <summary>
    /// Returns whether the note has fallen past the bottom long enough to be removed.
    /// </summary>
    /// <param name="songTime">Current song time in seconds.</param>
    /// <param name="trailSeconds">Time after the target time at which the note reaches the bottom.</param>
    /// <returns>True when the note can be despawned.</returns>
    public bool HasPassed(float songTime, float trailSeconds)
    {
        return songTime > noteData.beatTime + trailSeconds;
    }
}
