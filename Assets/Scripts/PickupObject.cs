using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceAmount
{
    public ResourceType type;
    public int amount;
}
public class PickupObject : InteractableObject
{
    [Header("Rewards")]
    [SerializeField]
    private List<ResourceAmount> rewards = new();
    [SerializeField] private SFXType grabSound = SFXType.GrabOil;

    public event System.Action OnPickedUp;

    public override void Interact(FirstPersonController player)
    {
        foreach (ResourceAmount reward in rewards)
        {
            player.Inventory.AddResource(reward.type, reward.amount);
        }

        OnPickedUp?.Invoke();

        SoundManager.Instance?.PlaySFX(grabSound);
        Destroy(gameObject);
    }
}

