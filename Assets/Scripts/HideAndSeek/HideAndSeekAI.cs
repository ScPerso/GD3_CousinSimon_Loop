using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA de patrouille. Patrouille entre des waypoints avec pauses et animation look around.
/// Passe en mode chasse si le joueur entre dans le cone de vision, retourne en patrouille si perdu.
/// Si elle touche le joueur : defaite.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class HideAndSeekAI : MonoBehaviour
{
    public static HideAndSeekAI Instance { get; private set; }

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waypointStopDistance = 0.5f;

    [Header("Patrol Pause")]
    [Tooltip("Duree minimale de la pause a chaque waypoint (secondes).")]
    public float waypointPauseMin = 1f;
    [Tooltip("Duree maximale de la pause a chaque waypoint (secondes).")]
    public float waypointPauseMax = 2f;

    [Header("Detection")]
    public float viewDistance = 8f;
    [Range(1f, 180f)]
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 5f;
    [Tooltip("Vitesse lente en patrouille (sans avoir vu le joueur).")]
    public float patrolSpeed = 2.5f;
    public float catchDistance = 1.2f;
    public float loseSightDistance = 12f;

    [Header("Animator Parameters")]
    public string animSpeedParam = "Speed";
    public string animAttackParam = "Attack";
    [Tooltip("Parametre bool active pendant la pause au waypoint (animation look around).")]
    public string animLookAroundParam = "LookAround";

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint;
    private bool isChasingPlayer;
    private bool isPausing;
    private Transform player;
    private float attackCooldown;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (HideAndSeekPlayer.Instance != null)
            player = HideAndSeekPlayer.Instance.transform;
        else
            Debug.LogError("[HideAndSeekAI] HideAndSeekPlayer.Instance introuvable !");

        agent.speed = patrolSpeed;

        if (waypoints != null && waypoints.Length > 0)
            GoToNextWaypoint();
        else
            Debug.LogWarning("[HideAndSeekAI] Aucun waypoint assigne.");
    }

    private void Update()
    {
        if (HideAndSeekManager.Instance != null && !HideAndSeekManager.Instance.IsPlaying)
            return;

        if (player == null) return;

        attackCooldown -= Time.deltaTime;

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            // Interrompre la pause si elle est en cours
            if (isPausing)
            {
                StopAllCoroutines();
                isPausing = false;
                SetLookAround(false);
            }

            isChasingPlayer = true;
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else if (isChasingPlayer)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > loseSightDistance)
            {
                isChasingPlayer = false;
                agent.speed = patrolSpeed;
                Debug.Log("[HideAndSeekAI] Joueur perdu — retour en patrouille.");
                GoToNextWaypoint();
            }
            else
            {
                // Continue vers la derniere position connue
                agent.SetDestination(player.position);
            }
        }
        else if (!isPausing)
        {
            // Patrouille : avancer vers le prochain waypoint
            if (!agent.pathPending && agent.remainingDistance <= waypointStopDistance)
                StartCoroutine(PauseAtWaypoint());
        }

        // Animation vitesse
        if (animator != null)
            animator.SetFloat(animSpeedParam, agent.velocity.magnitude);

        // Capture
        if (Vector3.Distance(transform.position, player.position) <= catchDistance && attackCooldown <= 0f)
        {
            attackCooldown = 1f;
            if (animator != null)
                animator.SetTrigger(animAttackParam);
            Debug.Log("[HideAndSeekAI] JOUEUR ATTRAPE !");
            HideAndSeekManager.Instance?.TriggerDefeat();
        }
    }

    /// <summary>Arrete l'IA au waypoint, joue l'animation look around, puis repart.</summary>
    private IEnumerator PauseAtWaypoint()
    {
        isPausing = true;
        agent.isStopped = true;
        SetLookAround(true);

        float pauseDuration = Random.Range(waypointPauseMin, waypointPauseMax);
        Debug.Log($"[HideAndSeekAI] Pause au waypoint {currentWaypoint} — {pauseDuration:F1}s.");
        yield return new WaitForSeconds(pauseDuration);

        SetLookAround(false);
        agent.isStopped = false;
        isPausing = false;

        GoToNextWaypoint();
    }

    /// <summary>Active ou desactive le parametre bool LookAround sur l'Animator.</summary>
    private void SetLookAround(bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(animLookAroundParam))
            animator.SetBool(animLookAroundParam, value);
    }

    /// <summary>Retourne vrai si le joueur est dans le cone de vision et sans obstacle.</summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypoint].position);
        Debug.Log($"[HideAndSeekAI] Cap waypoint {currentWaypoint}.");
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isChasingPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftDir  = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir  * viewDistance);
        Gizmos.DrawRay(transform.position, rightDir * viewDistance);
    }
}
