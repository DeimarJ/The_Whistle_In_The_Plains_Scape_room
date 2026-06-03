using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilesSelectionScreen : DynamicScreen
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private FileButtonUI buttonPrefab;

    private readonly List<FileButtonUI> buttons = new();

    protected override void CustomInit()
    {
    }

    protected override void CustomOpen()
    {
        RebuildList();

        if (buttons.Count > 0)
        {
            buttons[0].Button.Select();
        }
        else
        {
            FocusDefaultObject();
        }
    }

    protected override void CustomClose()
    {
    }

    private void RebuildList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        buttons.Clear();

        foreach (FileData file in MainScene.Player.Inventory.GetUnlockedFiles())
        {
            FileButtonUI button =
                Instantiate(buttonPrefab, contentParent);

            button.Setup(file);

            buttons.Add(button);
        }

        ConfigureNavigation();
    }

    private void ConfigureNavigation()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = buttons[i].Button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp =
                i > 0
                ? buttons[i - 1].Button
                : buttons[i].Button;

            nav.selectOnDown =
                i < buttons.Count - 1
                ? buttons[i + 1].Button
                : buttons[i].Button;

            buttons[i].Button.navigation = nav;
        }
    }

    public override void FocusDefaultObject()
    {
        if (buttons.Count > 0)
        {
            Canvas.ForceUpdateCanvases();

            buttons[0].Button.Select();
            return;
        }

        base.FocusDefaultObject();
    }
}
