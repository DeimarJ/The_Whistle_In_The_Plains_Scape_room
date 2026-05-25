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

    public override void Interact(FirstPersonController player)
    {
        foreach (ResourceAmount reward in rewards)
        {
            player.Inventory.AddResource(
                reward.type,
                reward.amount
            );
        }

        Destroy(gameObject);
    }
}