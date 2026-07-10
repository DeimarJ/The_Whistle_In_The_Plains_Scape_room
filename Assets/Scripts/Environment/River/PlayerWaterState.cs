using UnityEngine;

public class PlayerWaterState : MonoBehaviour
{
    public bool IsSwimming { get; private set; }
    public float CurrentDepth { get; private set; }

    [Header("Eventos opcionales (conectar con tu controlador FPS)")]
    public float swimMoveSpeedMultiplier = 0.6f; // tu controller puede leer esto para frenar el movimiento al nadar

    public void SetSwimming(bool swimming, float depth)
    {
        if (swimming && !IsSwimming)
        {
            // Acá podés disparar sonido de chapoteo, animación, efecto de pantalla mojada, etc.
        }
        else if (!swimming && IsSwimming)
        {
            // Salió del agua
        }

        IsSwimming = swimming;
        CurrentDepth = depth;
    }
}