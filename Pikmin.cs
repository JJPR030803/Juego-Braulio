using UnityEngine;

public class Pikmin : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Following")]
    [SerializeField] private float followDistance = 2f; // Minimum distance to maintain from player
    [SerializeField] private float stopDistance = 1.5f; // Distance at which pikmin stops moving
    [SerializeField] private float followDelay = 0.5f; // Delay before starting to follow after landing
    [SerializeField] private bool autoFindPlayer = true; // Automatically find player by tag
    [SerializeField] private string playerTag = "Player";
    
    [Header("Physics")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer = -1;
    
    [Header("Landing")]
    [SerializeField] private float landingDeceleration = 5f;
    [SerializeField] private ParticleSystem landingEffect; // Optional
    
    [Header("Obstacle Avoidance")]
    [SerializeField] private float avoidanceRadius = 0.5f;
    [SerializeField] private float avoidanceForce = 50f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    
    private Rigidbody rb;
    private Transform playerTransform;
    private bool hasLanded = false;
    private bool isGrounded = false;
    private float landingTime = 0f;
    private bool canFollow = false;
    private Vector3 targetPosition;
    
    // Optional: For formations
    private static int pikminCount = 0;
    private int myIndex;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Pikmin requires a Rigidbody component!");
            enabled = false;
            return;
        }
        
        // Ensure proper physics settings
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Apply custom gravity scale if needed
        if (gravityScale != 1f)
        {
            rb.useGravity = false; // We'll apply custom gravity
        }
        
        // Auto find player
        if (autoFindPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning($"Pikmin: No GameObject with tag '{playerTag}' found!");
            }
        }
        
        // Assign index for formation
        myIndex = pikminCount++;
    }
    
    void OnDestroy()
    {
        pikminCount--;
    }
    
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    void Update()
    {
        // Check if grounded
        CheckGrounded();
        
        // Handle different states
        if (!hasLanded)
        {
            // Face movement direction while in air
            if (rb.velocity.magnitude > 0.1f)
            {
                Vector3 lookDirection = rb.velocity;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else if (canFollow && playerTransform != null)
        {
            // Follow player after landing
            FollowPlayer();
        }
        
        // Start following after delay
        if (hasLanded && !canFollow && Time.time - landingTime > followDelay)
        {
            canFollow = true;
        }
    }
    
    void FixedUpdate()
    {
        // Apply custom gravity if needed
        if (!rb.useGravity && gravityScale != 1f)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
        
        // Decelerate after landing (before following starts)
        if (hasLanded && !canFollow && isGrounded)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, landingDeceleration * Time.fixedDeltaTime);
        }
    }
    
    void FollowPlayer()
    {
        if (!isGrounded) return;
        
        // Calculate formation position
        Vector3 formationOffset = CalculateFormationOffset(myIndex);
        targetPosition = playerTransform.position + formationOffset;
        
        // Calculate distance to target
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0; // Keep movement horizontal
        float distanceToTarget = directionToTarget.magnitude;
        
        // Move if outside stop distance
        if (distanceToTarget > stopDistance)
        {
            // Normalize direction
            Vector3 moveDirection = directionToTarget.normalized;
            
            // Add obstacle avoidance
            moveDirection += GetAvoidanceVector();
            moveDirection.Normalize();
            
            // Calculate desired velocity
            float currentSpeed = Mathf.Lerp(0, moveSpeed, (distanceToTarget - stopDistance) / (followDistance - stopDistance));
            Vector3 desiredVelocity = moveDirection * currentSpeed;
            
            // Apply movement
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(desiredVelocity.x, rb.velocity.y, desiredVelocity.z), acceleration * Time.deltaTime);
            
            // Rotate to face movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Stop horizontal movement when close enough
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    
    Vector3 CalculateFormationOffset(int index)
    {
        // Create a circular formation around the player
        float angle = (index * 45f) * Mathf.Deg2Rad; // 45 degrees between each pikmin
        float radius = followDistance + (index / 8) * 0.5f; // Expand radius for every 8 pikmin
        
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetAvoidanceVector()
    {
        Vector3 avoidance = Vector3.zero;
        
        // Check for nearby obstacles
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, avoidanceRadius, obstacleLayer);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.transform != transform && col.transform != playerTransform)
            {
                // Calculate avoidance direction
                Vector3 awayFromObstacle = transform.position - col.ClosestPoint(transform.position);
                awayFromObstacle.y = 0;
                
                // Stronger avoidance for closer obstacles
                float distance = awayFromObstacle.magnitude;
                if (distance > 0)
                {
                    avoidance += (awayFromObstacle.normalized / distance) * avoidanceForce;
                }
            }
        }
        
        return avoidance;
    }
    
    void CheckGrounded()
    {
        // Raycast down to check if grounded
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we've hit the ground
        if (!hasLanded)
        {
            // Check if we hit something below us (ground)
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    hasLanded = true;
                    landingTime = Time.time;
                    
                    // Play landing effect if available
                    if (landingEffect != null)
                    {
                        landingEffect.Play();
                    }
                    
                    // Optional: Add a small bounce
                    rb.velocity = new Vector3(rb.velocity.x * 0.5f, 2f, rb.velocity.z * 0.5f);
                    
                    Debug.Log($"Pikmin landed at {transform.position}");
                    break;
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * (groundCheckDistance + 0.1f));
        
        // Draw follow radius
        if (playerTransform != null && canFollow)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
        
        // Draw avoidance radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}
