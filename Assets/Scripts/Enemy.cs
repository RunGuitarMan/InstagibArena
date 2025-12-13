using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 4f;
    public float attackRange = 25f;
    public float attackCooldown = 4f;
    public float accuracy = 0.08f; // 0-1, basically stormtroopers
    public float detectionRange = 40f;
    public bool friendlyFire = true; // shoot other bots too

    [Header("Visual")]
    public Color enemyColor = Color.red;
    public Color beamColor = Color.red;
    public float beamDuration = 0.15f;

    [Header("Movement")]
    public float strafeSpeed = 5f;
    public float strafeChangeInterval = 1.5f;
    public float minDistanceToPlayer = 8f;
    public float maxDistanceToPlayer = 25f;

    private Transform player;
    private float lastAttackTime = -999f;
    private float strafeDirection = 0f;
    private float nextStrafeChange;
    private LineRenderer beamLine;
    private bool isDead = false;
    private Renderer meshRenderer;
    private CharacterController controller;

    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetupVisuals();
        SetupBeam();
        controller = GetComponent<CharacterController>();
        nextStrafeChange = Time.time + strafeChangeInterval;
    }

    void SetupVisuals()
    {
        meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
        {
            // Try URP shader first
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            meshRenderer.material = new Material(shader);
            meshRenderer.material.SetColor("_BaseColor", enemyColor);
            meshRenderer.material.color = enemyColor;
        }
    }

    void SetupBeam()
    {
        beamLine = gameObject.AddComponent<LineRenderer>();
        beamLine.positionCount = 2;
        beamLine.startWidth = 0.08f;
        beamLine.endWidth = 0.04f;
        beamLine.material = new Material(Shader.Find("Sprites/Default"));
        beamLine.startColor = beamColor;
        beamLine.endColor = beamColor;
        beamLine.enabled = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer > detectionRange) return;

        // Look at player
        LookAtPlayer();

        // Movement AI
        HandleMovement(distanceToPlayer);

        // Attack if in range and can see player
        if (distanceToPlayer <= attackRange && CanAttack && CanSeePlayer())
        {
            Attack();
        }
    }

    void LookAtPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleMovement(float distanceToPlayer)
    {
        // Update strafe direction
        if (Time.time >= nextStrafeChange)
        {
            strafeDirection = Random.Range(-1f, 1f);
            nextStrafeChange = Time.time + strafeChangeInterval + Random.Range(-0.5f, 0.5f);
        }

        Vector3 moveDirection = Vector3.zero;

        // Move towards or away from player based on distance
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer < minDistanceToPlayer)
        {
            // Too close, back away
            moveDirection -= directionToPlayer * moveSpeed;
        }
        else if (distanceToPlayer > maxDistanceToPlayer)
        {
            // Too far, move closer
            moveDirection += directionToPlayer * moveSpeed;
        }

        // Strafe
        Vector3 strafeDir = Vector3.Cross(Vector3.up, directionToPlayer);
        moveDirection += strafeDir * strafeDirection * strafeSpeed;

        // Apply movement
        if (controller != null)
        {
            moveDirection.y = -9.8f; // Gravity
            controller.Move(moveDirection * Time.deltaTime);
        }
        else
        {
            transform.position += moveDirection * Time.deltaTime;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, out hit, attackRange))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 targetPoint = player.position + Vector3.up * 0.5f;

        // Apply accuracy - almost always miss lol
        bool willHit = Random.value <= accuracy;

        if (!willHit)
        {
            // Miss - HUGE offset, these guys are blind
            Vector3 missOffset = Random.insideUnitSphere * 8f;
            targetPoint += missOffset;
        }

        Vector3 direction = (targetPoint - origin).normalized;
        Vector3 endPoint = origin + direction * attackRange;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, attackRange))
        {
            endPoint = hit.point;

            // Hit player
            if (hit.collider.CompareTag("Player"))
            {
                PlayerController playerController = hit.collider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.Die();
                }
            }
            // Friendly fire - hit other bots
            else if (friendlyFire)
            {
                Enemy otherEnemy = hit.collider.GetComponent<Enemy>();
                if (otherEnemy == null)
                    otherEnemy = hit.collider.GetComponentInParent<Enemy>();

                if (otherEnemy != null && otherEnemy != this)
                {
                    otherEnemy.Die();
                    GameManager.Instance?.OnEnemyKilled(); // player gets credit lol
                }
            }
        }

        // Show beam
        StartCoroutine(ShowBeam(origin, endPoint));
    }

    IEnumerator ShowBeam(Vector3 start, Vector3 end)
    {
        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);

        float elapsed = 0f;
        Color startColor = beamColor;

        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / beamDuration);
            Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            beamLine.startColor = fadeColor;
            beamLine.endColor = fadeColor;
            yield return null;
        }

        beamLine.enabled = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Notify spawn manager
        SpawnManager.Instance?.OnEnemyDied(this);

        // Destroy after short delay for effect
        Destroy(gameObject, 0.1f);
    }

    public void Respawn(Vector3 position)
    {
        isDead = false;
        transform.position = position;
        lastAttackTime = Time.time; // Small delay before first attack after spawn
    }
}
