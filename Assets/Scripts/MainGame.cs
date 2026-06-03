using System.Collections.Generic;
using UnityEngine;

public enum PauseReason
{
    PauseMenu,
    Puzzle,
    Dialogue,
    Cutscene,
    File,
}
public class MainGame : MonoBehaviour
{
    public static MainGame Instance { get; private set; }

    private static readonly HashSet<PauseReason> pauseReasons = new();

    public static bool IsPaused => pauseReasons.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public static void Pause(PauseReason reason)
    {
        if (pauseReasons.Add(reason))
        {
            UpdatePauseState();
        }

        if (reason == PauseReason.PauseMenu)
        {
            MainScene.MainCanvas.PauseScreen.Open();
        }
    }

    public static void Unpause(PauseReason reason)
    {
        if (pauseReasons.Remove(reason))
        {
            UpdatePauseState();
        }

        if (reason == PauseReason.PauseMenu)
        {
            MainScene.MainCanvas.PauseScreen.Close();
        }
    }

    public static void TogglePauseMenu()
    {
        if (pauseReasons.Contains(PauseReason.PauseMenu))
            Unpause(PauseReason.PauseMenu);
        else
            Pause(PauseReason.PauseMenu);
    }

    private static void UpdatePauseState()
    {
        bool paused = pauseReasons.Count > 0;

        Time.timeScale = paused ? 0f : 1f;

        if (MainScene.InputHandler == null)
        {
            return;
        }

        if (paused)
            MainScene.InputHandler.SwitchToUI();
        else
            MainScene.InputHandler.SwitchToGameplay();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            pauseReasons.Clear();
            Time.timeScale = 1f;
        }
    }

}