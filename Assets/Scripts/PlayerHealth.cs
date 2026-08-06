using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats Stats;
    private float currentHealth { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = Stats.GetStat(StatType.Health); // Initialize with full health
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
