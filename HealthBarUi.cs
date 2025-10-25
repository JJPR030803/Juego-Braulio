using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText; // Optional
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    void Start()
    {
        if (playerHealth != null)
        {
            // Subscribe to health changes
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }
    }

    public void UpdateHealthBar(float healthPercentage)
    {
        // Update fill amount
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercentage;
            
            // Change color based on health
            healthBarFill.color = healthPercentage <= lowHealthThreshold ? 
                lowHealthColor : fullHealthColor;
        }

        // Update text (optional)
        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"{Mathf.Ceil(playerHealth.GetCurrentHealth())} / {playerHealth.GetMaxHealth()}";
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}
