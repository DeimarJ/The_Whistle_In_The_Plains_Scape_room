using UnityEngine;

public class PauseScreen : DynamicScreen
{
    [SerializeField] private DynamicButton m_resumeBtn = default;
    [SerializeField] private DynamicButton m_backgroundBtn = default;
    private void ResumeBtn()
    {
        Close();
    }
    protected override void CustomInit()
    {
        m_backgroundBtn.SetOnClick(ResumeBtn);
        m_resumeBtn.SetOnClick(ResumeBtn);

    }
    protected override void CustomOpen()
    {
    }
    protected override void CustomClose()
    {
    }
}
