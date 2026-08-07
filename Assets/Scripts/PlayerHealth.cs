using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats playerStats;
    private float currentHealth { get; set; }
    private float maxHealth { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = playerStats.GetStat(StatType.Health); // Initialize with full health
        playerStats.OnStatsChanged += UpdateStatsFromManager;
    }
    void UpdateStatsFromManager()
    {
        maxHealth= playerStats.GetStat(StatType.Health);
      
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
    }
    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateStatsFromManager;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
