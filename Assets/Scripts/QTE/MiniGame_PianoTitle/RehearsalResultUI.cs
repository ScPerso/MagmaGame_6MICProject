using UnityEngine;

/// <summary>
/// Temporary end-of-rehearsal screen drawn with IMGUI. It listens to the
/// <see cref="RehearsalManager"/> completion event and shows the score with a replay
/// button. This is throwaway placeholder UI meant to be replaced by a proper UI later.
/// </summary>
[RequireComponent(typeof(RehearsalManager))]
public class RehearsalResultUI : MonoBehaviour
{
    private RehearsalManager rehearsalManager;
    private RehearsalResult result;
    private bool hasResult;

    private const float PanelWidth = 360f;
    private const float PanelHeight = 240f;

    private void Awake()
    {
        rehearsalManager = GetComponent<RehearsalManager>();
    }

    private void OnEnable()
    {
        rehearsalManager.Completed += OnRehearsalCompleted;
    }

    private void OnDisable()
    {
        rehearsalManager.Completed -= OnRehearsalCompleted;
    }

    private void OnRehearsalCompleted(RehearsalResult rehearsalResult)
    {
        result = rehearsalResult;
        hasResult = true;
    }

    private void OnGUI()
    {
        if (!hasResult)
        {
            return;
        }

        float x = (Screen.width - PanelWidth) * 0.5f;
        float y = (Screen.height - PanelHeight) * 0.5f;

        GUILayout.BeginArea(
            new Rect(x, y, PanelWidth, PanelHeight),
            GUI.skin.box
        );

        GUILayout.Label("Repetition terminee");
        GUILayout.Space(8f);

        GUILayout.Label("Score : " + result.Score + " / " + result.MaxScore);
        GUILayout.Label("Precision : " + (result.Accuracy * 100f).ToString("F0") + "%");
        GUILayout.Space(8f);

        GUILayout.Label("Perfect : " + result.PerfectCount);
        GUILayout.Label("Good    : " + result.GoodCount);
        GUILayout.Label("Miss    : " + result.MissCount);
        GUILayout.Space(12f);

        if (GUILayout.Button("Rejouer", GUILayout.Height(32f)))
        {
            hasResult = false;
            rehearsalManager.Restart();
        }

        GUILayout.EndArea();
    }
}
