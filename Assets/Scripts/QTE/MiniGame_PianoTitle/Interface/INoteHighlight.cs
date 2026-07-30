/// <summary>
/// Abstraction of a note's highlight feedback (glow, color change, scintillation)
/// shown while the note is inside its perfect window. Kept separate from the movement
/// so the highlight style can be swapped without touching the game logic.
/// </summary>
public interface INoteHighlight
{
    /// <summary>
    /// Enables or disables the highlight.
    /// </summary>
    /// <param name="active">True while the note is inside its perfect window.</param>
    void SetHighlight(bool active);
}
