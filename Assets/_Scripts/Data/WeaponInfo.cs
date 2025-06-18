using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EquipPostion
{
    Back,
    Waist
}

public enum WeaponType 
{ 
    NONE = 0,
    OnehandSword, 
    DualSword, 
    Katana,
    Spear
}

[System.Serializable]
public class CraftingMaterial
{
    public ItemInfo material;
    public int count;
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game Data/Weapon")]
public class WeaponInfo : ScriptableObject
{
    public string weaponID;
    public string weaponName;
    public WeaponType weaponType;
    public GameObject weaponPrefab;

    [Header("Animation & Location")]
    public AnimatorOverrideController animatorOverride;
    public EquipPostion equipPostion = EquipPostion.Back;

    [Header("Stats")]
    public int Damage;
    public float Sharpness;
    public float AttackRange;
    public float AttackSpeed;

    [Header("Craft")]
    public bool isCraftable = true;
    public List<CraftingMaterial> craftRecipe;
    public long craftGold;

    [Header("Upgrade")]
    public WeaponInfo requiredPreviousWeapon;
    public List<CraftingMaterial> upgradeRecipe;
    public long upgradeGold;
}
