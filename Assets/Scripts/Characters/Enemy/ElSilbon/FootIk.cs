using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistanceUp = 0.5f;
    [SerializeField] private float raycastDistanceDown = 1.0f;
    [SerializeField] private float footOffset = 0.05f;

    [Header("Suavizado")]
    [Range(0f, 1f)][SerializeField] private float ikWeight = 1f;
    [SerializeField] private float positionSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ajuste de Cadera")]
    [SerializeField] private bool adjustHips = true;
    [SerializeField] private float hipsAdjustSpeed = 8f;
    [SerializeField] private float maxHipsOffset = 0.3f;

    private Vector3 leftFootIKPos, rightFootIKPos;
    private Quaternion leftFootIKRot, rightFootIKRot;
    private float lastHipsOffset;
    private Vector3 initialHipsLocalPos;
    private Transform hipsBone;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hipsBone != null) initialHipsLocalPos = hipsBone.localPosition;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // Pesos de IK para pies (posición y rotación)
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);

        ProcessFoot(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot, ref leftFootIKPos, ref leftFootIKRot);
        ProcessFoot(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot, ref rightFootIKPos, ref rightFootIKRot);

        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPos);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootIKRot);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPos);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootIKRot);

        if (adjustHips) AdjustHips();
    }

    private void ProcessFoot(AvatarIKGoal goal, HumanBodyBones bone, ref Vector3 ikPos, ref Quaternion ikRot)
    {
        Vector3 animPos = animator.GetIKPosition(goal);
        Transform footTransform = animator.GetBoneTransform(bone);

        Vector3 rayOrigin = animPos + Vector3.up * raycastDistanceUp;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistanceUp + raycastDistanceDown, groundLayer))
        {
            Vector3 targetPos = hit.point + Vector3.up * footOffset;
            Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * animator.GetIKRotation(goal);

            ikPos = Vector3.Lerp(ikPos == Vector3.zero ? animPos : ikPos, targetPos, Time.deltaTime * positionSpeed);
            ikRot = Quaternion.Slerp(ikRot == Quaternion.identity ? animator.GetIKRotation(goal) : ikRot, targetRot, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // no detectó suelo (ej. en el aire) -> vuelve a la pose de animación normal
            ikPos = Vector3.Lerp(ikPos, animPos, Time.deltaTime * positionSpeed);
            ikRot = Quaternion.Slerp(ikRot, animator.GetIKRotation(goal), Time.deltaTime * rotationSpeed);
        }
    }

    private void AdjustHips()
    {
        if (hipsBone == null) return;

        float leftOffset = leftFootIKPos.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y;
        float rightOffset = rightFootIKPos.y - animator.GetIKPosition(AvatarIKGoal.RightFoot).y;

        // nos quedamos con el offset más bajo (la pierna que más "cae")
        float targetOffset = Mathf.Min(leftOffset, rightOffset);
        targetOffset = Mathf.Clamp(targetOffset, -maxHipsOffset, maxHipsOffset);

        lastHipsOffset = Mathf.Lerp(lastHipsOffset, targetOffset, Time.deltaTime * hipsAdjustSpeed);

        hipsBone.localPosition = initialHipsLocalPos + Vector3.up * lastHipsOffset;
    }

    private void OnDrawGizmosSelected()
    {
        if (animator == null) return;
        Gizmos.color = Color.red;
        if (animator.isHuman)
        {
            Vector3 l = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Vector3 r = animator.GetIKPosition(AvatarIKGoal.RightFoot);
            Gizmos.DrawWireSphere(l, 0.05f);
            Gizmos.DrawWireSphere(r, 0.05f);
        }
    }
}