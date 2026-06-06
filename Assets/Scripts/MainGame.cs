using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum PauseReason
{
    PauseMenu,
    Puzzle,
    Dialogue,
    Cutscene,
    File,
    Win,
    Death,
}
public class MainGame : MonoBehaviour
{
    public static MainGame Instance { get; private set; }
    [SerializeField] private MusicType musicType = MusicType.Ambient1;

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
    private void Start()
    {
        SoundManager.Instance?.PlayMusic(musicType);
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
    public static void OnDeath()
    {
        Pause(PauseReason.Death);
        MainScene.InputHandler.SwitchToUI();
        if(MainScene.MainCanvas.GameOverScreen != null)
        {
            MainScene.MainCanvas.GameOverScreen.Open();
        }
    } 
    public static void OnWin()
    {
        Pause(PauseReason.Win);
        MainScene.InputHandler.SwitchToUI();
        if (MainScene.MainCanvas.WinScreen != null)
        {
            MainScene.MainCanvas.WinScreen.Open();
        }
    }
    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}