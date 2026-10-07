using UnityEngine;

public enum ConsumableEffectType
{
    Oil,
    Heal,
    Chili
}

[CreateAssetMenu(
    fileName = "Consumable",
    menuName = "Game/Consumable")]
public class ConsumableData : ScriptableObject
{
    [Header("Identity")]
    public ResourceType resourceType;

    public string displayName;

    public Sprite icon;

    [Header("Effect")]
    public ConsumableEffectType effectType;

    public float amount;
}