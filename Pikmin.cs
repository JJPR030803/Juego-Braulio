using UnityEngine;

public class Pikmin : MonoBehaviour
{
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody rb;
    private bool hasLanded = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        // Check if landed
        if (!hasLanded && rb.velocity.magnitude < 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
            {
                OnLanded();
            }
        }
    }
    
    void OnLanded()
    {
        hasLanded = true;
        Debug.Log("Pikmin landed!");
        
        // Add your landed behavior here
        // (AI, follow player, attack enemies, etc.)
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Handle collision with enemies, objects, etc.
    }
}
