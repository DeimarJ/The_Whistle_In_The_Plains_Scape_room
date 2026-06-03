using System.Collections.Generic;
using UnityEngine;
public enum ResourceType
{
    Oil,
    Medicine,
    Ammo
}

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class ResourceEntry
    {
        public ResourceType type;
        public int amount;
    }

    [SerializeField]
    private List<ResourceEntry> resources = new();

    [Header("Files")]
    [SerializeField]
    private List<FileData> unlockedFiles = new();
    public int GetResource(ResourceType type)
    {
        ResourceEntry entry =
            resources.Find(r => r.type == type);

        return entry != null
            ? entry.amount
            : 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        ResourceEntry entry =
            resources.Find(r => r.type == type);

        if (entry == null)
        {
            entry = new ResourceEntry
            {
                type = type,
                amount = 0
            };

            resources.Add(entry);
        }

        entry.amount += amount;
    }

    public bool ConsumeResource(ResourceType type, int amount)
    {
        ResourceEntry entry =
            resources.Find(r => r.type == type);

        if (entry == null || entry.amount < amount)
        {
            return false;
        }

        entry.amount -= amount;

        return true;
    }

    public bool HasResource(ResourceType type, int amount = 1)
    {
        return GetResource(type) >= amount;
    }

    public bool UnlockFile(FileData file)
    {
        if (file == null || unlockedFiles.Contains(file))
        {
            return false;
        }

        unlockedFiles.Add(file);
        return true;
    }

    public bool HasFile(FileData file)
    {
        return unlockedFiles.Contains(file);
    }

    public IReadOnlyList<FileData> GetUnlockedFiles()
    {
        return unlockedFiles;
    }
}

