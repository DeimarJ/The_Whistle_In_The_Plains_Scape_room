using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiverCurrent : MonoBehaviour
{
    [Header("Corriente")]
    [SerializeField] private Vector3 flowDirection = Vector3.forward; // dirección del cauce, normalizada en Awake
    [SerializeField] private float currentForce = 4f;
    [SerializeField] private float maxPushSpeed = 6f; // tope de velocidad que la corriente puede imponer

    [Header("Profundidad / Inmersión")]
    [SerializeField] private float waterSurfaceY; // altura del agua en mundo (Y), ajustar según tu Water Surface
    [SerializeField] private float swimThreshold = 1.2f; // cuánto debe hundirse el personaje para considerarse "nadando"

    [Header("Tags afectados")]
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        flowDirection = flowDirection.normalized;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null) return;

        float depth = waterSurfaceY - other.transform.position.y;
        bool isSwimming = depth >= swimThreshold;

        // Empuje de la corriente (escalado según qué tan sumergido está)
        float immersionFactor = Mathf.Clamp01(depth / swimThreshold);
        Vector3 push = flowDirection * currentForce * immersionFactor * Time.deltaTime;

        // Aplicamos el empuje como movimiento adicional sin pisar el input del jugador
        cc.Move(Vector3.ClampMagnitude(push, maxPushSpeed * Time.deltaTime));

        // Notificamos al jugador si tiene un componente que reaccione a estar nadando
        PlayerWaterState waterState = other.GetComponent<PlayerWaterState>();
        if (waterState != null)
        {
            waterState.SetSwimming(isSwimming, depth);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        PlayerWaterState waterState = other.GetComponent<PlayerWaterState>();
        if (waterState != null) waterState.SetSwimming(false, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 dir = Application.isPlaying ? flowDirection : flowDirection.normalized;
        Gizmos.DrawRay(transform.position, dir * 5f);
    }
}