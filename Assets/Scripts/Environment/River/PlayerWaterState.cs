using UnityEngine;

public class PlayerWaterState : MonoBehaviour
{
    public bool IsSwimming { get; private set; }
    public float CurrentDepth { get; private set; }

    [Header("Eventos opcionales (conectar con tu controlador FPS)")]
    public float swimMoveSpeedMultiplier = 0.6f;

    public void SetSwimming(bool swimming, float depth)
    {
        if (swimming && !IsSwimming)
        {
            
        }
        else if (!swimming && IsSwimming)
        {
            
        }

        IsSwimming = swimming;
        CurrentDepth = depth;
    }
}