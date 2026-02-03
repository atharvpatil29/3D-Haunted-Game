using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SmartGhostAI : MonoBehaviour
{
    public Transform player;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Vision Settings")]
    public float sightRange = 20f;
    public float fieldOfView = 90f;

    [Header("Chase & Search Settings")]
    public float chaseMemoryDuration = 8f;
    public float searchDuration = 10f;
    public float patrolSpeed = 4f;
    public float chaseSpeed = 8f;

    [Header("Audio Settings")]
    public float lullabyRange = 15f;
    public AudioSource lullabyAudio;
    public AudioSource chaseAudio;

    [Header("Player Catch Settings")]
    public float catchDistance = 2f; // New field for catching distance

    private NavMeshAgent agent;
    private Vector3 lastSeenPosition;
    private float timeSinceLastSeen = Mathf.Infinity;
    private float searchTimer = 0f;
    private bool isSearching = false;

    private enum State { Patrolling, Chasing, Searching }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        if (lullabyAudio != null) lullabyAudio.Play();
    }

    void Update()
    {
        bool canSeePlayer = CanSeePlayer();

        if (PlayerHiding.isHiding)
        {
            canSeePlayer = false;
        }

        if (canSeePlayer)
        {
            lastSeenPosition = player.position;
            timeSinceLastSeen = 0f;
            currentState = State.Chasing;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;

            case State.Chasing:
                Chase();
                TryCatchPlayer(); // New logic to check for catch
                if (timeSinceLastSeen > chaseMemoryDuration)
                {
                    currentState = State.Searching;
                    searchTimer = 0f;
                    isSearching = true;
                }
                break;

            case State.Searching:
                Search();
                break;
        }

        HandleAudio();
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < sightRange)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < fieldOfView / 2f)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 50f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 50f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
        }
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void Search()
    {
        agent.speed = patrolSpeed;
        if (isSearching)
        {
            agent.SetDestination(lastSeenPosition + Random.insideUnitSphere * 10f);
            isSearching = false;
        }

        searchTimer += Time.deltaTime;

        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            isSearching = true;
        }

        if (searchTimer > searchDuration)
        {
            currentState = State.Patrolling;
        }
    }

    void HandleAudio()
    {
        if (currentState == State.Chasing)
        {
            if (lullabyAudio.isPlaying) lullabyAudio.Stop();
            if (!chaseAudio.isPlaying) chaseAudio.Play();
        }
        else
        {
            if (!lullabyAudio.isPlaying) lullabyAudio.Play();
            if (chaseAudio.isPlaying) chaseAudio.Stop();
        }
    }

    void TryCatchPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) <= catchDistance && !PlayerHiding.isHiding)
        {
            GameManager.Instance.OnPlayerCaught();
            currentState = State.Patrolling;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vision Range (blue circle)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Lullaby Audio Range (yellow circle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lullabyRange);

        // Vision Cone (green lines)
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * sightRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * sightRange);

        // Catch Range (red circle)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
