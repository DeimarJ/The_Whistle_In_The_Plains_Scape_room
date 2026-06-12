using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ConsumableDatabase",
    menuName = "Game/Consumable Database")]
public class ConsumableDatabase : ScriptableObject
{
    [SerializeField]
    private List<ConsumableData> consumables = new();

    private Dictionary<ResourceType, ConsumableData> lookup;

    private void BuildLookup()
    {
        if (lookup != null)
        {
            return;
        }

        lookup = new Dictionary<ResourceType, ConsumableData>();

        foreach (ConsumableData consumable in consumables)
        {
            lookup[consumable.resourceType] = consumable;
        }
    }

    public ConsumableData Get(ResourceType type)
    {
        BuildLookup();

        lookup.TryGetValue(type, out ConsumableData data);

        return data;
    }

    public IReadOnlyList<ConsumableData> GetAll()
    {
        return consumables;
    }
}