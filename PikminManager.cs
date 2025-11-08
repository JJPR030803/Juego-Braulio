using UnityEngine;
using System.Collections.Generic;

public class PikminManager : MonoBehaviour
{
    [Header("Pikmin Management")]
    [SerializeField] private List<Pikmin> activePikmin = new List<Pikmin>();
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int maxPikmin = 100;
    
    [Header("Formation Settings")]
    [SerializeField] private float formationSpacing = 1f;
    [SerializeField] private int pikminsPerRow = 5;
    [SerializeField] private FormationType formationType = FormationType.Circle;
    
    [Header("Command Settings")]
    [SerializeField] private KeyCode dismissKey = KeyCode.X; // Dismiss all Pikmin
    [SerializeField] private KeyCode whistleKey = KeyCode.C; // Call all Pikmin
    [SerializeField] private float whistleRadius = 20f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject whistleEffectPrefab; // Optional whistle effect
    
    public enum FormationType
    {
        Circle,
        Square,
        Triangle,
        Line
    }
    
    private static PikminManager instance;
    public static PikminManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PikminManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
    
    void Update()
    {
        // Handle commands
        if (Input.GetKeyDown(dismissKey))
        {
            DismissAllPikmin();
        }
        
        if (Input.GetKeyDown(whistleKey))
        {
            WhistleForPikmin();
        }
        
        // Update formations
        UpdateFormations();
    }
    
    public void RegisterPikmin(Pikmin pikmin)
    {
        if (!activePikmin.Contains(pikmin) && activePikmin.Count < maxPikmin)
        {
            activePikmin.Add(pikmin);
            pikmin.SetPlayer(playerTransform);
            AssignFormationPosition(pikmin, activePikmin.Count - 1);
        }
    }
    
    public void UnregisterPikmin(Pikmin pikmin)
    {
        if (activePikmin.Contains(pikmin))
        {
            activePikmin.Remove(pikmin);
            ReorganizeFormation();
        }
    }
    
    void DismissAllPikmin()
    {
        foreach (Pikmin pikmin in activePikmin)
        {
            if (pikmin != null)
            {
                pikmin.SetPlayer(null);
            }
        }
        activePikmin.Clear();
        Debug.Log("All Pikmin dismissed!");
    }
    
    void WhistleForPikmin()
    {
        // Find all Pikmin in radius
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, whistleRadius);
        
        foreach (Collider col in colliders)
        {
            Pikmin pikmin = col.GetComponent<Pikmin>();
            if (pikmin != null && !activePikmin.Contains(pikmin))
            {
                RegisterPikmin(pikmin);
            }
        }
        
        // Show whistle effect
        if (whistleEffectPrefab != null)
        {
            GameObject effect = Instantiate(whistleEffectPrefab, playerTransform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"Whistled! {activePikmin.Count} Pikmin following.");
    }
    
    void UpdateFormations()
    {
        for (int i = 0; i < activePikmin.Count; i++)
        {
            if (activePikmin[i] != null)
            {
                AssignFormationPosition(activePikmin[i], i);
            }
        }
    }
    
    void AssignFormationPosition(Pikmin pikmin, int index)
    {
        Vector3 offset = GetFormationOffset(index);
        // The Pikmin script will handle the actual movement to this position
    }
    
    Vector3 GetFormationOffset(int index)
    {
        switch (formationType)
        {
            case FormationType.Circle:
                return GetCircleFormation(index);
            case FormationType.Square:
                return GetSquareFormation(index);
            case FormationType.Triangle:
                return GetTriangleFormation(index);
            case FormationType.Line:
                return GetLineFormation(index);
            default:
                return GetCircleFormation(index);
        }
    }
    
    Vector3 GetCircleFormation(int index)
    {
        float angle = (index * 360f / Mathf.Max(8, activePikmin.Count)) * Mathf.Deg2Rad;
        float radius = formationSpacing + (index / 8) * formationSpacing * 0.5f;
        
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetSquareFormation(int index)
    {
        int row = index / pikminsPerRow;
        int col = index % pikminsPerRow;
        
        float x = (col - pikminsPerRow / 2f) * formationSpacing;
        float z = -row * formationSpacing - formationSpacing;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetTriangleFormation(int index)
    {
        int row = 0;
        int posInRow = index;
        
        // Find which row this pikmin is in
        int pikminsInRow = 1;
        int totalPikmin = 0;
        while (totalPikmin + pikminsInRow <= index)
        {
            totalPikmin += pikminsInRow;
            pikminsInRow++;
            row++;
        }
        posInRow = index - totalPikmin;
        
        float x = (posInRow - row * 0.5f) * formationSpacing;
        float z = -row * formationSpacing - formationSpacing;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetLineFormation(int index)
    {
        int row = index / pikminsPerRow;
        int col = index % pikminsPerRow;
        
        float x = (col - pikminsPerRow / 2f) * formationSpacing * 0.5f;
        float z = -index * formationSpacing * 0.3f - formationSpacing;
        
        return new Vector3(x, 0, z);
    }
    
    void ReorganizeFormation()
    {
        // Remove null entries
        activePikmin.RemoveAll(p => p == null);
        
        // Reassign positions
        UpdateFormations();
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // Draw whistle radius
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(playerTransform.position, whistleRadius);
        }
    }
    
    // Public methods for other scripts
    public int GetPikminCount() => activePikmin.Count;
    public List<Pikmin> GetActivePikmin() => new List<Pikmin>(activePikmin);
    public void ChangeFormation(FormationType newFormation) => formationType = newFormation;
}
