using Unity.VisualScripting;
using UnityEngine;

public class MainCanvas : DynamicScreen
{
    [SerializeField] private HUD m_hud = default;
    [SerializeField] private PauseScreen m_pauseScreen = default;
    [SerializeField] private FilesScreen m_filesScreen = default;
    [SerializeField] private LockScreen m_lockScreen = default;
    [SerializeField] private WinScreen m_winScreen = default;
    [SerializeField] private GameOverScreen m_gameOverScreen = default;
    public HUD HUD => m_hud;
    public PauseScreen PauseScreen => m_pauseScreen;
    public FilesScreen FilesScreen => m_filesScreen;
    public LockScreen LockScreen => m_lockScreen;
    public WinScreen WinScreen => m_winScreen;  
    public GameOverScreen GameOverScreen => m_gameOverScreen;
    protected override void CustomInit()
    {
        HUD.Init();
        PauseScreen.Init();
        FilesScreen.Init();
        LockScreen.Init();
        if (WinScreen!= null)
        {
            WinScreen.Init();
        }
        if (GameOverScreen != null)
        {
            GameOverScreen.Init();
        }
    }
    protected override void CustomOpen()
    {
        HUD.Open();
        PauseScreen.Close();
        FilesScreen.Close();
        LockScreen.Close();
        if (WinScreen != null)
        {
            WinScreen.Close();
        }
        if (GameOverScreen != null)
        {
            GameOverScreen.Close();
        }
    }
    protected override void CustomClose()
    {
        HUD.Close();
        PauseScreen.Close();
        FilesScreen.Close();
        LockScreen.Close();
        if (WinScreen != null)
        {
            WinScreen.Close();
        }
        if (GameOverScreen != null)
        {
            GameOverScreen.Close();
        }
    }
}
