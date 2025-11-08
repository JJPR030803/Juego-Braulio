using UnityEngine;

public class PikminLauncher : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float minLaunchForce = 5f;
    [SerializeField] private float maxLaunchForce = 15f;
    [SerializeField] private float currentLaunchForce = 10f;
    [SerializeField] private float arcHeight = 3f; // How high the arc goes
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Aim Settings")]
    [SerializeField] private float maxLaunchDistance = 15f;
    [SerializeField] private Camera playerCamera;
    
    [Header("Trajectory Visual")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryResolution = 30;
    [SerializeField] private Color validTrajectoryColor = Color.green;
    [SerializeField] private Color invalidTrajectoryColor = Color.red;
    [SerializeField] private GameObject landingMarker; // Optional visual marker
    
    [Header("Pikmin Settings")]
    [SerializeField] private GameObject pikminPrefab;
    [SerializeField] private Transform launchPoint; // Where pikmin spawns from
    
    [Header("Limit Feedback")]
    [SerializeField] private bool showLimitWarning = true;
    [SerializeField] private Color limitReachedTrajectoryColor = Color.gray;
    [SerializeField] private UnityEngine.UI.Text pikminCountText; // Optional UI text
    [SerializeField] private GameObject limitReachedUI; // Optional UI panel to show when limit reached
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isAiming = false;
    private Vector3 targetPoint;
    private Vector3 launchVelocity;
    private bool canLaunch = false;
    
    void Start()
    {
        // Validate setup
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }
        
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
            trajectoryLine.positionCount = trajectoryResolution;
        }
        
        if (landingMarker != null)
            landingMarker.SetActive(false);
    }
    
    bool ValidateSetup()
    {
        bool isValid = true;
        
        // Check if pikmin prefab has required components
        if (pikminPrefab != null)
        {
            // Temporarily instantiate to check components
            GameObject tempPikmin = Instantiate(pikminPrefab);
            tempPikmin.SetActive(false);
            
            Rigidbody rb = tempPikmin.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("PikminLauncher: Pikmin prefab is missing Rigidbody component!");
                isValid = false;
            }
            else
            {
                // Check Rigidbody settings
                if (rb.isKinematic)
                {
                    Debug.LogWarning("PikminLauncher: Pikmin prefab Rigidbody is kinematic. It won't be affected by physics!");
                }
            }
            
            // Check if prefab has collider
            Collider col = tempPikmin.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning("PikminLauncher: Pikmin prefab is missing Collider component!");
            }
            
            DestroyImmediate(tempPikmin);
        }
        else
        {
            Debug.LogError("PikminLauncher: Pikmin prefab is not assigned!");
            isValid = false;
        }
        
        // Check launch point
        if (launchPoint == null)
        {
            Debug.LogError("PikminLauncher: Launch point is not assigned! Please assign a transform for the launch position.");
            isValid = false;
        }
        
        // Check ground layer
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("PikminLauncher: Ground layer is not set. Trajectory calculation may not work properly!");
        }
        
        return isValid;
    }
    
    void Update()
    {
        // Start aiming
        if (Input.GetMouseButtonDown(0))
        {
            StartAiming();
        }
        
        // Update aim while holding
        if (Input.GetMouseButton(0) && isAiming)
        {
            UpdateAim();
        }
        
        // Launch on release
        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            if (canLaunch)
            {
                LaunchPikmin();
            }
            else
            {
                Debug.LogWarning("Cannot launch - no valid target found!");
                StopAiming();
            }
        }
        
        // Cancel aiming with right click
        if (Input.GetMouseButtonDown(1) && isAiming)
        {
            StopAiming();
        }
        
        // Adjust power with scroll wheel (optional)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && isAiming)
        {
            currentLaunchForce = Mathf.Clamp(
                currentLaunchForce + scroll * 5f,
                minLaunchForce,
                maxLaunchForce
            );
            UpdateAim(); // Refresh trajectory
        }
    }
    
    void StartAiming()
    {
        isAiming = true;
        canLaunch = false;
        
        if (trajectoryLine != null)
            trajectoryLine.enabled = true;
        if (landingMarker != null)
            landingMarker.SetActive(true);
            
        if (showDebugInfo)
            Debug.Log("Started aiming...");
    }
    
    void StopAiming()
    {
        isAiming = false;
        canLaunch = false;
        
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
        if (landingMarker != null)
            landingMarker.SetActive(false);
    }
    
    void UpdateAim()
    {
        // Raycast to find target point
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Use a larger raycast distance and check if we hit anything
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if we hit the ground layer or any surface
            if (groundLayer.value == 0 || (groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                targetPoint = hit.point;
                canLaunch = true;
                
                // Clamp distance
                Vector3 directionToTarget = targetPoint - launchPoint.position;
                float distance = directionToTarget.magnitude;
                
                if (distance > maxLaunchDistance)
                {
                    targetPoint = launchPoint.position + directionToTarget.normalized * maxLaunchDistance;
                }
                
                // Calculate launch velocity
                launchVelocity = CalculateLaunchVelocity(launchPoint.position, targetPoint, arcHeight);
                
                // Check if we're at limit
                bool atLimit = IsAtPikminLimit();
                
                // Draw trajectory
                DrawTrajectory(atLimit);
                
                // Update landing marker
                if (landingMarker != null)
                {
                    landingMarker.transform.position = targetPoint;
                    // Optional: Change landing marker color when at limit
                    Renderer markerRenderer = landingMarker.GetComponent<Renderer>();
                    if (markerRenderer != null)
                    {
                        markerRenderer.material.color = atLimit ? limitReachedTrajectoryColor : validTrajectoryColor;
                    }
                }
                
                // Update UI if available
                UpdatePikminCountUI();
            }
            else
            {
                canLaunch = false;
                if (trajectoryLine != null)
                    trajectoryLine.enabled = false;
            }
        }
        else
        {
            canLaunch = false;
            if (trajectoryLine != null)
                trajectoryLine.enabled = false;
        }
    }
    
    Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = end.y - start.y;
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
        
        // Handle case where target is at same position
        if (displacementXZ.magnitude < 0.001f)
        {
            return Vector3.up * 10f; // Just launch upward
        }
        
        float time = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;
        
        return velocityXZ + velocityY;
    }
    
    void DrawTrajectory()
    {
        DrawTrajectory(false);
    }
    
    void DrawTrajectory(bool atLimit)
    {
        if (trajectoryLine == null || !canLaunch) return;
        
        trajectoryLine.enabled = true;
        Vector3[] points = new Vector3[trajectoryResolution];
        Vector3 startPosition = launchPoint.position;
        Vector3 startVelocity = launchVelocity;
        
        int actualPointCount = trajectoryResolution;
        
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * 0.1f;
            points[i] = startPosition + startVelocity * time + 0.5f * Physics.gravity * time * time;
            
            // Stop trajectory at ground
            if (i > 0) // Skip first point
            {
                Vector3 rayStart = points[i-1];
                Vector3 rayDirection = points[i] - points[i-1];
                float rayDistance = rayDirection.magnitude;
                
                if (Physics.Raycast(rayStart, rayDirection.normalized, out RaycastHit hit, rayDistance))
                {
                    points[i] = hit.point;
                    actualPointCount = i + 1;
                    break;
                }
            }
        }
        
        // Set only the actual points we calculated
        trajectoryLine.positionCount = actualPointCount;
        System.Array.Resize(ref points, actualPointCount);
        trajectoryLine.SetPositions(points);
        
        // Color based on validity and limit
        float distance = Vector3.Distance(launchPoint.position, targetPoint);
        Color trajectoryColor;
        
        if (atLimit)
        {
            trajectoryColor = limitReachedTrajectoryColor;
        }
        else if (distance <= maxLaunchDistance)
        {
            trajectoryColor = validTrajectoryColor;
        }
        else
        {
            trajectoryColor = invalidTrajectoryColor;
        }
        
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;
    }
    
    bool IsAtPikminLimit()
    {
        if (PikminManager.Instance != null)
        {
            return !PikminManager.Instance.CanRegisterMorePikmin();
        }
        return false;
    }
    
    void UpdatePikminCountUI()
    {
        if (pikminCountText != null && PikminManager.Instance != null)
        {
            int current = PikminManager.Instance.GetPikminCount();
            int max = PikminManager.Instance.GetMaxPikmin();
            pikminCountText.text = $"Pikmin: {current}/{max}";
            
            // Optional: Change color when near limit
            if (current >= max)
            {
                pikminCountText.color = limitReachedTrajectoryColor;
            }
            else if (current >= max * 0.9f) // 90% full
            {
                pikminCountText.color = Color.yellow;
            }
            else
            {
                pikminCountText.color = Color.white;
            }
        }
    }
    
    void LaunchPikmin()
    {
        if (pikminPrefab != null && launchPoint != null && canLaunch)
        {
            // Check if we can spawn more Pikmin
            bool canSpawn = true;
            string limitMessage = "";
            
            if (PikminManager.Instance != null)
            {
                // Manager exists, check its limit
                if (!PikminManager.Instance.CanRegisterMorePikmin())
                {
                    canSpawn = false;
                    limitMessage = $"Pikmin limit reached! ({PikminManager.Instance.GetPikminCount()}/{PikminManager.Instance.GetMaxPikmin()})";
                }
            }
            else
            {
                // No manager, check if Pikmin script has a standalone limit
                // You could implement a static counter in Pikmin script if needed
                // For now, we'll allow spawning without manager
            }
            
            if (!canSpawn)
            {
                Debug.LogWarning(limitMessage);
                
                // Optional: Show UI feedback
                if (limitReachedUI != null)
                {
                    limitReachedUI.SetActive(true);
                    // Hide it after a few seconds
                    Invoke(nameof(HideLimitUI), 2f);
                }
                
                // Flash the trajectory line
                if (trajectoryLine != null && showLimitWarning)
                {
                    StartCoroutine(FlashTrajectory());
                }
                
                StopAiming();
                return;
            }
            
            // Spawn pikmin
            GameObject pikmin = Instantiate(pikminPrefab, launchPoint.position, Quaternion.identity);
            
            // Make sure it's not a child of the player
            pikmin.transform.parent = null;
            
            // Apply launch velocity
            Rigidbody rb = pikmin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = launchVelocity;
                
                // Make sure gravity is enabled
                rb.useGravity = true;
                rb.isKinematic = false;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Launched Pikmin with velocity: {launchVelocity}, magnitude: {launchVelocity.magnitude}");
                }
            }
            else
            {
                Debug.LogError("Spawned Pikmin has no Rigidbody!");
            }
            
            // Register with PikminManager if available
            Pikmin pikminComponent = pikmin.GetComponent<Pikmin>();
            if (pikminComponent != null && PikminManager.Instance != null)
            {
                // The manager will handle registration when the Pikmin lands and is ready
                // Or you can register immediately if you prefer
                // PikminManager.Instance.RegisterPikmin(pikminComponent);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Current Pikmin count: {PikminManager.Instance.GetPikminCount()}/{PikminManager.Instance.GetMaxPikmin()}");
                }
            }
        }
        
        StopAiming();
    }
    
    void OnDrawGizmos()
    {
        // Debug visualization
        if (Application.isPlaying && isAiming && launchPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(launchPoint.position, 0.3f);
            
            if (canLaunch)
            {
                Gizmos.color = IsAtPikminLimit() ? limitReachedTrajectoryColor : Color.green;
                Gizmos.DrawWireSphere(targetPoint, 0.5f);
                
                // Draw launch direction
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(launchPoint.position, launchVelocity.normalized * 2f);
            }
        }
        
        // Always show launch point when selected
        if (launchPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(launchPoint.position, 0.2f);
        }
    }
    
    void HideLimitUI()
    {
        if (limitReachedUI != null)
        {
            limitReachedUI.SetActive(false);
        }
    }
    
    System.Collections.IEnumerator FlashTrajectory()
    {
        if (trajectoryLine != null)
        {
            Color originalColor = trajectoryLine.startColor;
            
            // Flash red 3 times
            for (int i = 0; i < 3; i++)
            {
                trajectoryLine.startColor = Color.red;
                trajectoryLine.endColor = Color.red;
                yield return new WaitForSeconds(0.1f);
                
                trajectoryLine.startColor = limitReachedTrajectoryColor;
                trajectoryLine.endColor = limitReachedTrajectoryColor;
                yield return new WaitForSeconds(0.1f);
            }
            
            trajectoryLine.startColor = originalColor;
            trajectoryLine.endColor = originalColor;
        }
    }
}
