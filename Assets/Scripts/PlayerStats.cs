using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Propojení")]
    public InventoryUI inventoryUI;

    
    // Slovníky pro staty (žádné ruční naklikávání v Inspectoru)
    private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
    public Dictionary<StatType, float> TotalStats { get; private set; } = new Dictionary<StatType, float>();

    // Event pro UI
    public event Action OnStatsChanged;

    void Awake()
    {
        // 1. AUTOMATIZACE: Projdeme všechny staty v Enumu a nastavíme jim výchozí hodnotu 0
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            baseStats[type] = 0f;
            TotalStats[type] = 0f;
        }

        // 2. RUČNÍ ZÁKLAD: Tady si natvrdo nastavíš startovní hodnoty, které nejsou 0
        // Co tu nenapíšeš, to zůstane na nule (např. Armor, LifeSteal).
        baseStats[StatType.Health] = 100f;
        baseStats[StatType.Damage] = 10f;
        baseStats[StatType.Speed] = 5f;
    }

    void Start()
    {
        if (inventoryUI != null)
        {
            // Připojíme se na eventy inventáře
            inventoryUI.OnItemInserted += (item, slotName) => RecalculateStats();
            inventoryUI.OnItemRemoved += (item, slotName) => RecalculateStats();
        }

        // Spočítáme staty rovnou při startu
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        // 1. Resetujeme všechny staty na jejich základní hodnotu z baseStats
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            TotalStats[type] = baseStats[type];
        }

        // 2. Přičteme bonusy z vybavených itemů
        if (inventoryUI != null)
        {
            AddStatsFromSlot("slot-hat");
            AddStatsFromSlot("slot-chest");
            AddStatsFromSlot("slot-boots");
            AddStatsFromSlot("slot-ring");
            // AddStatsFromSlot("slot-weapon"); 
        }

        // 3. Pošleme signál do UI, ať se překreslí
        OnStatsChanged?.Invoke();
    }

    private void AddStatsFromSlot(string slotName)
    {
        InventoryItem item = inventoryUI.GetItemInSlot(slotName);

        // Pokud je ve slotu item a má nějaké staty, přičteme je
        if (item != null && item.stats != null)
        {
            foreach (var bonus in item.stats)
            {
                // Automaticky to přičte k odpovídajícímu statu
                TotalStats[bonus.statType] += bonus.value;
            }
        }
    }

    // Metoda pro UI skript, aby si mohl snadno vytáhnout jakékoliv číslo
    public float GetStat(StatType type)
    {
        return TotalStats[type];
    }
}