using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class RiverCurrent : MonoBehaviour
{
    [Header("Corriente")]
    [SerializeField] private float currentForce = 4f;
    [SerializeField] private float maxPushSpeed = 6f;

    [Header("Profundidad / Inmersión")]
    [SerializeField] private float waterSurfaceY = 0f;
    [SerializeField] private float swimThreshold = 1.2f;

    [Header("Tags afectados")]
    [SerializeField] private string playerTag = "Player";

    [Header("Spline del río")]
    [SerializeField] private SplineContainer splineContainer; // arrastrá el GameObject River acá

    private void Awake()
    {
        // Si no se asignó el spline, intentamos buscarlo en el padre
        if (splineContainer == null)
            splineContainer = GetComponentInParent<SplineContainer>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null) return;

        // Dirección del flujo en el punto más cercano al jugador sobre el spline
        Vector3 flowDirection = GetFlowAtPosition(other.transform.position);

        float depth = waterSurfaceY - other.transform.position.y;
        float immersionFactor = Mathf.Clamp01(depth / Mathf.Max(swimThreshold, 0.01f));

        Vector3 push = flowDirection * currentForce * immersionFactor * Time.deltaTime;
        cc.Move(Vector3.ClampMagnitude(push, maxPushSpeed * Time.deltaTime));

        PlayerWaterState waterState = other.GetComponent<PlayerWaterState>();
        if (waterState != null)
            waterState.SetSwimming(depth >= swimThreshold, depth);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        PlayerWaterState waterState = other.GetComponent<PlayerWaterState>();
        if (waterState != null) waterState.SetSwimming(false, 0f);
    }

    private Vector3 GetFlowAtPosition(Vector3 worldPos)
    {
        if (splineContainer == null) return Vector3.forward;

        Spline spline = splineContainer.Spline;

        // Convertimos posición del jugador a espacio local del SplineContainer
        Vector3 localPos = splineContainer.transform.InverseTransformPoint(worldPos);

        // Encontramos el punto más cercano en el spline
        SplineUtility.GetNearestPoint(spline, (float3)(Vector3)localPos,
            out _, out float t);

        // Obtenemos la tangente (dirección del flujo) en ese punto
        spline.Evaluate(t, out _, out float3 tangent, out _);

        // Convertimos la tangente a espacio mundo y la proyectamos en el plano horizontal
        Vector3 worldTangent = splineContainer.transform.TransformDirection((Vector3)tangent);
        worldTangent.y = 0f;

        return worldTangent.sqrMagnitude > 0.001f ? worldTangent.normalized : Vector3.forward;
    }

    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null) return;
        Gizmos.color = Color.cyan;
        Vector3 flow = GetFlowAtPosition(transform.position);
        Gizmos.DrawRay(transform.position, flow * 5f);
    }
}