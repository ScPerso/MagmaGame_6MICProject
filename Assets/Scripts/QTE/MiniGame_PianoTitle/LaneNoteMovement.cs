using UnityEngine;

/// <summary>
/// Falling-lane presentation of a note (Piano Tiles-inspired):
/// the note travels in a straight line from a spawn point down to the hit line.
/// It implements <see cref="INoteMovement"/> so it can be swapped for another
/// presentation (e.g. an incoming 3D movement) without changing the game logic.
/// </summary>
public class LaneNoteMovement : MonoBehaviour, INoteMovement
{
    private Vector3 spawnPosition;
    private Vector3 hitPosition;

    /// <inheritdoc />
    public void Configure(Vector3 spawnPosition, Vector3 hitPosition)
    {
        this.spawnPosition = spawnPosition;
        this.hitPosition = hitPosition;

        transform.position = spawnPosition;
    }

    /// <inheritdoc />
    public void UpdateMovement(float progress)
    {
        // Unclamped so the note keeps travelling past the hit line when missed.
        transform.position = Vector3.LerpUnclamped(
            spawnPosition,
            hitPosition,
            progress
        );
    }
}
