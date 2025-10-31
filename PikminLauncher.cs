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
    
    private bool isAiming = false;
    private Vector3 targetPoint;
    private Vector3 launchVelocity;
    
    void Start()
    {
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
            LaunchPikmin();
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
        if (trajectoryLine != null)
            trajectoryLine.enabled = true;
        if (landingMarker != null)
            landingMarker.SetActive(true);
    }
    
    void UpdateAim()
    {
        // Raycast to find target point
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            targetPoint = hit.point;
            
            // Clamp distance
            Vector3 directionToTarget = targetPoint - launchPoint.position;
            float distance = directionToTarget.magnitude;
            
            if (distance > maxLaunchDistance)
            {
                targetPoint = launchPoint.position + directionToTarget.normalized * maxLaunchDistance;
            }
            
            // Calculate launch velocity
            launchVelocity = CalculateLaunchVelocity(launchPoint.position, targetPoint, arcHeight);
            
            // Draw trajectory
            DrawTrajectory();
            
            // Update landing marker
            if (landingMarker != null)
            {
                landingMarker.transform.position = targetPoint;
            }
        }
    }
    
    Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = end.y - start.y;
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
        
        float time = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;
        
        return velocityXZ + velocityY;
    }
    
    void DrawTrajectory()
    {
        if (trajectoryLine == null) return;
        
        Vector3[] points = new Vector3[trajectoryResolution];
        Vector3 startPosition = launchPoint.position;
        Vector3 startVelocity = launchVelocity;
        
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * 0.1f;
            points[i] = startPosition + startVelocity * time + 0.5f * Physics.gravity * time * time;
            
            // Stop trajectory at ground
            if (Physics.Raycast(points[i] + Vector3.up, Vector3.down, out RaycastHit hit, 2f, groundLayer))
            {
                points[i] = hit.point;
                trajectoryLine.positionCount = i + 1;
                break;
            }
        }
        
        trajectoryLine.SetPositions(points);
        
        // Color based on validity (optional)
        float distance = Vector3.Distance(launchPoint.position, targetPoint);
        trajectoryLine.startColor = distance <= maxLaunchDistance ? validTrajectoryColor : invalidTrajectoryColor;
        trajectoryLine.endColor = trajectoryLine.startColor;
    }
    
    void LaunchPikmin()
    {
        if (pikminPrefab != null && launchPoint != null)
        {
            // Spawn pikmin
            GameObject pikmin = Instantiate(pikminPrefab, launchPoint.position, Quaternion.identity);
            
            // Apply launch velocity
            Rigidbody rb = pikmin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = launchVelocity;
            }
        }
        
        // Hide trajectory
        isAiming = false;
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
        if (landingMarker != null)
            landingMarker.SetActive(false);
    }
    
    void OnDrawGizmos()
    {
        // Debug visualization
        if (isAiming && launchPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(launchPoint.position, 0.3f);
            Gizmos.DrawWireSphere(targetPoint, 0.5f);
        }
    }
}
