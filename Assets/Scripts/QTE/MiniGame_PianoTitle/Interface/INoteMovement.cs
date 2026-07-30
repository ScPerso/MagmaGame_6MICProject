using UnityEngine;

/// <summary>
/// Abstraction of how a note moves visually from its spawn point to the hit line.
/// The game logic drives the normalized progress while a concrete implementation
/// decides the actual on-screen movement. This lets the presentation be swapped
/// (falling lanes, incoming perspective, etc.) without touching the logic.
/// </summary>
public interface INoteMovement
{
    /// <summary>
    /// Configures the endpoints of the note movement.
    /// Called once when the note is spawned.
    /// </summary>
    /// <param name="spawnPosition">World position at progress 0.</param>
    /// <param name="hitPosition">World position at progress 1 (the hit line).</param>
    void Configure(Vector3 spawnPosition, Vector3 hitPosition);

    /// <summary>
    /// Updates the note visual for the given normalized progress.
    /// </summary>
    /// <param name="progress">
    /// Normalized progress from 0 (spawn point) to 1 (hit line). Values above 1
    /// mean the note has travelled past the hit line.
    /// </param>
    void UpdateMovement(float progress);
}
