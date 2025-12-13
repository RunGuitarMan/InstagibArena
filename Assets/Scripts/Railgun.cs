using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Railgun : MonoBehaviour
{
    [Header("Railgun Settings")]
    public float cooldown = 1.5f;
    public float range = 500f;
    public LayerMask hitMask = -1;

    [Header("Rail Jump")]
    public float railJumpRadius = 5f;
    public float railJumpForce = 10f;
    public float railJumpBaseUpForce = 6f;
    public float maxEffectiveHeight = 1.5f; // Height where boost starts falling off
    public float jumpComboBonus = 1.3f; // 30% bonus when jump+shoot together

    [Header("Visual")]
    public Color beamColor = Color.cyan;
    public float beamDuration = 0.15f;
    public float beamWidth = 0.08f;

    [Header("Audio")]
    public AudioClip fireSound;

    [Header("References")]
    public Transform firePoint;

    private float lastFireTime = -999f;
    private LineRenderer beamLine;
    private AudioSource audioSource;
    private Camera playerCamera;
    private PlayerController playerController;

    public bool CanFire => Time.time >= lastFireTime + cooldown;
    public float CooldownRemaining => Mathf.Max(0, (lastFireTime + cooldown) - Time.time);
    public float CooldownPercent => 1f - (CooldownRemaining / cooldown);

    void Start()
    {
        playerCamera = Camera.main;
        playerController = GetComponentInParent<PlayerController>();
        SetupBeamRenderer();
        SetupAudio();

        if (firePoint == null)
            firePoint = playerCamera?.transform;
    }

    void SetupBeamRenderer()
    {
        beamLine = gameObject.AddComponent<LineRenderer>();
        beamLine.positionCount = 2;
        beamLine.startWidth = beamWidth;
        beamLine.endWidth = beamWidth * 0.5f;
        beamLine.material = new Material(Shader.Find("Sprites/Default"));
        beamLine.startColor = beamColor;
        beamLine.endColor = beamColor;
        beamLine.enabled = false;
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        // Don't shoot when paused
        if (Time.timeScale == 0f) return;

        // Don't shoot if player is dead
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player != null && player.IsDead) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            TryFire();
        }
    }

    public void TryFire()
    {
        if (!CanFire) return;

        Fire();
    }

    void Fire()
    {
        lastFireTime = Time.time;

        // Play sound
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Raycast from camera center
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        Vector3 endPoint = origin + direction * range;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range, hitMask))
        {
            endPoint = hit.point;

            // Check if we hit an enemy
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy == null)
                enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.Die();
                GameManager.Instance?.OnEnemyKilled();
            }

            // Rail jump - if hit is close to player, boost them
            if (playerController != null)
            {
                float distanceToHit = Vector3.Distance(playerController.transform.position, hit.point);
                if (distanceToHit <= railJumpRadius)
                {
                    // Calculate height factor - quadratic falloff for faster decay with height
                    float distanceToGround = playerController.GetDistanceToGround();
                    float normalizedHeight = Mathf.Clamp01(distanceToGround / maxEffectiveHeight);
                    float heightFactor = (1f - normalizedHeight) * (1f - normalizedHeight); // Quadratic falloff

                    // Bonus for jump+shoot combo (pressing jump at same time as shooting)
                    float comboMultiplier = playerController.WasJumpPressedRecently() ? jumpComboBonus : 1f;

                    // Calculate vertical force with height scaling
                    float upForce = railJumpBaseUpForce * heightFactor * comboMultiplier;

                    // Cap at triple jump height (jumpForce * 3)
                    float maxUpForce = playerController.JumpForce * 3f;
                    upForce = Mathf.Min(upForce, maxUpForce);

                    // If already moving up, subtract current velocity to prevent stacking
                    if (playerController.Velocity.y > 0)
                    {
                        upForce = Mathf.Max(0, upForce - playerController.Velocity.y);
                    }

                    // Push player away from hit point + calculated upward
                    Vector3 pushDirection = (playerController.transform.position - hit.point).normalized;
                    Vector3 jumpBoost = pushDirection * railJumpForce + Vector3.up * upForce;
                    playerController.AddVelocity(jumpBoost);
                }
            }
        }

        // Show beam
        StartCoroutine(ShowBeam(origin + direction * 0.5f, endPoint));
    }

    IEnumerator ShowBeam(Vector3 start, Vector3 end)
    {
        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);

        // Fade out effect
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
}
