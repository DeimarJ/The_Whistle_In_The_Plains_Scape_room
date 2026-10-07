using UnityEngine;

public class GameOverScreen : DynamicScreen
{
    [SerializeField] private DynamicButton m_restartButton = default;
    private void RestartLevel()
    {
        if (MainGame.Instance != null)
        {
            MainGame.RestartScene();
        }
    }
    protected override void CustomInit()
    {
        m_restartButton.SetOnClick(RestartLevel);
    }

    protected override void CustomOpen()
    {
    }
    protected override void CustomClose()
    {
    }

    public override void OnCancel()
    {
        RestartLevel();
    }
}
