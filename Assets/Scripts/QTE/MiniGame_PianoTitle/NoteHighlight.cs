using UnityEngine;

/// <summary>
/// Color-based highlight for a note: while active, the note's material pulses between
/// its base color and a highlight color to signal the perfect window. Implements
/// <see cref="INoteHighlight"/> so the feedback style can be swapped independently.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class NoteHighlight : MonoBehaviour, INoteHighlight
{
    [Tooltip("Color of the note when it is not in its perfect window.")]
    [SerializeField] private Color baseColor = Color.white;

    [Tooltip("Color the note pulses toward while highlighted.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

    [Tooltip("Speed of the highlight pulse, in radians per second.")]
    [SerializeField] private float pulseSpeed = DefaultPulseSpeed;

    private Renderer noteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool isHighlighted;

    // URP Lit and most shaders expose the base color under this property name.
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private const float DefaultPulseSpeed = 10f;

    private void Awake()
    {
        noteRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        ApplyColor(baseColor);
    }

    /// <inheritdoc />
    public void SetHighlight(bool active)
    {
        isHighlighted = active;

        if (!active)
        {
            ApplyColor(baseColor);
        }
    }

    private void Update()
    {
        if (!isHighlighted)
        {
            return;
        }

        // Oscillate between base and highlight color to create the scintillation.
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        ApplyColor(Color.Lerp(baseColor, highlightColor, pulse));
    }

    private void ApplyColor(Color color)
    {
        if (noteRenderer == null)
        {
            return;
        }

        noteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, color);
        noteRenderer.SetPropertyBlock(propertyBlock);
    }
}
