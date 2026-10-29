
using System.Collections;
using UnityEngine;

public class Frog : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Gameplay")]
    public float lifetime = 4.5f;

    [Header("Death Animation")]
    public float deathDuration = 0.3f;
    public float deathSpinSpeed = 720f;

    private Rigidbody2D rb;
    private Collider2D frogCollider;
    private bool isDead;
    private float elapsedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        frogCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (rb == null)
        {
            Debug.LogError("Frog: A Rigidbody2D component is required.", this);
            enabled = false;
            return;
        }

        // Choose a random direction.
        float angle = Random.Range(0f, 360f)
                      * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        );

        // Start moving in that direction.
        rb.AddForce(
            direction * moveSpeed,
            ForceMode2D.Impulse
        );

        // Rotate the sprite to face its direction.
        float faceAngle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg - 90f;

        transform.rotation =
            Quaternion.Euler(0f, 0f, faceAngle);
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null || !rb.simulated)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
        Vector2 velocity = rb.linearVelocity;
        bool bounced = false;

        if (viewportPosition.x < 0.08f)
        {
            viewportPosition.x = 0.08f;
            velocity.x = Mathf.Abs(velocity.x);
            bounced = true;
        }
        else if (viewportPosition.x > 0.92f)
        {
            viewportPosition.x = 0.92f;
            velocity.x = -Mathf.Abs(velocity.x);
            bounced = true;
        }

        if (viewportPosition.y < 0.08f)
        {
            viewportPosition.y = 0.08f;
            velocity.y = Mathf.Abs(velocity.y);
            bounced = true;
        }
        else if (viewportPosition.y > 0.78f)
        {
            viewportPosition.y = 0.78f;
            velocity.y = -Mathf.Abs(velocity.y);
            bounced = true;
        }

        if (bounced)
        {
            float cameraDistance = -camera.transform.position.z;
            Vector3 worldPosition = camera.ViewportToWorldPoint(
                new Vector3(viewportPosition.x, viewportPosition.y, cameraDistance)
            );
            rb.position = new Vector2(worldPosition.x, worldPosition.y);
            rb.linearVelocity = velocity;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        // Only count lifetime during active gameplay.
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lifetime)
        {
            Escape();
        }
    }

    public void Kill()
    {
        if (isDead)
            return;

        isDead = true;

        // Prevent repeated clicks and further movement.
        if (frogCollider != null)
            frogCollider.enabled = false;

        rb.simulated = false;

        if (GameManager.Instance != null)
            GameManager.Instance.FrogKilled();

        StartCoroutine(DeathRoutine());
    }

    private void Escape()
    {
        if (isDead)
            return;

        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.FrogEscaped();

        Destroy(gameObject);
    }

    private IEnumerator DeathRoutine()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / deathDuration
            );

            transform.Rotate(
                0f, 0f,
                deathSpinSpeed * Time.deltaTime
            );

            transform.localScale = Vector3.Lerp(
                originalScale,
                Vector3.zero,
                progress
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}
