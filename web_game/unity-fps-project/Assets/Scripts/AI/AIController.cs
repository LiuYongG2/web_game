using UnityEngine;
using UnityEngine.AI;

public enum AIState { Idle, Patrol, Chase, Attack, Cover, Dead }
public enum AITeam { Red, Blue }

public class AIController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 3.5f;
    public float sprintSpeed = 5.5f;
    public float attackRange = 18f;
    public float detectionRange = 25f;
    public float fieldOfView = 120f;
    public int damage = 12;
    public float fireRate = 0.8f;
    public float accuracy = 0.6f;

    [Header("Team")]
    public AITeam team = AITeam.Red;

    [Header("References")]
    public Transform headTransform;

    private int currentHealth;
    private AIState currentState = AIState.Patrol;
    private NavMeshAgent agent;
    private Transform target;
    private float nextFireTime;
    private float patrolTimer;
    private Vector3 patrolTarget;
    private float stateTimer;
    private Renderer[] renderers;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = 2f;
        renderers = GetComponentsInChildren<Renderer>();
        SetRandomPatrolPoint();
    }

    void Update()
    {
        if (currentState == AIState.Dead) return;

        FindTarget();

        switch (currentState)
        {
            case AIState.Patrol: UpdatePatrol(); break;
            case AIState.Chase: UpdateChase(); break;
            case AIState.Attack: UpdateAttack(); break;
            case AIState.Cover: UpdateCover(); break;
        }
    }

    void FindTarget()
    {
        float closest = detectionRange;
        target = null;

        foreach (var player in FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closest)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle < fieldOfView / 2f || dist < 5f)
                {
                    closest = dist;
                    target = player.transform;
                }
            }
        }

        foreach (var other in FindObjectsByType<AIController>(FindObjectsSortMode.None))
        {
            if (other == this || other.team == team || other.currentState == AIState.Dead) continue;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < closest)
            {
                closest = dist;
                target = other.transform;
            }
        }

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= attackRange) currentState = AIState.Attack;
            else currentState = AIState.Chase;
        }
        else if (currentState != AIState.Patrol)
        {
            currentState = AIState.Patrol;
        }
    }

    void UpdatePatrol()
    {
        agent.speed = moveSpeed;
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                SetRandomPatrolPoint();
                patrolTimer = 2f + Random.Range(0f, 3f);
            }
        }
    }

    void UpdateChase()
    {
        if (target == null) { currentState = AIState.Patrol; return; }
        agent.speed = sprintSpeed;
        agent.SetDestination(target.position);
    }

    void UpdateAttack()
    {
        if (target == null) { currentState = AIState.Patrol; return; }
        agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate + Random.Range(-0.2f, 0.2f);
            if (Random.value < accuracy)
            {
                PlayerHealth ph = target.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
                AIController ai = target.GetComponent<AIController>();
                if (ai != null) ai.TakeDamage(damage);
            }
        }

        if (currentHealth < maxHealth * 0.3f && Random.value < 0.01f)
            currentState = AIState.Cover;
    }

    void UpdateCover()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            currentState = AIState.Chase;
            stateTimer = 3f + Random.Range(0f, 2f);
            return;
        }

        if (target != null)
        {
            Vector3 awayDir = (transform.position - target.position).normalized;
            Vector3 coverPoint = transform.position + awayDir * 5f + Random.insideUnitSphere * 2f;
            coverPoint.y = transform.position.y;
            agent.SetDestination(coverPoint);
        }
    }

    void SetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * 15f;
        randomDir.y = 0;
        randomDir += transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 15f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public void TakeDamage(int amount)
    {
        if (currentState == AIState.Dead) return;
        currentHealth -= amount;

        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
                r.material.SetColor("_EmissionColor", Color.red * 2f);
        }
        Invoke(nameof(ResetEmission), 0.1f);

        if (currentHealth <= 0) Die();
        else if (target == null) currentState = AIState.Chase;
    }

    void ResetEmission()
    {
        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
                r.material.SetColor("_EmissionColor", Color.black);
        }
    }

    void Die()
    {
        currentState = AIState.Dead;
        if (agent != null) agent.enabled = false;
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), 10f);
    }

    void Respawn()
    {
        currentHealth = maxHealth;
        currentState = AIState.Patrol;
        if (agent != null) agent.enabled = true;
        gameObject.SetActive(true);
        SetRandomPatrolPoint();
    }

    public int Health => currentHealth;
    public AITeam Team => team;
    public AIState State => currentState;
}
