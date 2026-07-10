using UnityEngine;

public class BodyTerrainAlign : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform navRoot; // el objeto con el NavMeshAgent (el padre)

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastUpOffset = 1f;
    [SerializeField] private float raycastDistance = 3f;

    [Header("Suavizado")]
    [SerializeField] private float tiltSpeed = 6f;
    [SerializeField] private float maxTiltAngle = 45f; // por seguridad, no inclinar más de esto

    private Quaternion currentTilt = Quaternion.identity;

    private void Reset()
    {
        if (navRoot == null && transform.parent != null)
            navRoot = transform.parent;
    }

    private void LateUpdate()
    {
        if (navRoot == null) return;

        Vector3 origin = navRoot.position + Vector3.up * raycastUpOffset;
        Vector3 groundNormal = Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastUpOffset + raycastDistance, groundLayer))
        {
            groundNormal = hit.normal;
        }

        // Limitar el ángulo máximo de inclinación para que no se vea raro en paredes/superficies extremas
        float angle = Vector3.Angle(Vector3.up, groundNormal);
        if (angle > maxTiltAngle)
        {
            groundNormal = Vector3.Slerp(Vector3.up, groundNormal, maxTiltAngle / angle);
        }

        // Mantiene el "forward" (hacia dónde mira/camina) que ya define el root,
        // pero inclina el "up" según la normal del terreno
        Vector3 forward = Vector3.ProjectOnPlane(navRoot.forward, groundNormal).normalized;
        if (forward.sqrMagnitude < 0.0001f) forward = navRoot.forward;

        Quaternion targetTilt = Quaternion.LookRotation(forward, groundNormal);

        currentTilt = Quaternion.Slerp(currentTilt == Quaternion.identity ? navRoot.rotation : currentTilt,
                                        targetTilt, Time.deltaTime * tiltSpeed);

        transform.rotation = currentTilt;
        transform.position = navRoot.position; // el Model sigue al root en posición, solo cambia la rotación visual
    }
}