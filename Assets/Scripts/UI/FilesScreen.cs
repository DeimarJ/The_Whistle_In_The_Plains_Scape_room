using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FilesScreen : DynamicScreen
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private FileData currentFile;

    public void ShowFile(FileData file)
    {
        MainGame.Pause(PauseReason.File);
        currentFile = file;

        titleText.text = file.fileName;
        bodyText.text = file.content;

        Open();
    }

    protected override void CustomInit()
    {
    }

    protected override void CustomOpen()
    {
        FocusDefaultObject();

        SoundManager.Instance?.PlaySFX(SFXType.PageOpen);
    }

    protected override void CustomClose()
    {
    }

    public override void OnCancel()
    {
        MainGame.Unpause(PauseReason.File);

        base.OnCancel();
    }
}
