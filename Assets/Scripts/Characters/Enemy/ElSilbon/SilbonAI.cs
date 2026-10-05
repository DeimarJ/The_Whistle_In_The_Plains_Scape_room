using UnityEngine;
using UnityEngine.AI;

public class SilbonAI : MonoBehaviour
{
    //Referencias
    [Header("Referencias")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator anim;

    //Estados
    private enum SilbonState { Patrol, Alerted, Chasing, Attacking, Blocked, Fleeing, Charging }
    private SilbonState currentState = SilbonState.Patrol;
    private bool firstDetection = true; // Voy a detectare l grito al detectar por primera vez

    //NIveles de amenaa
    [Header("Nivel de Amenaza")]
    [Range(0f, 1f)] public float threatLevel = 0f;
    public float threatFleeThreshold = 0.5f; // por debajo de esto huye al recibir ají
    [SerializeField] private float chargeMoveSpeed = 8f;
    [SerializeField] private float normalMoveSpeed = 3.5f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float fleeDuration = 6f;
    private float fleeTimer = 0f;

    // Detección por visión
    [Header("Visión")]
    public float detectionRange = 20f;
    public float attackRange = 2.2f;
    public float fieldOfViewAngle = 85f;
    public LayerMask obstacleMask;

    // Detección por pasos
    [Header("Detección por Pasos")]
    public float footstepDetectionRange = 10f;
    [HideInInspector] public float currentNoiseLevel = 0f; // Ya veré como conecto esto al script de pasos del player
    public float noiseThreshold = 0.3f;

    // Detección por la luz de la linterna
    [Header("Detección por Linterna")]
    public float flashlightDetectionRange = 25f;
    [HideInInspector] public bool playerFlashlightOn = false; // También veré como conecto esto al script de la linterna

    // Detección por micrófono
    [Header("Detección por Micrófono")]
    public float micDetectionRange = 12f;
    public float micVolumeThreshold = 0.02f; // sensibilidad al volumen del mic
    private AudioClip micClip;
    private string micDevice;
    private const int MIC_SAMPLE_RATE = 44100;
    private const int MIC_BUFFER_SECONDS = 1;

    // Patrullaje
    [Header("Patrullaje")]
    public Transform[] waypoints;
    public float waypointWaitTime = 3f;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Quaternion targetRotation;
    public float rotationSpeed = 4f;

    // Ataque
    [Header("Ataque")]
    public float attackDamage = 25f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    // Barrera del río
    [Header("Comportamiento en el Río")]
    public float blockedScreamCooldown = 8f; // va a gritar cuando no pueda atrapar al player en el río
    private float blockedScreamTimer = 0f;
    [SerializeField] private AudioSource whistleSource;  // Audio con el silbido ambiental
    [SerializeField] private float whistleRangeMin = 10f;
    [SerializeField] private float whistleRangeMax = 40f;

    // Estado de alerta (Aunque no haya visto al player lo detecta y va a su zona)
    //Con esto intentaré copiar más de la IA de Alien.
    private Vector3 alertPosition;
    private float alertTimer = 0f;
    private float alertDuration = 5f;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.speed = normalMoveSpeed;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
            
        anim = GetComponentInChildren<Animator>();

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
            
        InitMicrophone();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        UpdateWhistle(distance);
        CheckDetection(distance);
        UpdateState(distance);
        UpdateAnimator();
    }

    //Detección multiple (Intentaré que coexistan todas las detecciones posibles y dependiendo de la acción del jugador cambiar el estado)
    void CheckDetection(float distance)
    {
        if (currentState == SilbonState.Fleeing || currentState == SilbonState.Attacking)
        {
            return;
        }

        bool detected = false;

        // 1. Detección por visión
        if (CanSeePlayer()) detected = true;

        // 2. Detección por linterna
        if (!detected && playerFlashlightOn && distance <= flashlightDetectionRange)
        {
            // Si la linterna apunta hacia él, lo detecta
            //NOTA: Después de verificar que se integra bien, modificar esto para el spot ligth de la lámpara, funcionó bien en el juego del orfanato con point ligth.
            Vector3 dirToSilbon = (transform.position - player.position).normalized;
            float angleToSilbon = Vector3.Angle(player.forward, dirToSilbon);
            if (angleToSilbon < 30f) detected = true;
        }

        // 3. Detección por pasos
        //NOTA: Intentar hacer uqe los pasos agachados hagan menos ruido para El Silbón.
        if (!detected && distance <= footstepDetectionRange && currentNoiseLevel >= noiseThreshold)
        {
            alertPosition = player.position;
            alertTimer = alertDuration;
            if (currentState == SilbonState.Patrol) currentState = SilbonState.Alerted;
        }

        //// 4. Micrófono (Dios quiera y esto funcione)
        //if (!detected && distance <= micDetectionRange && GetMicVolume() >= micVolumeThreshold)
        //{
        //    alertPosition = player.position;
        //    alertTimer = alertDuration;
        //    if (currentState == SilbonState.Patrol) currentState = SilbonState.Alerted;
        //}

        //if (detected)
        //{
        //    if (firstDetection)
        //    {
        //        firstDetection = false;
        //        anim.SetTrigger("Scream");
        //    }

        //    currentState = SilbonState.Chasing;
        //    alertTimer = alertDuration;
        //    alertPosition = player.position;
        //}
        //else if (currentState == SilbonState.Chasing)
        //{
        //    // Perdió de vista al jugador
        //    alertTimer -= Time.deltaTime;
        //    if (alertTimer <= 0f) currentState = SilbonState.Patrol;
        //}
        //else if (currentState == SilbonState.Alerted)
        //{
        //    alertTimer -= Time.deltaTime;
        //    if (alertTimer <= 0f) currentState = SilbonState.Patrol;
        //}

        float adjustedThreshold = micVolumeThreshold * (distance / micDetectionRange);
        float micVol = GetMicVolume();

        if (!detected && distance <= micDetectionRange && micVol >= adjustedThreshold)
        {
            // Qué tan bien escuchó (0 = apenas, 1 = perfectamente)
            float hearingClarity = Mathf.Clamp01(micVol / (adjustedThreshold * 2f));

            // Radio de búsqueda inversamente proporcional a la claridad
            // Claridad alta (cerca, habló fuerte) = radio pequeño = punto casi exacto
            // Claridad baja (lejos, habló suave) = radio grande = búsqueda amplia
            float searchRadius = Mathf.Lerp(12f, 0.5f, hearingClarity);

            Vector3 noise = new Vector3(
                Random.Range(-searchRadius, searchRadius),
                0f,
                Random.Range(-searchRadius, searchRadius));

            alertPosition = player.position + noise;
            alertTimer = alertDuration;

            if (currentState == SilbonState.Patrol)
                currentState = SilbonState.Alerted;
        }
    }

    // Switche de máquina de estados
    void UpdateState(float distance)
    {
        switch (currentState)
        {
            case SilbonState.Patrol:
                agent.speed = normalMoveSpeed;
                agent.isStopped = false;
                Patrol();
                break;

            case SilbonState.Alerted:
                agent.speed = normalMoveSpeed;
                agent.isStopped = false;
                agent.SetDestination(alertPosition);
                RotateTowardsMovement();
                break;

            case SilbonState.Chasing:
                // Verificar si el río bloquea el camino
                if (agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    currentState = SilbonState.Blocked;
                    break;
                }

                agent.speed = normalMoveSpeed * (1f + threatLevel); // Voy a multiplicarlo según el nivel de amenaza
                agent.isStopped = distance <= attackRange;

                if (distance > attackRange)
                {
                    agent.SetDestination(player.position);
                    RotateTowardsMovement();
                }
                else
                {
                    agent.isStopped = true;
                    RotateTowardsTarget(player.position);
                    TryAttack();
                    currentState = SilbonState.Attacking;
                }
                break;

            case SilbonState.Attacking:
                agent.isStopped = true;
                RotateTowardsTarget(player.position);
                TryAttack();
                // Vuelve a perseguir si el jugador se aleja
                if (distance > attackRange * 1.5f) currentState = SilbonState.Chasing;
                break;

            case SilbonState.Blocked:
                agent.isStopped = true;
                RotateTowardsTarget(player.position);
                blockedScreamTimer -= Time.deltaTime;
                if (blockedScreamTimer <= 0f)
                {
                    anim.SetTrigger("Scream");
                    blockedScreamTimer = blockedScreamCooldown;
                }
                // Si el jugador vuelve al mismo lado retoma la persecución 
                if (agent.pathStatus != NavMeshPathStatus.PathPartial)
                    currentState = SilbonState.Chasing;
                break;

            case SilbonState.Fleeing:
                agent.speed = fleeSpeed;
                agent.isStopped = false;
                fleeTimer -= Time.deltaTime;
                // Huir en dirección opuesta al jugador
                Vector3 fleeDir = (transform.position - player.position).normalized;
                agent.SetDestination(transform.position + fleeDir * 20f);
                RotateTowardsMovement();
                if (fleeTimer <= 0f)
                {
                    currentState = SilbonState.Patrol;
                    firstDetection = true; // reinicia el grito para la próxima detección
                }
                break;

            case SilbonState.Charging:
                agent.speed = chargeMoveSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);
                RotateTowardsMovement();
                if (distance <= attackRange)
                {
                    agent.isStopped = true;
                    currentState = SilbonState.Attacking;
                }
                break;
        }
    }

    // Le tiramo ají a la cara
    public void OnHitByAji()
    {
        anim.SetTrigger("Scream");

        if (threatLevel < threatFleeThreshold)
        {
            currentState = SilbonState.Fleeing;
            fleeTimer = fleeDuration;
        }
        else
        {
            currentState = SilbonState.Charging;
        }
    }

    //Aquí seteo el ruido de lo spasos del player pa' que esta cosa los lea
    public void SetNoiseLevel(float noise)
    {
        currentNoiseLevel = noise;
    }

    //Blend Tree Direccional
    void UpdateAnimator()
    {
        // Velocidad local relativa a la orientación del personaje
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float maxSpeed = Mathf.Max(normalMoveSpeed, chargeMoveSpeed);

        float vx = Mathf.Clamp(localVelocity.x / maxSpeed, -1f, 1f);
        float vz = Mathf.Clamp(localVelocity.z / maxSpeed, -1f, 1f);

        anim.SetFloat("VelocityX", vx, 0.1f, Time.deltaTime);
        anim.SetFloat("VelocityZ", vz, 0.1f, Time.deltaTime);
    }

    // Silbido ambiental
    void UpdateWhistle(float distance)
    {
        if (whistleSource == null) return;

        // Solo silba cuando está en modo Chasing o Blocked (A futuro intentar ponerlo situacional aunque no esté chasing)
        bool shouldWhistle = currentState == SilbonState.Chasing || currentState == SilbonState.Blocked;

        if (shouldWhistle && !whistleSource.isPlaying)
        {
            whistleSource.Play();
        }
        else if (!shouldWhistle && whistleSource.isPlaying)
        {
            whistleSource.Stop();
        }

        // Volumen inversamente proporcional a la distancia
        if (whistleSource.isPlaying)
        {
            float t = Mathf.InverseLerp(whistleRangeMax, whistleRangeMin, distance);
            whistleSource.volume = Mathf.Lerp(0f, 1f, t);
        }
    }

    // Visión de El Silbón
    bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfViewAngle * 0.5f) return false;

        // Raycast desde la altura del torso debido a que mide más de 2 metros, si no funciona ajustarlo a la mitad del torso (No olvidarme).
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        if (Physics.Raycast(origin, dirToPlayer, distance, obstacleMask)) return false;

        return true;
    }

    // Rotación de movimiento
    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Rotación hacia el jugador
    void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    // Mirar al jugador
    bool IsFacingTarget(Vector3 targetPos, float threshold = 20f)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        return Vector3.Angle(transform.forward, dir) < threshold;
    }

    // Patrullaje por waypoints
    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
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

    // Ataque
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (!IsFacingTarget(player.position, 20f)) return;

        lastAttackTime = Time.time;

        // Seleccionar ataque aleatorio (0=L, 1=R, 2=Ground)
        float attackIndex = Mathf.Round(Random.Range(0f, 2f));
        anim.SetFloat("AttackIndex", attackIndex);
        anim.SetTrigger("Attack");

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }   
    }

    // Microfono (La improvisada más grande de mi vida)
    void InitMicrophone()
    {
        if (Microphone.devices.Length == 0) return;
        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, MIC_BUFFER_SECONDS, MIC_SAMPLE_RATE);
    }

    float GetMicVolume()
    {
        if (micClip == null) return 0f;

        int micPos = Microphone.GetPosition(micDevice);
        if (micPos <= 0) return 0f;

        int sampleCount = Mathf.Min(1024, micPos);
        float[] samples = new float[sampleCount];
        micClip.GetData(samples, Mathf.Max(0, micPos - sampleCount));

        float sum = 0f;
        foreach (float s in samples) sum += s * s;
        return Mathf.Sqrt(sum / sampleCount); // RMS
    }

    void OnDestroy()
    {
        if (micDevice != null && Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
        }   
    }

    // Gizmos
    void OnDrawGizmosSelected()
    {
        // Rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 left = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, left * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rango de pasos
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, footstepDetectionRange);

        // Rango de linterna
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, flashlightDetectionRange);

        // Rango de micrófono
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, micDetectionRange);
    }
}