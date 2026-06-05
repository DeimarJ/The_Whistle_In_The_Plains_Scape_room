using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    private NavMeshAgent agent;
    private Animator anim;

    // Visión
    public float fieldOfViewAngle = 90f;
    public LayerMask obstacleMask;
    bool isChasing = false;

    // Patrullaje
    public Transform[] waypoints;
    public float waypointWaitTime = 2f;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;

    // Rotación manual
    private bool isWaiting = false;
    private Quaternion targetRotation;
    public float rotationSpeed = 5f;

    // Ataque
    [Header("Attack")]
    public float attackDamage = 20f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        anim = GetComponent<Animator>();

        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            isChasing = true;
            waitTimer = 0f;
            isWaiting = false;
        }
        else if (distance > detectionRange)
        {
            isChasing = false;
        }

        if (isChasing)
        {
            agent.isStopped = distance <= attackRange;

            if (distance > attackRange)
            {
                agent.SetDestination(player.position);

                // Rotación suave siguiendo la velocidad real del agente (No olvidarme de esto)
                RotateTowardsMovement();
            }
            else
            {
                // Rotar hacia el jugador antes/durante el ataque (No olvidarme de hacer la prueba de irme detrás de él)
                RotateTowardsTarget(player.position);
                TryAttack();
            }
        }
        else
        {
            agent.isStopped = false;
            Patrol();
        }

        float speedNorm = agent.velocity.magnitude / agent.speed;
        anim.SetFloat("Speed", speedNorm, 0.1f, Time.deltaTime);
    }


    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Rotaci+on hacia el jugador o siguiente waypoint
    void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    bool IsFacingTarget(Vector3 targetPos, float threshold = 10f)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        return Vector3.Angle(transform.forward, dir) < threshold;
    }

    bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfViewAngle * 0.5f) return false;
        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstacleMask)) return false;
        return true;
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // isWaiting para girar antes de moverse (SI NO ME FUNCIONA UTILIZARÉ OTRO BOOLEANO DE CONTROL PARA LA ROTACIÓN)
            isWaiting = true;
            waitTimer += Time.deltaTime;

            int nextWaypoint = (currentWaypoint + 1) % waypoints.Length;
            RotateTowardsTarget(waypoints[nextWaypoint].position);

            if (waitTimer >= waypointWaitTime && IsFacingTarget(waypoints[nextWaypoint].position))
            {
                currentWaypoint = nextWaypoint;
                agent.SetDestination(waypoints[currentWaypoint].position);
                waitTimer = 0f;
                isWaiting = false;
            }
        }
        else
        {
            isWaiting = false;
            RotateTowardsMovement();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (!IsFacingTarget(player.position, 20f)) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 left = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, left * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);
    }
}