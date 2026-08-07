using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public event Action<InventoryItem> OnActiveWeaponChanged;
    private InventoryItem _currWeapon;
    private SpriteRenderer spriteRenderer;
    private GameObject _currentWeaponInstance; // ← nový field

    public InventoryItem CurrentWeapon
    {
        get => _currWeapon;
        set
        {
            if (_currWeapon != value)
            {
                _currWeapon = value;
                OnActiveWeaponChanged?.Invoke(_currWeapon);
                EquipWeapon(); // ← volá se stále stejně
            }
        }
    }

    private void EquipWeapon()
    {
        if (_currentWeaponInstance != null)
            Destroy(_currentWeaponInstance);

        if (CurrentWeapon?.weaponPrefab != null)
        {
            _currentWeaponInstance = Instantiate(CurrentWeapon.weaponPrefab, transform);
            var behaviour = _currentWeaponInstance.GetComponent<WeaponBehaviour>();
            behaviour?.Initialize(CurrentWeapon);
            behaviour?.OnEquip();
        }

        spriteRenderer.sprite = CurrentWeapon?.icon;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}