using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 1f;

    void Update()
    {
        if (player == null) return;

        // Calculate direction to player
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Get distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Move towards player if beyond stopping distance
        if (distance > stoppingDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Optional: Make enemy face the player
            transform.LookAt(player);
        }
    }
}
