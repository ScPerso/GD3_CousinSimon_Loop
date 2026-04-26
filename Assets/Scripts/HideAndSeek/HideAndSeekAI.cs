using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA de patrouille du mini-jeu Cache-cache.
///
/// Comportement standard :
///   - Patrouille entre waypoints.
///   - A chaque waypoint : s'arrete, joue l'animation LookAround UNE fois en entier, repart.
///   - Si le joueur entre dans le cone de vision : mode chasse.
///   - Si le joueur sort du loseSightDistance : retour en patrouille.
///   - Contact avec le joueur : TriggerDefeat.
///
/// Competence speciale (tous les <specialAbilityInterval> waypoints atteints) :
///   - Au lieu du LookAround standard, l'IA tourne sur elle-meme (spin) en T-pose 2-3 fois.
///   - Pendant le spin, un obstacle de cachette (HidingObstacle) glow et s'envole vers le haut.
///   - Au fil du temps, les obstacles disparaissent un par un.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class HideAndSeekAI : MonoBehaviour
{
    // Pas de singleton : plusieurs instances peuvent coexister dans la scene.

    // ── Patrouille ────────────────────────────────────────────────────────
    [Header("Patrol")]
    public Transform[] waypoints;
    public float waypointStopDistance = 0.5f;

    // ── Detection ─────────────────────────────────────────────────────────
    [Header("Detection")]
    public float viewDistance = 8f;
    [Range(1f, 180f)]
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    // ── Vitesses ──────────────────────────────────────────────────────────
    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed  = 5f;
    public float catchDistance = 1.2f;
    public float loseSightDistance = 12f;

    // ── Animator ──────────────────────────────────────────────────────────
    [Header("Animator Parameters")]
    [Tooltip("Float : magnitude de la vitesse (Walk/Run blend).")]
    public string animSpeedParam      = "Speed";
    [Tooltip("Trigger : joue l'animation d'attaque.")]
    public string animAttackParam     = "Attack";
    [Tooltip("Bool : passe en animation LookAround.")]
    public string animLookAroundParam = "LookAround";
    [Tooltip("Bool : passe en T-pose pour la competence speciale.")]
    public string animAbilityParam    = "Ability";
    [Tooltip("Nom exact de l'etat Animator du LookAround (pour lire sa duree).")]
    public string lookAroundStateName = "Looking";

    // ── Competence speciale ───────────────────────────────────────────────
    [Header("Special Ability")]
    [Tooltip("Nombre de waypoints atteints avant de declencher la competence speciale.")]
    public int specialAbilityInterval = 3;
    [Tooltip("Nombre de tours complets sur place lors du spin (2-3 recommande).")]
    [Range(1f, 5f)]
    public float spinRotations = 2.5f;
    [Tooltip("Duree totale du spin (secondes).")]
    public float spinDuration = 2f;
    [Tooltip("Duree de la pause apres le spin avant de repartir.")]
    public float postSpinPause = 0.5f;

    // ── Prive ─────────────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator     animator;
    private Transform    player;

    private int  currentWaypoint;
    private int  waypointsReachedCount;
    private bool isChasingPlayer;
    private bool isPausing;
    private float attackCooldown;

    // ─────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
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
            InterruptPause();
            isChasingPlayer = true;
            agent.isStopped = false;
            agent.speed     = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else if (isChasingPlayer)
        {
            if (Vector3.Distance(transform.position, player.position) > loseSightDistance)
            {
                isChasingPlayer = false;
                agent.speed     = patrolSpeed;
                Debug.Log("[HideAndSeekAI] Joueur perdu — retour en patrouille.");
                GoToNextWaypoint();
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }
        else if (!isPausing)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointStopDistance)
                StartCoroutine(HandleWaypointReached());
        }

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

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Patrol Logic

    /// <summary>Dispatche vers PauseAtWaypoint ou SpecialAbility selon le compteur.</summary>
    private IEnumerator HandleWaypointReached()
    {
        isPausing = true;
        agent.isStopped = true;
        waypointsReachedCount++;

        Debug.Log($"[HideAndSeekAI] Waypoint atteint ({waypointsReachedCount}).");

        if (waypointsReachedCount % specialAbilityInterval == 0)
            yield return StartCoroutine(SpecialAbility());
        else
            yield return StartCoroutine(LookAroundPause());

        agent.isStopped = false;
        isPausing = false;
        GoToNextWaypoint();
    }

    /// <summary>Joue l'animation LookAround UNE fois en entier avant de repartir.</summary>
    private IEnumerator LookAroundPause()
    {
        SetAnimBool(animLookAroundParam, true);

        // Attendre que l'Animator entre dans l'etat LookAround
        yield return new WaitUntil(() =>
            animator != null &&
            animator.GetCurrentAnimatorStateInfo(0).IsName(lookAroundStateName));

        // Lire la duree reelle du clip et attendre qu'il joue une fois
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log($"[HideAndSeekAI] LookAround — duree clip : {clipLength:F2}s.");
        yield return new WaitForSeconds(clipLength);

        SetAnimBool(animLookAroundParam, false);
    }

    /// <summary>
    /// Competence speciale : spin en T-pose + suppression d'un obstacle de cachette.
    /// </summary>
    private IEnumerator SpecialAbility()
    {
        Debug.Log("[HideAndSeekAI] COMPETENCE SPECIALE !");

        // Passer en T-pose (Ability = true gele/remplace l'animation courante)
        SetAnimBool(animAbilityParam, true);
        yield return null; // laisser l'animator transitionner

        // Supprimer un obstacle pendant le spin.
        // ClaimRandom() retire l'obstacle de la liste atomiquement :
        // une autre IA simultanee ne peut pas choisir le meme mur.
        HidingObstacle obstacle = HidingObstacle.ClaimRandom();
        if (obstacle != null)
        {
            Debug.Log($"[HideAndSeekAI] ({name}) Obstacle supprime : {obstacle.name}");
            obstacle.Remove();
        }
        else
        {
            Debug.Log($"[HideAndSeekAI] ({name}) Plus aucun obstacle a supprimer.");
        }

        // Spin : <spinRotations> tours complets en <spinDuration> secondes
        float elapsed    = 0f;
        float totalAngle = spinRotations * 360f;
        float startY     = transform.eulerAngles.y;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            float angle = Mathf.Lerp(0f, totalAngle, t);
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                startY + angle,
                transform.eulerAngles.z);
            yield return null;
        }

        // Pause courte apres le spin
        yield return new WaitForSeconds(postSpinPause);

        SetAnimBool(animAbilityParam, false);
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Choisir un waypoint aleatoire different du precedent
        if (waypoints.Length > 1)
        {
            int next;
            do { next = Random.Range(0, waypoints.Length); }
            while (next == currentWaypoint);
            currentWaypoint = next;
        }

        agent.SetDestination(waypoints[currentWaypoint].position);
        Debug.Log($"[HideAndSeekAI] Cap waypoint aleatoire {currentWaypoint}.");
    }

    /// <summary>Interrompt une pause en cours si l'IA voit le joueur.</summary>
    private void InterruptPause()
    {
        if (!isPausing) return;
        StopAllCoroutines();
        isPausing = false;
        SetAnimBool(animLookAroundParam, false);
        SetAnimBool(animAbilityParam, false);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Detection

    /// <summary>Retourne vrai si le joueur est dans le cone de vision sans obstacle.</summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance) return false;

        if (Vector3.Angle(transform.forward, dirToPlayer) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Helpers

    private void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
            animator.SetBool(paramName, value);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isChasingPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left  = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0,  viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left  * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
    }

    #endregion
}
