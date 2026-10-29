
using System.Collections;
using UnityEngine;

public class FrogSpawner : MonoBehaviour
{
    [Header("Frog Prefab")]
    public GameObject frogPrefab;

    [Header("Spawn Timing")]
    public float startingInterval = 1.1f;
    public float minimumInterval = 0.65f;
    public float intervalDecrease = 0.025f;

    [Header("Spawn Area")]
    [Range(0f, 1f)] public float minViewportX = 0.1f;
    [Range(0f, 1f)] public float maxViewportX = 0.9f;
    [Range(0f, 1f)] public float minViewportY = 0.1f;
    [Range(0f, 1f)] public float maxViewportY = 0.78f;

    private Coroutine spawnCoroutine;
    private float currentInterval;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        currentInterval = startingInterval;
    }

    public void BeginSpawning()
    {
        StopSpawning();

        currentInterval = startingInterval;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentInterval);

            SpawnFrog();

            // Gradually make frogs appear more frequently.
            currentInterval = Mathf.Max(
                minimumInterval,
                currentInterval - intervalDecrease
            );
        }
    }

    private void SpawnFrog()
    {
        if (frogPrefab == null)
        {
            Debug.LogError(
                "FrogSpawner: Assign the Frog prefab in the Inspector.",
                this
            );
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError(
                "FrogSpawner: No camera tagged MainCamera was found.",
                this
            );
            return;
        }

        // Choose a random position within the visible camera area.
        float x = Random.Range(minViewportX, maxViewportX);
        float y = Random.Range(minViewportY, maxViewportY);

        // Convert the screen-relative position into a world position.
        float cameraDistance = -mainCamera.transform.position.z;

        Vector3 viewportPosition = new Vector3(
            x,
            y,
            cameraDistance
        );

        Vector3 worldPosition =
            mainCamera.ViewportToWorldPoint(viewportPosition);

        worldPosition.z = 0f;

        Instantiate(
            frogPrefab,
            worldPosition,
            Quaternion.identity
        );
    }
}
