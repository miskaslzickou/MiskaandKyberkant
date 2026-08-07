using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class GunWeapon : WeaponBehaviour
{
  
    public override void Attack()
    {

        float damage = weaponItem.GetStat(StatType.Damage) + player.GetComponent<PlayerStats>().GetStat(StatType.Damage);
        float criticalChance = weaponItem.GetStat(StatType.CriticalChance) +player.GetComponent<PlayerStats>().GetStat(StatType.CriticalChance);
        float penetration = weaponItem.GetStat(StatType.Penetration)+player.GetComponent<PlayerStats>().GetStat(StatType.Penetration);
        float attackSpeed = weaponItem.GetStat(StatType.AttackSpeed) + player.GetComponent<PlayerStats>().GetStat(StatType.AttackSpeed);


    }

    public override void OnEquip()
    {
       
    }

    public override void OnUnequip()
    {
      
    }
    public override void WhileEquipped()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePosition - (Vector2)weapon.transform.position).normalized;
         
    }
    private void OnDestroy()
    {
        
    }

}
