using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Onion system that spawns Pikmin from underground, manages Pikmin population,
/// and accepts pellets/pillflowers to create new Pikmin
/// </summary>
public class PikminOnion : MonoBehaviour
{
    [Header("Pikmin Spawning")]
    [SerializeField] private GameObject pikminPrefab;
    [SerializeField] private int maxPikminInOnion = 50;
    [SerializeField] private int currentPikminCount = 0;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private int maxActiveSpawns = 5; // Max Pikmin spawning at once

    [Header("Spawn Position & Ground Settings")]
    [SerializeField] private Transform spawnPoint; // Where Pikmin emerge from ground
    [SerializeField] private float spawnRadius = 3f; // Radius around spawn point
    [SerializeField] private float digDepth = 2f; // How deep underground they start
    [SerializeField] private float emergeSpeed = 3f; // Speed of emerging from ground
    [SerializeField] private float emergeHeight = 1f; // Height they pop up to
    [SerializeField] private LayerMask groundLayer;

    [Header("Pellet/Pillflower System")]
    [SerializeField] private Transform pelletReceivePoint; // Where pellets go into onion
    [SerializeField] private float pelletAbsorbRadius = 2f;
    [SerializeField] private float pelletAbsorbSpeed = 5f;
    [SerializeField] private ParticleSystem pelletAbsorbEffect;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private ParticleSystem onionGlowEffect;
    [SerializeField] private GameObject groundDigEffect; // Dirt particles when emerging

    [Header("Auto Spawn Settings")]
    [SerializeField] private bool autoSpawnEnabled = false;
    [SerializeField] private float autoSpawnInterval = 5f;

    [Header("Activation Settings")]
    [SerializeField] private bool startDeactivated = true; // Start buried in ground
    [SerializeField] private float buriedDepth = 3f; // How deep the onion is buried
    [SerializeField] private float riseSpeed = 2f; // Speed of rising from ground
    [SerializeField] private float activationRadius = 3f; // How close player needs to be to activate
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requirePlayerTouch = true; // If false, player just needs to be nearby
    [SerializeField] private ParticleSystem activationEffect;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;

    // Activation state
    public enum OnionState
    {
        Buried,      // Hidden underground, inactive
        Rising,      // Currently emerging from ground
        Active       // Fully emerged and functional
    }

    private OnionState currentState = OnionState.Buried;
    private Vector3 buriedPosition;
    private Vector3 activePosition;
    private bool isActivating = false;

    private Queue<int> pikminToSpawn = new Queue<int>(); // Queue of Pikmin waiting to spawn
    private List<GameObject> currentlySpawning = new List<GameObject>();
    private float lastSpawnTime = 0f;
    private float lastAutoSpawnTime = 0f;
    private List<Pellet> incomingPellets = new List<Pellet>();

    void Start()
    {
        // Auto-find spawn point if not set
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        // Auto-find pellet receive point if not set
        if (pelletReceivePoint == null)
        {
            pelletReceivePoint = transform;
        }

        // Validate settings
        if (pikminPrefab == null)
        {
            Debug.LogError($"[PikminOnion] No Pikmin prefab assigned to {gameObject.name}!");
        }

        // Setup activation state
        if (startDeactivated)
        {
            // Store the active position (current position)
            activePosition = transform.position;

            // Calculate buried position
            buriedPosition = transform.position - Vector3.up * buriedDepth;

            // Move onion underground
            transform.position = buriedPosition;
            currentState = OnionState.Buried;

            if (showDebugInfo)
                Debug.Log($"[PikminOnion] Starting in BURIED state at {buriedPosition}");
        }
        else
        {
            // Start active
            activePosition = transform.position;
            buriedPosition = transform.position - Vector3.up * buriedDepth;
            currentState = OnionState.Active;

            if (showDebugInfo)
                Debug.Log($"[PikminOnion] Starting in ACTIVE state");
        }

        if (showDebugInfo)
        {
            Debug.Log($"[PikminOnion] Initialized with {currentPikminCount} Pikmin stored");
        }
    }

    void Update()
    {
        // Check for player activation if buried
        if (currentState == OnionState.Buried)
        {
            CheckForPlayerActivation();
        }

        // Only process spawning/pellets when active
        if (currentState == OnionState.Active)
        {
            // Handle spawning queue
            ProcessSpawnQueue();

            // Auto spawn if enabled
            if (autoSpawnEnabled && Time.time - lastAutoSpawnTime >= autoSpawnInterval)
            {
                if (currentPikminCount > 0 && PikminManager.Instance != null &&
                    PikminManager.Instance.CanRegisterMorePikmin())
                {
                    RequestSpawnPikmin(1);
                    lastAutoSpawnTime = Time.time;
                }
            }

            // Process incoming pellets
            ProcessIncomingPellets();
        }
    }

    /// <summary>
    /// Check if player is nearby to activate the onion
    /// </summary>
    void CheckForPlayerActivation()
    {
        if (isActivating) return;

        // Find player if proximity activation is enabled
        if (!requirePlayerTouch)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= activationRadius)
                {
                    ActivateOnion();
                }
            }
        }
    }

    /// <summary>
    /// Activate the onion (can be called externally too)
    /// </summary>
    public void ActivateOnion()
    {
        if (currentState != OnionState.Buried || isActivating) return;

        if (showDebugInfo)
            Debug.Log("[PikminOnion] Activating onion! Rising from ground...");

        isActivating = true;
        currentState = OnionState.Rising;
        StartCoroutine(RiseFromGround());

        // Play activation effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        // Play ground dig effect
        if (groundDigEffect != null)
        {
            GameObject digFX = Instantiate(groundDigEffect, activePosition, Quaternion.identity);
            Destroy(digFX, 3f);
        }
    }

    /// <summary>
    /// Coroutine to handle the onion rising from underground
    /// </summary>
    IEnumerator RiseFromGround()
    {
        Vector3 startPos = buriedPosition;
        Vector3 targetPos = activePosition;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / riseSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out curve for smooth deceleration
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, easedT);

            yield return null;
        }

        // Ensure final position
        transform.position = activePosition;

        // Activate glow effect if available
        if (onionGlowEffect != null)
        {
            onionGlowEffect.Play();
        }

        // Set state to active
        currentState = OnionState.Active;
        isActivating = false;

        if (showDebugInfo)
            Debug.Log("[PikminOnion] Onion is now ACTIVE!");
    }

    /// <summary>
    /// Triggered when player touches the onion (for touch-based activation)
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTouch && currentState == OnionState.Buried)
        {
            if (other.CompareTag(playerTag))
            {
                ActivateOnion();
            }
        }
    }

    /// <summary>
    /// Alternative collision-based activation
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (requirePlayerTouch && currentState == OnionState.Buried)
        {
            if (collision.gameObject.CompareTag(playerTag))
            {
                ActivateOnion();
            }
        }
    }

    /// <summary>
    /// Request the onion to spawn a certain number of Pikmin
    /// </summary>
    public void RequestSpawnPikmin(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (currentPikminCount > 0)
            {
                pikminToSpawn.Enqueue(1);
                currentPikminCount--;

                if (showDebugInfo)
                    Debug.Log($"[PikminOnion] Queued Pikmin for spawn. Remaining in onion: {currentPikminCount}");
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogWarning("[PikminOnion] No Pikmin left in onion to spawn!");
                break;
            }
        }
    }

    /// <summary>
    /// Process the spawn queue and create Pikmin emerging from ground
    /// </summary>
    void ProcessSpawnQueue()
    {
        // Check cooldown and max active spawns
        if (pikminToSpawn.Count > 0 &&
            Time.time - lastSpawnTime >= spawnCooldown &&
            currentlySpawning.Count < maxActiveSpawns)
        {
            int toSpawn = pikminToSpawn.Dequeue();
            SpawnPikminFromGround();
            lastSpawnTime = Time.time;
        }

        // Clean up finished spawns
        currentlySpawning.RemoveAll(p => p == null);
    }

    /// <summary>
    /// Spawns a Pikmin from underground with emergence animation
    /// </summary>
    void SpawnPikminFromGround()
    {
        if (pikminPrefab == null)
        {
            Debug.LogError("[PikminOnion] Cannot spawn - no Pikmin prefab assigned!");
            return;
        }

        // Calculate random spawn position within radius
        Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 50f, groundLayer))
        {
            spawnPosition = hit.point;
        }

        // Start underground
        Vector3 undergroundPos = spawnPosition - Vector3.up * digDepth;

        // Instantiate Pikmin underground
        GameObject newPikmin = Instantiate(pikminPrefab, undergroundPos, Quaternion.identity);
        currentlySpawning.Add(newPikmin);

        // Disable physics temporarily during emergence
        Rigidbody rb = newPikmin.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable the Pikmin AI script temporarily
        Pikmin pikminScript = newPikmin.GetComponent<Pikmin>();
        if (pikminScript != null)
        {
            pikminScript.enabled = false;
        }

        // Start emergence coroutine
        StartCoroutine(EmergeFromGround(newPikmin, undergroundPos, spawnPosition));

        // Play spawn effect
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
        }

        if (showDebugInfo)
            Debug.Log($"[PikminOnion] Spawning Pikmin at {spawnPosition}");
    }

    /// <summary>
    /// Coroutine that handles the Pikmin emerging from underground
    /// </summary>
    IEnumerator EmergeFromGround(GameObject pikmin, Vector3 startPos, Vector3 groundPos)
    {
        float elapsed = 0f;
        float duration = digDepth / emergeSpeed;

        // Spawn ground dig effect
        if (groundDigEffect != null)
        {
            GameObject digFX = Instantiate(groundDigEffect, groundPos, Quaternion.identity);
            Destroy(digFX, 2f);
        }

        // Emerge from ground
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move from underground to ground level with a little pop
            float height = Mathf.Lerp(-digDepth, emergeHeight, t);
            if (pikmin != null)
            {
                pikmin.transform.position = groundPos + Vector3.up * height;
            }
            else
            {
                yield break; // Pikmin was destroyed
            }

            yield return null;
        }

        // Pop back down to ground level
        float popDuration = 0.3f;
        elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float height = Mathf.Lerp(emergeHeight, 0f, t);
            if (pikmin != null)
            {
                pikmin.transform.position = groundPos + Vector3.up * height;
            }
            else
            {
                yield break;
            }

            yield return null;
        }

        // Finalize position
        if (pikmin != null)
        {
            pikmin.transform.position = groundPos;

            // Re-enable physics
            Rigidbody rb = pikmin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
            }

            // Re-enable Pikmin AI
            Pikmin pikminScript = pikmin.GetComponent<Pikmin>();
            if (pikminScript != null)
            {
                pikminScript.enabled = true;
            }

            if (showDebugInfo)
                Debug.Log($"[PikminOnion] Pikmin fully emerged at {groundPos}");
        }
    }

    /// <summary>
    /// Called when a pellet is brought to the onion
    /// </summary>
    public void ReceivePellet(Pellet pellet)
    {
        if (pellet == null) return;

        // Check if we can accept more Pikmin
        if (currentPikminCount >= maxPikminInOnion)
        {
            if (showDebugInfo)
                Debug.LogWarning($"[PikminOnion] Onion is full! ({maxPikminInOnion} max)");
            return;
        }

        // Add to incoming pellets list
        if (!incomingPellets.Contains(pellet))
        {
            incomingPellets.Add(pellet);
            StartCoroutine(AbsorbPellet(pellet));
        }
    }

    /// <summary>
    /// Absorbs a pellet into the onion and converts it to Pikmin
    /// </summary>
    IEnumerator AbsorbPellet(Pellet pellet)
    {
        if (pellet == null) yield break;

        Transform pelletTransform = pellet.transform;
        Vector3 startPos = pelletTransform.position;

        // Disable pellet physics
        Rigidbody pelletRb = pellet.GetComponent<Rigidbody>();
        if (pelletRb != null)
        {
            pelletRb.isKinematic = true;
        }

        // Disable pellet collider
        Collider pelletCol = pellet.GetComponent<Collider>();
        if (pelletCol != null)
        {
            pelletCol.enabled = false;
        }

        // Move pellet to onion
        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, pelletReceivePoint.position) / pelletAbsorbSpeed;

        while (elapsed < duration)
        {
            if (pellet == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            pelletTransform.position = Vector3.Lerp(startPos, pelletReceivePoint.position, t);
            pelletTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            yield return null;
        }

        // Absorb the pellet
        if (pellet != null)
        {
            int pikminToCreate = pellet.GetPikminValue();

            // Add Pikmin to storage
            int added = Mathf.Min(pikminToCreate, maxPikminInOnion - currentPikminCount);
            currentPikminCount += added;

            // Play absorb effect
            if (pelletAbsorbEffect != null)
            {
                pelletAbsorbEffect.Play();
            }

            if (showDebugInfo)
                Debug.Log($"[PikminOnion] Absorbed pellet! +{added} Pikmin. Total in onion: {currentPikminCount}");

            // Destroy pellet
            incomingPellets.Remove(pellet);
            Destroy(pellet.gameObject);
        }
    }

    /// <summary>
    /// Process any incoming pellets
    /// </summary>
    void ProcessIncomingPellets()
    {
        // Check for nearby pellets that can be auto-absorbed
        Collider[] nearbyColliders = Physics.OverlapSphere(pelletReceivePoint.position, pelletAbsorbRadius);

        foreach (Collider col in nearbyColliders)
        {
            Pellet pellet = col.GetComponent<Pellet>();
            if (pellet != null && pellet.IsReadyForAbsorption())
            {
                ReceivePellet(pellet);
            }
        }
    }

    /// <summary>
    /// Add Pikmin directly to storage (for debugging or special cases)
    /// </summary>
    public void AddPikminToStorage(int count)
    {
        int added = Mathf.Min(count, maxPikminInOnion - currentPikminCount);
        currentPikminCount += added;

        if (showDebugInfo)
            Debug.Log($"[PikminOnion] Added {added} Pikmin to storage. Total: {currentPikminCount}");
    }

    // Public getters
    public int GetStoredPikminCount() => currentPikminCount;
    public int GetMaxStorage() => maxPikminInOnion;
    public int GetQueuedSpawnCount() => pikminToSpawn.Count;
    public bool IsFull() => currentPikminCount >= maxPikminInOnion;
    public OnionState GetCurrentState() => currentState;
    public bool IsActive() => currentState == OnionState.Active;

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Draw activation state visualization
        if (startDeactivated)
        {
            // Draw buried position
            Vector3 buriedPos = Application.isPlaying ? buriedPosition : transform.position - Vector3.up * buriedDepth;
            Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.5f); // Brown for buried
            Gizmos.DrawWireSphere(buriedPos, 1f);
            Gizmos.DrawLine(buriedPos, buriedPos + Vector3.up * buriedDepth);

            // Draw active position
            Vector3 activePos = Application.isPlaying ? activePosition : transform.position;
            Gizmos.color = new Color(0, 1, 1, 0.5f); // Cyan for active
            Gizmos.DrawWireSphere(activePos, 1f);

            // Draw activation radius if using proximity activation
            if (!requirePlayerTouch)
            {
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                Gizmos.DrawWireSphere(buriedPos, activationRadius);
            }
        }

        if (spawnPoint != null)
        {
            // Draw spawn radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);

            // Draw underground start position for Pikmin spawns
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position - Vector3.up * digDepth, 0.5f);
        }

        if (pelletReceivePoint != null)
        {
            // Draw pellet absorb radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pelletReceivePoint.position, pelletAbsorbRadius);
        }
    }
}
