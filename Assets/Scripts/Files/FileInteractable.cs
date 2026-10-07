using UnityEngine;

public class FileInteractable : InteractableObject
{
    [Header("File")]
    [SerializeField] private FileData file;

    [SerializeField] private bool openOnPickup;

    public override void Interact(FirstPersonController player)
    {
        if (file == null)
        {
            Debug.LogWarning($"{name}: No FileData assigned.");
            return;
        }

        player.Inventory.UnlockFile(file);
        SoundManager.Instance?.PlaySFX(SFXType.PageGrab);
        if (openOnPickup)
        {
            MainScene.MainCanvas.FilesScreen.ShowFile(file);
        }

        Destroy(gameObject);
    }
}