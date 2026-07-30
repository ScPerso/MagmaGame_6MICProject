using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Core driver of the rehearsal mini-game.
/// It owns the audio-synced clock, spawns notes ahead of their perfect moment so they
/// fall down their column on the beat, drives their movement and glow, judges player
/// input (mouse, keyboard, touch), accumulates the score, and reports a final
/// <see cref="RehearsalResult"/> on completion. The note presentation is delegated
/// through <see cref="INoteMovement"/> and <see cref="INoteHighlight"/> so it can be
/// swapped without changing this logic.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RehearsalManager : MonoBehaviour
{
    [Header("Track")]
    [Tooltip("Song track (chart + music) played during this rehearsal.")]
    [SerializeField] private SongTrack songTrack;

    [Header("Timing")]
    [Tooltip("Delay in seconds before the track starts, giving the player time to get ready.")]
    [SerializeField] private float startDelaySeconds = DefaultStartDelaySeconds;

    [Tooltip("Time in seconds the note keeps falling past its perfect moment before reaching the bottom and despawning.")]
    [SerializeField] private float noteTrailSeconds = DefaultNoteTrailSeconds;

    [Header("Notes")]
    [Tooltip("Prefab spawned for each note. Must have a component implementing INoteMovement.")]
    [SerializeField] private GameObject notePrefab;

    [Header("Lanes")]
    [Tooltip("Lane definitions. A note's laneIndex maps to an entry in this array.")]
    [SerializeField] private Lane[] lanes;

    [Tooltip("World Y position where notes spawn (top of the play field).")]
    [SerializeField] private float spawnHeight = DefaultSpawnHeight;

    [Tooltip("World Y position where notes despawn (bottom of the play field).")]
    [SerializeField] private float despawnHeight = DefaultDespawnHeight;

    private AudioSource audioSource;
    private Camera mainCamera;

    // Accumulates judgements into the final score.
    private readonly RehearsalScorer scorer = new RehearsalScorer();

    /// <summary>Raised once when the rehearsal finishes, carrying the final result.</summary>
    public event Action<RehearsalResult> Completed;

    /// <summary>Raised each time a note is judged, with its result and world position.</summary>
    public event Action<JudgementResult, Vector3> Judged;

    /// <summary>Points accumulated so far this rehearsal.</summary>
    public int CurrentScore => scorer.CurrentScore;

    // Notes sorted by target time, so the manager can advance a single spawn cursor.
    private List<NoteData> orderedNotes = new List<NoteData>();

    // Notes currently travelling on screen.
    private readonly List<FallingNote> activeNotes = new List<FallingNote>();

    // Index of the next note to spawn.
    private int spawnCursor;

    // DSP time at which the track logically starts. All song times are relative to it.
    private double songStartDspTime;

    private bool isPlaying;

    private const float DefaultStartDelaySeconds = 1f;
    private const float DefaultNoteTrailSeconds = 1.2f;
    private const float DefaultSpawnHeight = 5f;
    private const float DefaultDespawnHeight = -5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (!TryPrepareTrack())
        {
            return;
        }

        StartTrack();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        float songTime = GetSongTime();

        SpawnDueNotes(songTime);
        HandleInput(songTime);
        UpdateActiveNotes(songTime);

        if (spawnCursor >= orderedNotes.Count && activeNotes.Count == 0)
        {
            EndTrack();
        }
    }

    /// <summary>
    /// Current playback time of the track, in seconds, relative to its logical start.
    /// Based on the DSP clock so it stays in sync with the audio.
    /// </summary>
    /// <returns>Elapsed song time in seconds. Negative during the start delay.</returns>
    public float GetSongTime()
    {
        return (float)(AudioSettings.dspTime - songStartDspTime);
    }

    private bool TryPrepareTrack()
    {
        if (songTrack == null)
        {
            Debug.LogWarning("RehearsalManager has no SongTrack assigned.");
            return false;
        }

        if (songTrack.notes == null || songTrack.notes.Count == 0)
        {
            Debug.LogWarning("The assigned SongTrack has no notes.");
            return false;
        }

        if (notePrefab == null)
        {
            Debug.LogWarning("RehearsalManager has no note prefab assigned.");
            return false;
        }

        if (lanes == null || lanes.Length == 0)
        {
            Debug.LogWarning("RehearsalManager has no lanes configured.");
            return false;
        }

        orderedNotes = songTrack.notes
            .OrderBy(note => note.beatTime)
            .ToList();

        spawnCursor = 0;
        activeNotes.Clear();
        scorer.Reset();

        return true;
    }

    private void StartTrack()
    {
        songStartDspTime = AudioSettings.dspTime + startDelaySeconds;

        if (songTrack.music != null)
        {
            audioSource.clip = songTrack.music;
            audioSource.PlayScheduled(songStartDspTime);
        }

        isPlaying = true;
    }

    private void SpawnDueNotes(float songTime)
    {
        float leadTime = songTrack.travelTime;

        while (spawnCursor < orderedNotes.Count
               && songTime >= orderedNotes[spawnCursor].beatTime - leadTime)
        {
            SpawnNote(orderedNotes[spawnCursor], leadTime);
            spawnCursor++;
        }
    }

    private void SpawnNote(NoteData note, float leadTime)
    {
        int laneIndex = Mathf.Clamp(note.laneIndex, 0, lanes.Length - 1);
        float laneX = lanes[laneIndex].xPosition;

        Vector3 topPosition = new Vector3(laneX, spawnHeight, 0f);
        Vector3 bottomPosition = new Vector3(laneX, despawnHeight, 0f);

        GameObject instance = Instantiate(notePrefab, transform);

        FallingNote fallingNote = instance.GetComponent<FallingNote>();

        if (fallingNote == null)
        {
            Debug.LogError("Note prefab is missing a FallingNote component.");
            Destroy(instance);
            return;
        }

        // Note appears leadTime before its perfect moment and keeps falling for
        // noteTrailSeconds afterwards, so the full top-to-bottom fall spans both.
        float spawnSongTime = note.beatTime - leadTime;
        float totalTravelTime = leadTime + noteTrailSeconds;

        fallingNote.Initialize(
            note,
            spawnSongTime,
            totalTravelTime,
            topPosition,
            bottomPosition
        );

        activeNotes.Add(fallingNote);
    }

    private void HandleInput(float songTime)
    {
        HandleMouseInput(songTime);
        HandleKeyboardInput(songTime);
        HandleTouchInput(songTime);
    }

    private void HandleMouseInput(float songTime)
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || mainCamera == null)
        {
            return;
        }

        if (!mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            FallingNote note = hit.collider.GetComponentInParent<FallingNote>();

            if (note != null)
            {
                JudgeNote(note, songTime);
            }
        }
    }

    private void HandleKeyboardInput(float songTime)
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            Key key = lanes[i].key;

            if (key == Key.None)
            {
                continue;
            }

            if (keyboard[key].wasPressedThisFrame)
            {
                TryHitLane(i, songTime);
            }
        }
    }

    private void HandleTouchInput(float songTime)
    {
        Touchscreen touchscreen = Touchscreen.current;

        if (touchscreen == null)
        {
            return;
        }

        foreach (var touch in touchscreen.touches)
        {
            if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
            {
                continue;
            }

            Vector2 screenPosition = touch.position.ReadValue();
            int laneIndex = GetLaneFromScreenX(screenPosition.x);

            TryHitLane(laneIndex, songTime);
        }
    }

    private void TryHitLane(int laneIndex, float songTime)
    {
        FallingNote target = FindHittableNote(laneIndex, songTime);

        if (target == null)
        {
            return;
        }

        JudgeNote(target, songTime);
    }

    // Judges a specific note against the current song time, then resolves and despawns it.
    private void JudgeNote(FallingNote note, float songTime)
    {
        if (note.IsResolved)
        {
            return;
        }

        float timingError = songTime - note.TargetTime;
        JudgementResult result = NoteJudge.Judge(timingError);

        note.Resolve();
        scorer.Register(result);
        Judged?.Invoke(result, note.transform.position);

        Debug.Log(
            "Hit lane " + note.LaneIndex + " -> " + result
            + " (" + (timingError * 1000f).ToString("F0") + "ms)"
        );

        activeNotes.Remove(note);
        Destroy(note.gameObject);
    }

    // Returns the closest unresolved, in-window note for a lane, or null if none.
    private FallingNote FindHittableNote(int laneIndex, float songTime)
    {
        FallingNote best = null;
        float bestError = float.MaxValue;

        foreach (FallingNote note in activeNotes)
        {
            if (note.IsResolved || note.LaneIndex != laneIndex)
            {
                continue;
            }

            float error = songTime - note.TargetTime;

            if (!NoteJudge.IsWithinHitWindow(error))
            {
                continue;
            }

            float absError = Mathf.Abs(error);

            if (absError < bestError)
            {
                bestError = absError;
                best = note;
            }
        }

        return best;
    }

    // Maps a horizontal screen position to a lane index (left to right).
    private int GetLaneFromScreenX(float screenX)
    {
        float normalized = Mathf.Clamp01(screenX / Screen.width);
        int laneIndex = (int)(normalized * lanes.Length);

        return Mathf.Clamp(laneIndex, 0, lanes.Length - 1);
    }

    private void UpdateActiveNotes(float songTime)
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            FallingNote note = activeNotes[i];

            note.Tick(songTime);

            // Flag notes that flew past the hit window without being hit.
            if (!note.IsResolved
                && songTime - note.TargetTime > NoteJudge.GoodWindowSeconds)
            {
                note.Resolve();
                scorer.Register(JudgementResult.Miss);
                Judged?.Invoke(JudgementResult.Miss, note.transform.position);
                Debug.Log("Miss lane " + note.LaneIndex);
            }

            if (note.HasPassed(songTime, noteTrailSeconds))
            {
                activeNotes.RemoveAt(i);
                Destroy(note.gameObject);
            }
        }
    }

    /// <summary>
    /// Restarts the rehearsal from the beginning, clearing notes and score.
    /// </summary>
    public void Restart()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] != null)
            {
                Destroy(activeNotes[i].gameObject);
            }
        }

        activeNotes.Clear();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (!TryPrepareTrack())
        {
            return;
        }

        StartTrack();
    }

    private void EndTrack()
    {
        isPlaying = false;

        RehearsalResult result = scorer.BuildResult();

        Debug.Log(
            "Rehearsal finished | Score " + result.Score + "/" + result.MaxScore
            + " | Accuracy " + (result.Accuracy * 100f).ToString("F0") + "%"
            + " | Perfect " + result.PerfectCount
            + " Good " + result.GoodCount
            + " Miss " + result.MissCount
        );

        Completed?.Invoke(result);
    }
}
