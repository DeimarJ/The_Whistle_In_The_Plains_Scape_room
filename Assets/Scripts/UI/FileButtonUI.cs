using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileButtonUI : MonoBehaviour
{
    [SerializeField] private DynamicButton button;
    [SerializeField] private TMP_Text titleText;

    public Button Button => button.TargetButton;

    private void ShowFile(FileData file)
    {
        MainScene.MainCanvas.FilesScreen.ShowFile(file);
    }
    public void Setup(FileData file)
    {
        titleText.text = file.fileName;

        button.SetOnClick(()=> ShowFile(file));
    }
}