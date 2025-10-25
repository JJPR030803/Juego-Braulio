using UnityEngine;

public class HealthTester : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    void Update()
    {
        // Press 'H' to take damage
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerHealth.TakeDamage(10f);
        }

        // Press 'J' to heal
        if (Input.GetKeyDown(KeyCode.J))
        {
            playerHealth.Heal(10f);
        }
    }
}
