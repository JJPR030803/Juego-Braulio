using UnityEngine;

/// <summary>
/// Represents a pellet (pillflower) that can be carried to the Onion
/// to create new Pikmin
/// </summary>
public class Pellet : MonoBehaviour
{
    [Header("Pellet Properties")]
    [SerializeField] private int pikminValue = 1; // How many Pikmin this pellet creates
    [SerializeField] private float weight = 1f; // How many Pikmin needed to carry it
    [SerializeField] private PelletType pelletType = PelletType.Number;
    [SerializeField] private int pelletNumber = 1; // The number on the pellet (1, 5, 10, 20)

    [Header("Carry Settings")]
    [SerializeField] private bool canBeCarried = true;
    [SerializeField] private bool isBeingCarried = false;
    [SerializeField] private bool readyForAbsorption = false;

    [Header("Visual Settings")]
    [SerializeField] private Color pelletColor = Color.red;
    [SerializeField] private Renderer pelletRenderer;
    [SerializeField] private TextMesh numberText; // Optional text showing the number

    [Header("Physics")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private LayerMask onionLayer;

    private Rigidbody rb;
    private PikminOnion targetOnion;
    private bool hasBeenDelivered = false;

    public enum PelletType
    {
        Number,     // Standard numbered pellet (1, 5, 10, 20)
        Flower,     // Pillflower/Nectar pellet
        Enemy       // Enemy corpse that acts like a pellet
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Setup physics
        if (rb != null)
        {
            rb.useGravity = useGravity;
        }

        // Auto-find renderer if not assigned
        if (pelletRenderer == null)
        {
            pelletRenderer = GetComponent<Renderer>();
        }

        // Apply color
        if (pelletRenderer != null && pelletRenderer.material != null)
        {
            pelletRenderer.material.color = pelletColor;
        }

        // Setup number text
        if (numberText != null)
        {
            numberText.text = pelletNumber.ToString();
        }

        // Calculate Pikmin value based on type and number
        CalculatePikminValue();
    }

    void Update()
    {
        // If marked ready for absorption and near an onion, notify it
        if (readyForAbsorption && targetOnion != null && !hasBeenDelivered)
        {
            targetOnion.ReceivePellet(this);
            hasBeenDelivered = true;
        }
    }

    /// <summary>
    /// Calculate how many Pikmin this pellet is worth
    /// </summary>
    void CalculatePikminValue()
    {
        switch (pelletType)
        {
            case PelletType.Number:
                // Standard pellets: value equals the number
                pikminValue = pelletNumber;
                break;

            case PelletType.Flower:
                // Flowers typically give 1 Pikmin
                pikminValue = 1;
                break;

            case PelletType.Enemy:
                // Enemies value can vary based on size/weight
                pikminValue = Mathf.Max(1, Mathf.RoundToInt(weight));
                break;
        }
    }

    /// <summary>
    /// Called when Pikmin start carrying this pellet
    /// </summary>
    public void StartCarrying()
    {
        if (!canBeCarried)
        {
            Debug.LogWarning($"[Pellet] {gameObject.name} cannot be carried!");
            return;
        }

        isBeingCarried = true;
        Debug.Log($"[Pellet] {gameObject.name} is being carried");
    }

    /// <summary>
    /// Called when Pikmin stop carrying this pellet
    /// </summary>
    public void StopCarrying()
    {
        isBeingCarried = false;
        Debug.Log($"[Pellet] {gameObject.name} is no longer being carried");
    }

    /// <summary>
    /// Mark this pellet as ready to be absorbed by an onion
    /// </summary>
    public void MarkReadyForAbsorption(PikminOnion onion)
    {
        readyForAbsorption = true;
        targetOnion = onion;
        Debug.Log($"[Pellet] {gameObject.name} ready for absorption by onion");
    }

    /// <summary>
    /// Check if this pellet is ready to be absorbed
    /// </summary>
    public bool IsReadyForAbsorption()
    {
        return readyForAbsorption && !hasBeenDelivered;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we've entered an onion's radius
        PikminOnion onion = other.GetComponent<PikminOnion>();
        if (onion != null && isBeingCarried)
        {
            MarkReadyForAbsorption(onion);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Alternative: Check collision with onion
        PikminOnion onion = collision.gameObject.GetComponent<PikminOnion>();
        if (onion != null && isBeingCarried)
        {
            MarkReadyForAbsorption(onion);
        }
    }

    // Public getters
    public int GetPikminValue() => pikminValue;
    public float GetWeight() => weight;
    public bool CanBeCarried() => canBeCarried;
    public bool IsBeingCarried() => isBeingCarried;
    public PelletType GetPelletType() => pelletType;

    // Public setters for runtime configuration
    public void SetPikminValue(int value)
    {
        pikminValue = value;
    }

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
    }

    public void SetPelletNumber(int number)
    {
        pelletNumber = number;
        if (numberText != null)
        {
            numberText.text = number.ToString();
        }
        CalculatePikminValue();
    }

    public void SetPelletColor(Color color)
    {
        pelletColor = color;
        if (pelletRenderer != null && pelletRenderer.material != null)
        {
            pelletRenderer.material.color = color;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw carry weight indicator
        Gizmos.color = isBeingCarried ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw arrow pointing to target onion if ready for absorption
        if (readyForAbsorption && targetOnion != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetOnion.transform.position);
        }
    }
}
