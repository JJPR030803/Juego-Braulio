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
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
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
            return;
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("PikminManager: No player found with 'Player' tag!");
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
        
        // Change formations with number keys (optional)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeFormation(FormationType.Circle);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeFormation(FormationType.Square);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ChangeFormation(FormationType.Triangle);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            ChangeFormation(FormationType.Line);
        
        // Update formations continuously
        UpdateFormations();
        
        // Clean up null entries periodically
        CleanupNullPikmin();
    }
    
    public bool RegisterPikmin(Pikmin pikmin)
    {
        if (pikmin == null) return false;
        
        // Check if already registered
        if (activePikmin.Contains(pikmin))
        {
            if (showDebugInfo)
                Debug.Log("Pikmin already registered!");
            return true;
        }
        
        // Check limit
        if (activePikmin.Count >= maxPikmin)
        {
            if (showDebugInfo)
                Debug.LogWarning($"Cannot register Pikmin - limit reached ({maxPikmin})!");
            return false;
        }
        
        // Register the pikmin
        activePikmin.Add(pikmin);
        pikmin.SetPlayer(playerTransform);
        
        // Assign formation position
        int index = activePikmin.Count - 1;
        pikmin.SetFormationIndex(index);
        UpdatePikminFormationPosition(pikmin, index);
        
        if (showDebugInfo)
            Debug.Log($"Registered Pikmin. Total: {activePikmin.Count}");
        
        return true;
    }
    
    public void UnregisterPikmin(Pikmin pikmin)
    {
        if (activePikmin.Contains(pikmin))
        {
            activePikmin.Remove(pikmin);
            
            // Don't set player to null here - let the pikmin decide
            
            if (showDebugInfo)
                Debug.Log($"Unregistered Pikmin. Total: {activePikmin.Count}");
            
            // Reorganize formation
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
        
        if (showDebugInfo)
            Debug.Log("All Pikmin dismissed!");
    }
    
    void WhistleForPikmin()
    {
        // Show whistle effect
        if (whistleEffectPrefab != null)
        {
            GameObject effect = Instantiate(whistleEffectPrefab, playerTransform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Find all Pikmin in radius
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, whistleRadius);
        int whistledCount = 0;
        
        foreach (Collider col in colliders)
        {
            Pikmin pikmin = col.GetComponent<Pikmin>();
            if (pikmin != null && pikmin.IsFollowing() && !pikmin.IsRegisteredWithManager())
            {
                if (RegisterPikmin(pikmin))
                {
                    whistledCount++;
                }
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"Whistled! Added {whistledCount} new Pikmin. Total: {activePikmin.Count}");
    }
    
    void UpdateFormations()
    {
        for (int i = 0; i < activePikmin.Count; i++)
        {
            if (activePikmin[i] != null)
            {
                UpdatePikminFormationPosition(activePikmin[i], i);
            }
        }
    }
    
    void UpdatePikminFormationPosition(Pikmin pikmin, int index)
    {
        Vector3 offset = GetFormationOffset(index);
        pikmin.SetFormationOffset(offset);
        pikmin.SetFormationIndex(index);
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
        int pikminsInRing = 8;
        int ring = index / pikminsInRing;
        int posInRing = index % pikminsInRing;
        
        float angle = (posInRing * 360f / pikminsInRing) * Mathf.Deg2Rad;
        float radius = formationSpacing * (1 + ring);
        
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetSquareFormation(int index)
    {
        int row = index / pikminsPerRow;
        int col = index % pikminsPerRow;
        
        float x = (col - pikminsPerRow / 2f) * formationSpacing;
        float z = -row * formationSpacing - formationSpacing * 2;
        
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
        float z = -row * formationSpacing - formationSpacing * 2;
        
        return new Vector3(x, 0, z);
    }
    
    Vector3 GetLineFormation(int index)
    {
        int row = index / pikminsPerRow;
        int col = index % pikminsPerRow;
        
        float x = (col - pikminsPerRow / 2f) * formationSpacing * 0.5f;
        float z = -index * formationSpacing * 0.3f - formationSpacing * 2;
        
        return new Vector3(x, 0, z);
    }
    
    void ReorganizeFormation()
    {
        // Remove null entries
        activePikmin.RemoveAll(p => p == null);
        
        // Reassign positions
        for (int i = 0; i < activePikmin.Count; i++)
        {
            UpdatePikminFormationPosition(activePikmin[i], i);
        }
    }
    
    void CleanupNullPikmin()
    {
        // Remove destroyed pikmin every few seconds
        if (Time.frameCount % 120 == 0) // Every ~2 seconds at 60fps
        {
            int removed = activePikmin.RemoveAll(p => p == null);
            if (removed > 0)
            {
                ReorganizeFormation();
                if (showDebugInfo)
                    Debug.Log($"Cleaned up {removed} null Pikmin entries");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // Draw whistle radius
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(playerTransform.position, whistleRadius);
            
            // Draw formation preview
            Gizmos.color = Color.yellow;
            for (int i = 0; i < Mathf.Min(20, maxPikmin); i++)
            {
                Vector3 offset = GetFormationOffset(i);
                Vector3 pos = playerTransform.position + offset;
                Gizmos.DrawWireSphere(pos, 0.2f);
            }
        }
    }
    
    // Public methods for other scripts
    public int GetPikminCount() => activePikmin.Count;
    public int GetMaxPikmin() => maxPikmin;
    public Transform GetPlayerTransform() => playerTransform;
    public List<Pikmin> GetActivePikmin() => new List<Pikmin>(activePikmin);
    
    public void ChangeFormation(FormationType newFormation)
    {
        formationType = newFormation;
        ReorganizeFormation();
        
        if (showDebugInfo)
            Debug.Log($"Changed formation to: {newFormation}");
    }
    
    public bool CanRegisterMorePikmin()
    {
        return activePikmin.Count < maxPikmin;
    }
}
