using Unity.VisualScripting;
using UnityEngine;

public class MainCanvas : DynamicScreen
{
    [SerializeField] private HUD m_hud = default;
    [SerializeField] private PauseScreen m_pauseScreen = default;
    [SerializeField] private LockScreen m_lockScreen = default;

    public HUD HUD => m_hud;
    public PauseScreen PauseScreen => m_pauseScreen;
    public LockScreen LockScreen => m_lockScreen;
    protected override void CustomInit()
    {
        HUD.Init();
        PauseScreen.Init();
        LockScreen.Init();
    }
    protected override void CustomOpen()
    {
        HUD.Open();
        PauseScreen.Close();
        LockScreen.Close();
    }
    protected override void CustomClose()
    {
        HUD.Close();
        PauseScreen.Close();
        LockScreen.Close();
    }
}
