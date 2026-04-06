using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA de patrouille. Patrouille entre des waypoints, passe en mode chasse si le joueur
/// entre dans le cône de vision, retourne en patrouille si elle le perd.
/// Si elle touche le joueur → défaite.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class HideAndSeekAI : MonoBehaviour
{
    public static HideAndSeekAI Instance { get; private set; }

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waypointStopDistance = 0.5f;

    [Header("Detection")]
    public float viewDistance = 8f;
    [Range(1f, 180f)]
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 6f;      // Légèrement plus vite que le joueur (moveSpeed = 5)
    public float patrolSpeed = 5f;     // Même vitesse que le joueur en patrouille
    public float catchDistance = 1.2f;
    public float loseSightDistance = 12f;

    [Header("Animator Parameters")]
    public string animSpeedParam = "Speed";
    public string animAttackParam = "Attack";

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint;
    private bool isChasingPlayer;
    private Transform player;
    private float attackCooldown;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        Debug.Log("[HideAndSeekAI] Awake OK.");
    }

    private void Start()
    {
        if (HideAndSeekPlayer.Instance != null)
            player = HideAndSeekPlayer.Instance.transform;
        else
            Debug.LogError("[HideAndSeekAI] HideAndSeekPlayer.Instance introuvable ! Vérifie que le joueur est dans la scène.");

        agent.speed = patrolSpeed;

        if (waypoints != null && waypoints.Length > 0)
            GoToNextWaypoint();
        else
            Debug.LogWarning("[HideAndSeekAI] Aucun waypoint assigné — l'IA ne patrouillera pas.");

        Debug.Log($"[HideAndSeekAI] Start — {waypoints?.Length ?? 0} waypoints, viewDistance={viewDistance}, viewAngle={viewAngle}");
    }

    private void Update()
    {
        if (HideAndSeekManager.Instance != null && !HideAndSeekManager.Instance.IsPlaying)
            return;

        if (player == null) return;

        attackCooldown -= Time.deltaTime;

        bool canSee = CanSeePlayer();
        Debug.Log($"[HideAndSeekAI] CanSeePlayer={canSee} isChasingPlayer={isChasingPlayer} distToPlayer={Vector3.Distance(transform.position, player.position):F1}");

        if (canSee)
        {
            isChasingPlayer = true;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
            Debug.Log("[HideAndSeekAI] Mode CHASSE — joueur détecté.");
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
                // Continue vers la dernière position connue
                agent.SetDestination(player.position);
            }
        }
        else
        {
            // Patrouille
            if (!agent.pathPending && agent.remainingDistance <= waypointStopDistance)
                GoToNextWaypoint();
        }

        // Mise à jour animation vitesse
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat(animSpeedParam, speed);
        }

        // Vérification capture
        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            if (attackCooldown <= 0f)
            {
                attackCooldown = 1f;
                if (animator != null)
                    animator.SetTrigger(animAttackParam);
                Debug.Log("[HideAndSeekAI] JOUEUR ATTRAPÉ — Défaite !");
                HideAndSeekManager.Instance?.TriggerDefeat();
            }
        }
    }

    /// <summary>Retourne vrai si le joueur est dans le cône de vision et sans obstacle entre eux.</summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle * 0.5f) return false;

        // Raycast pour vérifier les obstacles
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, dist, obstacleMask))
        {
            Debug.Log("[HideAndSeekAI] Joueur derrière un obstacle — non détecté.");
            return false;
        }

        return true;
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypoint].position);
        Debug.Log($"[HideAndSeekAI] Cap sur waypoint {currentWaypoint} : {waypoints[currentWaypoint].position}");
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    /// <summary>Dessine le cône de vision dans la SceneView pour debug.</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isChasingPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftDir = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * viewDistance);
        Gizmos.DrawRay(transform.position, rightDir * viewDistance);
    }
}
