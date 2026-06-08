using System;
using UnityEngine;

public enum UIClipType
{
    Undefined,
    ButtonClick1,
}

public enum SFXType
{
    Undefined,
    PlayerSteps,
    PlayerJump,
    PlayerCrouch,
    PlayerCrouchStep,
    PlayerGrab,
    PlayerDrop,
    PlayerSprint,
    PlayerDamage,
    DoorOpen,
    DoorClose,
    KeyGrab,
    PageGrab,
    PageOpen,
    LanternOn,
    LanternOff,
    OilOver,
    RefillOil,
    GrabOil
}

public enum MusicType
{
    Undefined,
    Ambient1,
    Ambient2,
    Ambient3,
    Ambient4
}

[Serializable]
public struct UIClipData
{
    public UIClipType type;
    public AudioClip[] clips;
}

[Serializable]
public struct SFXClipData
{
    public SFXType type;
    public AudioClip[] clips;
}

[Serializable]
public struct MusicClipData
{
    public MusicType type;
    public AudioClip clip;
}



[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Scriptable Objects/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [Header("UI Sounds")]
    [SerializeField] private UIClipData[] uiClipList = default;
    [SerializeField] private SFXClipData[] sfxClipList = default;
    [SerializeField] private MusicClipData[] musicClipList = default;

    public AudioClip GetUIClip(UIClipType type)
    {
        if (type == UIClipType.Undefined)
            return null;

        foreach (UIClipData data in uiClipList)
        {
            if (data.type == type)
            {
                if (data.clips == null || data.clips.Length == 0)
                    return null;

                int randomIndex = UnityEngine.Random.Range(0, data.clips.Length);
                return data.clips[randomIndex];
            }
        }

        Debug.LogWarning($"UI Clip not found for type: {type}");
        return null;
    }

    public AudioClip GetRandomSFXClip(SFXType type)
    {
        if (type == SFXType.Undefined)
            return null;

        foreach (SFXClipData data in sfxClipList)
        {
            if (data.type == type)
            {
                if (data.clips == null || data.clips.Length == 0)
                    return null;

                int randomIndex = UnityEngine.Random.Range(0, data.clips.Length);
                return data.clips[randomIndex];
            }
        }

        Debug.LogWarning($"SFX Clip not found for type: {type}");
        return null;
    }

    public AudioClip GetMusicClip(MusicType type)
    {
        if (type == MusicType.Undefined)
            return null;

        foreach (MusicClipData data in musicClipList)
        {
            if (data.type == type)
                return data.clip;
        }

        Debug.LogWarning($"Music Clip not found for type: {type}");
        return null;
    }


}