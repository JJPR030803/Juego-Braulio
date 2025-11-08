using UnityEngine;

public class Pikmin : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Physics")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer = -1;
    
    [Header("Landing")]
    [SerializeField] private float landingDeceleration = 5f;
    [SerializeField] private ParticleSystem landingEffect; // Optional
    
    private Rigidbody rb;
    private bool hasLanded = false;
    private bool isGrounded = false;
    private float landingTime = 0f;
    
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
    }
    
    void Update()
    {
        // Check if grounded
        CheckGrounded();
        
        // Face movement direction while in air
        if (!hasLanded && rb.velocity.magnitude > 0.1f)
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
    
    void FixedUpdate()
    {
        // Apply custom gravity if needed
        if (!rb.useGravity && gravityScale != 1f)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
        
        // Decelerate after landing
        if (hasLanded && isGrounded)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, landingDeceleration * Time.fixedDeltaTime);
            
            // Stop completely after a short time
            if (Time.time - landingTime > 0.5f && rb.velocity.magnitude < 0.1f)
            {
                rb.velocity = Vector3.zero;
            }
        }
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
    }
}
