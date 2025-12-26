using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Alerted, Combat, Searching, MovingToCover, Peeking }

    [Header("Stats")]
    public float hp = 50f;
    public float visionDistance = 40f;
    public float hearingDistance = 15f;
    public static float alertRadius = 25f;
    public float stoppingDistance = 1.2f;

    [Header("Search")]
    public float searchRadius = 6f;
    public int searchPoints = 4;
    public float searchTime = 6f;

    public Weapon weapon;

    protected EnemyState state = EnemyState.Idle;
    protected NavMeshAgent agent;
    protected Transform player;

    protected Vector3 lastKnownPlayerPos;
    protected Queue<Vector3> searchPointsQueue = new Queue<Vector3>();
    protected float stateTimer = 0f;
    protected float peekTimer = 0f;
    protected float coverTimer = 0f;

    protected Transform currentCover;

    [SerializeField]
    private AudioClip[] hittedSounds;

    [SerializeField]
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
    }

    protected virtual void Update()
    {
        // Динамический поиск игрока
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (player == null) return; // нет игрока, враг idle

        switch (state)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Alerted: UpdateAlerted(); break;
            case EnemyState.Combat: UpdateCombat(); break;
            case EnemyState.Searching: UpdateSearching(); break;
        }
    }

    // ==================== STATES ====================

    protected virtual void UpdateIdle()
    {
        agent.isStopped = true;

        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            EnterCombat();
        }
    }

    protected virtual void UpdateAlerted()
    {
        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPos);

        if (CanSeePlayer())
            EnterCombat();
        else if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            EnterSearching();
    }

    protected virtual void UpdateCombat()
    {
        AlertNearbyEnemies();
        lastKnownPlayerPos = player.position;

        // Стрельба только при видимости
        if (HasLineOfFire())
            CombatBehaviour();
    }

    protected virtual void UpdateSearching()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            state = EnemyState.Idle;
            return;
        }

        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            EnterCombat();
            return;
        }

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            if (searchPointsQueue.Count > 0)
            {
                agent.isStopped = false;
                agent.SetDestination(searchPointsQueue.Dequeue());
            }
            else
            {
                GenerateSearchPoints(lastKnownPlayerPos);
            }
        }
    }

    // ==================== TRANSITIONS ====================

    protected void EnterCombat()
    {
        state = EnemyState.Combat;

    }

    protected void EnterSearching()
    {
        state = EnemyState.Searching;
        stateTimer = searchTime;
        GenerateSearchPoints(lastKnownPlayerPos);
    }

    // ==================== VISION ====================

    protected bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) > visionDistance || (Quaternion.Angle(Quaternion.LookRotation(player.position - transform.position), Quaternion.LookRotation(transform.forward)) > 40f && Vector3.Distance(transform.position, player.position) > hearingDistance))
            return false;

        Vector3 dir = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, visionDistance, ~LayerMask.GetMask("Room") & ~LayerMask.GetMask("EnemyBody")))
            return hit.transform.CompareTag("Player");

        return false;
    }

    protected bool HasLineOfFire()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, 50f, ~LayerMask.GetMask("Room") & ~LayerMask.GetMask("EnemyBody")))
            return hit.transform.CompareTag("Player");
        return false;
    }

    // ==================== SEARCH ====================

    protected void GenerateSearchPoints(Vector3 center)
    {
        searchPointsQueue.Clear();
        for (int i = 0; i < searchPoints; i++)
        {
            Vector3 point = center + Random.insideUnitSphere * searchRadius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                searchPointsQueue.Enqueue(hit.position);
        }
    }

    // ==================== ALERT ====================

    protected void AlertNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, alertRadius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && e != this && e.state == EnemyState.Idle)
                e.OnAlert(lastKnownPlayerPos);
        }
    }

    public static void AlertNearbyEnemies(Vector3 point)
    {
        Collider[] hits = Physics.OverlapSphere(point, alertRadius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && e.state == EnemyState.Idle)
                e.OnAlert(point);
        }
    }

    public virtual void OnAlert(Vector3 pos)
    {
        lastKnownPlayerPos = pos;
        if (state == EnemyState.Idle)
            state = EnemyState.Alerted;
    }

    // ==================== COMBAT ====================

    protected abstract void CombatBehaviour();

    // ==================== DAMAGE ====================

    public virtual void TakeDamage(float dmg)
    {
        audioSource.PlayOneShot(hittedSounds[UnityEngine.Random.Range(0, hittedSounds.Length)]);

        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
    }
}
