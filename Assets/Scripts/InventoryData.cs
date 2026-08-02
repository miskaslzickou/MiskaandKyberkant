using System.Collections.Generic;
using UnityEngine;

// Zde musí být i definice enumů, pokud jsi je smazal z InventoryUI
public enum ItemCategory { Hat, Chest, Boots, Ring, Weapon, Item }
public enum SlotType { Hat, Chest, Boots, Ring, ItemSlot }
public enum StatType { Health, Damage, Speed, LifeSteal, AttackSpeed, Armor, Penetration, CriticalChance }

[System.Serializable]
public class StatBonus
{
    public StatType statType;
    public float value;
}
[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string description;
    public ItemCategory category;
    public Sprite icon;

    // Proměnná držící statistiky
    public List<StatBonus> stats=new List<StatBonus>();
    public InventoryItem(ItemData sourceData)
    {
        this.itemName = sourceData.itemName;
        this.description = sourceData.description;
        this.category = sourceData.category;
        this.icon = sourceData.icon;

        // Tady si ten item sám zkopíruje statistiky!
        this.stats = new List<StatBonus>();
        if (sourceData.stats != null)
        {
            foreach (var bonus in sourceData.stats)
            {
                this.stats.Add(new StatBonus { statType = bonus.statType, value = bonus.value });
            }
        }
    }
}

