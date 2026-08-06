using UnityEngine;
using System.Collections.Generic;

// Tento řádek vytvoří tlačítko v Unity menu
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    public ItemCategory category;
    public Sprite icon;
    public List<StatBonus> stats = new List<StatBonus>();
    public Rarity rarity;

}