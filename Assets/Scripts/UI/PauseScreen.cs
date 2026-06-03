using UnityEngine;

public class PauseScreen : DynamicScreen
{
    [SerializeField] private DynamicButton m_resumeBtn = default;
    [SerializeField] private DynamicButton m_filesBtn = default;
    [SerializeField] private DynamicButton m_controlsBtn = default;
    [SerializeField] private DynamicButton m_backgroundBtn = default;
    [SerializeField] private FilesSelectionScreen m_filesSelectionScreeen = default;
    [SerializeField] private ControlsScreen m_controlScreen = default;
    private void ResumeBtn()
    {
        MainGame.Unpause(PauseReason.PauseMenu);
    }
    private void FilesBtn()
    {
        m_filesSelectionScreeen.Open();
    }
    private void ControlsBtn()
    {
        m_controlScreen.Open();
    }
    protected override void CustomInit()
    {
        m_backgroundBtn.SetOnClick(ResumeBtn);
        m_resumeBtn.SetOnClick(ResumeBtn);
        m_filesBtn.SetOnClick(FilesBtn);
        m_controlsBtn.SetOnClick(ControlsBtn);
        m_filesSelectionScreeen.Init();
        m_controlScreen.Init();

    }
    protected override void CustomOpen()
    {
        m_filesSelectionScreeen.Close();
        m_controlScreen.Close();
    }
    protected override void CustomClose()
    {
        m_filesSelectionScreeen.Close();
        m_controlScreen.Close();
    }
    public override void OnCancel()
    {
        MainGame.Unpause(PauseReason.PauseMenu);
    }
}
