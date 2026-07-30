using System;
using UnityEngine;

/// <summary>
/// Serializable data for a single note of a rehearsal song track.
/// It describes when the note must be hit and in which lane it appears.
/// </summary>
[Serializable]
public class NoteData
{
    [Tooltip("Time in seconds, from the start of the track, when the note must be hit.")]
    public float beatTime;

    [Tooltip("Index of the lane the note travels through.")]
    public int laneIndex;
}
