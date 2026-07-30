using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject describing a full rehearsal song track: its music, tempo,
/// note chart and how long a note takes to reach the hit line.
/// It is data-driven so a new track can be authored without changing any code.
/// </summary>
[CreateAssetMenu(
    fileName = "SongTrack",
    menuName = "MiniGames/PianoTitle/Song Track"
)]
public class SongTrack : ScriptableObject
{
    [Header("Music")]
    [Tooltip("Audio clip played during the rehearsal.")]
    public AudioClip music;

    [Tooltip("Tempo of the track in beats per minute.")]
    public float bpm = DefaultBpm;

    [Header("Timing")]
    [Tooltip("Time in seconds a note takes to travel from its spawn point to the hit line.")]
    public float travelTime = DefaultTravelTime;

    [Header("Chart")]
    [Tooltip("Ordered list of notes to spawn during the track.")]
    public List<NoteData> notes = new List<NoteData>();

    private const float DefaultBpm = 120f;
    private const float DefaultTravelTime = 2f;
}
