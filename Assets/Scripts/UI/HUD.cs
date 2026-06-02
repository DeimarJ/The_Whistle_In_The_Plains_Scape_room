using UnityEngine;

public class HUD : DynamicScreen
{
    [SerializeField] private DynamicButton m_pauseBtn = default;
    private void PauseBtn() 
    {
        MainGame.Pause(PauseReason.PauseMenu);
    }
    protected override void CustomInit()
    {
        m_pauseBtn.SetOnClick(PauseBtn);
    }
    protected override void CustomOpen()
    {
    }
    protected override void CustomClose()
    {
    }

}
