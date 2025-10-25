using UnityEngine;

public class EnemyPatrolAndChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform patrolCenter; // The center point to patrol around
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolRadius = 10f; // Max distance from patrol center
    [SerializeField] private float detectionRange = 7f; // How far enemy can detect player
    
    [Header("Patrol Settings")]
    [SerializeField] private float patrolPointWaitTime = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    
    private Vector3 currentPatrolTarget;
    private float waitTimer;
    private bool isChasing;

    void Start()
    {
        // If no patrol center assigned, use current position as center
        if (patrolCenter == null)
        {
            GameObject centerObj = new GameObject("PatrolCenter");
            centerObj.transform.position = transform.position;
            patrolCenter = centerObj.transform;
        }
        
        SetNewPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToCenter = Vector3.Distance(transform.position, patrolCenter.position);
        float playerDistanceToCenter = Vector3.Distance(player.position, patrolCenter.position);

        // Check if player is within detection range AND within patrol radius
        if (distanceToPlayer <= detectionRange && playerDistanceToCenter <= patrolRadius)
        {
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            Patrol();
        }
    }

    void ChasePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 newPosition = transform.position + directionToPlayer * moveSpeed * Time.deltaTime;
        
        // Check if new position would be outside patrol radius
        float distanceFromCenter = Vector3.Distance(newPosition, patrolCenter.position);
        
        if (distanceFromCenter <= patrolRadius)
        {
            // Move towards player
            transform.position = newPosition;
            transform.LookAt(player);
        }
        else
        {
            // Move towards player but stay on the edge of radius
            Vector3 directionFromCenter = (newPosition - patrolCenter.position).normalized;
            transform.position = patrolCenter.position + directionFromCenter * patrolRadius;
        }
    }

    void Patrol()
    {
        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolTarget);
        
        if (distanceToPatrolPoint > 0.5f)
        {
            // Move towards patrol point
            Vector3 direction = (currentPatrolTarget - transform.position).normalized;
            transform.position += direction * patrolSpeed * Time.deltaTime;
            transform.LookAt(currentPatrolTarget);
        }
        else
        {
            // Wait at patrol point
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolPointWaitTime)
            {
                SetNewPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    void SetNewPatrolPoint()
    {
        // Generate random point within patrol radius
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    // Visualize patrol radius in editor
    void OnDrawGizmosSelected()
    {
        if (patrolCenter == null) return;
        
        // Draw patrol radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(patrolCenter.position, patrolRadius);
        
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw current patrol target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentPatrolTarget, 0.3f);
        }
    }
}
