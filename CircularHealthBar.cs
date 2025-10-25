using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image staminaFillImage; // Optional for stamina ring
    [SerializeField] private TextMeshProUGUI healthText; // Optional
    
    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStamina playerStamina; // Optional
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthColor = new Color(1f, 0.2f, 0.2f); // Red
    [SerializeField] private Color staminaColor = new Color(0.2f, 1f, 0.2f); // Green
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    
    private float targetHealthFill;
    private float targetStaminaFill;

    void Start()
    {
        // Set colors
        if (healthFillImage != null)
            healthFillImage.color = healthColor;
            
        if (staminaFillImage != null)
            staminaFillImage.color = staminaColor;
        
        // Subscribe to events
        if (playerHealth != null)
            playerHealth.OnHealthChanged.AddListener(UpdateHealth);
            
        if (playerStamina != null)
            playerStamina.OnStaminaChanged.AddListener(UpdateStamina);
    }

    void Update()
    {
        // Smooth transition
        if (smoothTransition)
        {
            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = Mathf.Lerp(
                    healthFillImage.fillAmount, 
                    targetHealthFill, 
                    Time.deltaTime * transitionSpeed
                );
            }
            
            if (staminaFillImage != null)
            {
                staminaFillImage.fillAmount = Mathf.Lerp(
                    staminaFillImage.fillAmount, 
                    targetStaminaFill, 
                    Time.deltaTime * transitionSpeed
                );
            }
        }
    }

    public void UpdateHealth(float healthPercentage)
    {
        targetHealthFill = healthPercentage;
        
        if (!smoothTransition && healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercentage;
        }
        
        // Update text
        if (healthText != null && playerHealth != null)
        {
            healthText.text = Mathf.Ceil(playerHealth.GetCurrentHealth()).ToString();
        }
    }

    public void UpdateStamina(float staminaPercentage)
    {
        targetStaminaFill = staminaPercentage;
        
        if (!smoothTransition && staminaFillImage != null)
        {
            staminaFillImage.fillAmount = staminaPercentage;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealth);
            
        if (playerStamina != null)
            playerStamina.OnStaminaChanged.RemoveListener(UpdateStamina);
    }
}
