using System.Collections.Generic;
using UnityEngine;

// Zde musí být i definice enumů, pokud jsi je smazal z InventoryUI
public enum ItemCategory { Hat, Chest, Boots, Ring, Weapon, Item }
public enum SlotType { Hat, Chest, Boots, Ring, ItemSlot,Weapon }
public enum StatType { Health, Damage, Speed, LifeSteal, AttackSpeed, Armor, Penetration, CriticalChance }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

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
    
    // Množství (stackování)
    public int quantity = 1;

    // Proměnná držící statistiky
    public List<StatBonus> stats=new List<StatBonus>();

    public Rarity rarity;

    // Unikátní ID pro každou instanci (např. kvůli duplicitám ve stejném inventáři)
    [System.NonSerialized]
    public string instanceId;

    public InventoryItem(ItemData sourceData)
    {
        this.itemName = sourceData.itemName;
        this.description = sourceData.description;
        this.category = sourceData.category;
        this.icon = sourceData.icon;
        this.quantity = 1; // Defaultní množství při vytvoření z ItemData

        // Generujeme si unikátní ID pro toto konkrétní jablko, aby se nemíchalo s dalším jablkem.
        this.instanceId = $"{this.itemName}_{System.Guid.NewGuid()}";
        this.rarity = sourceData.rarity;
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

