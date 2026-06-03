using Unity.VisualScripting;
using UnityEngine;

public class MainCanvas : DynamicScreen
{
    [SerializeField] private HUD m_hud = default;
    [SerializeField] private PauseScreen m_pauseScreen = default;
    [SerializeField] private FilesScreen m_filesScreen = default;
    [SerializeField] private LockScreen m_lockScreen = default;
    public HUD HUD => m_hud;
    public PauseScreen PauseScreen => m_pauseScreen;
    public FilesScreen FilesScreen => m_filesScreen;
    public LockScreen LockScreen => m_lockScreen;
    protected override void CustomInit()
    {
        HUD.Init();
        PauseScreen.Init();
        FilesScreen.Init();
        LockScreen.Init();
    }
    protected override void CustomOpen()
    {
        HUD.Open();
        PauseScreen.Close();
        FilesScreen.Close();
        LockScreen.Close();
    }
    protected override void CustomClose()
    {
        HUD.Close();
        PauseScreen.Close();
        FilesScreen.Close();
        LockScreen.Close();
    }
}
