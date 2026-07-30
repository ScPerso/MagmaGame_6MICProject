using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Configuration of a single lane the notes travel through.
/// The lane index is its position in the manager's lane array.
/// </summary>
[Serializable]
public class Lane
{
    [Tooltip("World X position of this lane.")]
    public float xPosition;

    [Tooltip("Keyboard key that hits notes in this lane.")]
    public Key key = Key.None;
}
