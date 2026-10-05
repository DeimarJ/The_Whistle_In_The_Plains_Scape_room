using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootIKRigging : MonoBehaviour
{
    [Header("Targets IK (los GameObjects vacíos del TwoBoneIK)")]
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;

    [Header("Hints de rodilla")]
    [SerializeField] private Transform leftKneeHint;
    [SerializeField] private Transform rightKneeHint;

    [Header("Huesos de referencia")]
    [SerializeField] private Transform leftFoot;   // L.Foot
    [SerializeField] private Transform rightFoot;  // R.Foot
    [SerializeField] private Transform leftCalf;   // L.Calf (para el hint)
    [SerializeField] private Transform rightCalf;  // R.Calf

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistanceUp = 14f;
    [SerializeField] private float raycastDistanceDown = 14f;
    [SerializeField] private float footOffset = 1f;

    [Header("Suavizado")]
    [SerializeField] private float positionSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ajuste de Cadera")]
    [SerializeField] private bool adjustHips = true;
    [SerializeField] private float hipsAdjustSpeed = 8f;
    [SerializeField] private float maxHipsOffset = 5f;
    [SerializeField] private Transform hipsBone;

    private Vector3 leftTargetPos, rightTargetPos;
    private Quaternion leftTargetRot = Quaternion.identity;
    private Quaternion rightTargetRot = Quaternion.identity;
    private float lastHipsOffset;
    private Vector3 initialHipsLocalPos;

    private void Awake()
    {
        if (hipsBone != null)
            initialHipsLocalPos = hipsBone.localPosition;

        // Inicializar en la posición real de los pies, no en Vector3.zero
        if (leftFoot != null) leftTargetPos = leftFoot.position;
        if (rightFoot != null) rightTargetPos = rightFoot.position;

        // Inicializar rotaciones también
        if (leftFoot != null) leftTargetRot = leftFoot.rotation;
        if (rightFoot != null) rightTargetRot = rightFoot.rotation;
    }



    private void LateUpdate()
    {
        ProcessFoot(leftFoot, ref leftTargetPos, ref leftTargetRot);
        ProcessFoot(rightFoot, ref rightTargetPos, ref rightTargetRot);

        // Aplicar a los targets del TwoBoneIK
        if (leftFootTarget != null)
        {
            leftFootTarget.position = leftTargetPos;
            leftFootTarget.rotation = leftTargetRot;
        }
        if (rightFootTarget != null)
        {
            rightFootTarget.position = rightTargetPos;
            rightFootTarget.rotation = rightTargetRot;
        }

        // Hints de rodilla — siempre apuntan hacia adelante del personaje
        if (leftKneeHint != null && leftCalf != null)
            leftKneeHint.position = leftCalf.position + transform.forward * 2f;
        if (rightKneeHint != null && rightCalf != null)
            rightKneeHint.position = rightCalf.position + transform.forward * 2f;

        if (adjustHips) AdjustHips();
    }

    private void ProcessFoot(Transform footBone, ref Vector3 ikPos, ref Quaternion ikRot)
    {
        if (footBone == null) return;

        Vector3 origin = footBone.position + Vector3.up * raycastDistanceUp;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
            raycastDistanceUp + raycastDistanceDown, groundLayer))
        {
            Vector3 targetPos = hit.point + Vector3.up * footOffset;
            Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal)
                                   * footBone.rotation;

            ikPos = Vector3.Lerp(ikPos == Vector3.zero ? footBone.position : ikPos,
                                 targetPos, Time.deltaTime * positionSpeed);
            ikRot = Quaternion.Slerp(ikRot, targetRot, Time.deltaTime * rotationSpeed);
        }
        else
        {
            ikPos = Vector3.Lerp(ikPos, footBone.position, Time.deltaTime * positionSpeed);
            ikRot = Quaternion.Slerp(ikRot, footBone.rotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void AdjustHips()
    {
        if (hipsBone == null) return;

        float leftDiff = leftTargetPos.y - (leftFoot != null ? leftFoot.position.y : 0f);
        float rightDiff = rightTargetPos.y - (rightFoot != null ? rightFoot.position.y : 0f);

        float targetOffset = Mathf.Clamp(Mathf.Min(leftDiff, rightDiff),
                                         -maxHipsOffset, maxHipsOffset);

        lastHipsOffset = Mathf.Lerp(lastHipsOffset, targetOffset,
                                    Time.deltaTime * hipsAdjustSpeed);

        hipsBone.localPosition = initialHipsLocalPos + Vector3.up * lastHipsOffset;
    }

    private void OnDrawGizmosSelected()
    {
        if (leftFoot != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftFoot.position, 0.3f);
        }
        if (rightFoot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rightFoot.position, 0.3f);
        }
    }
}