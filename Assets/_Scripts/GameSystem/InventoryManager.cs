using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance = null;

    [Header("Data & References")]
    private PlayerControl playerControl;
    private ItemDatabase itemDatabase;

    [Header("Start Weapon")]
    [SerializeField] private List<WeaponInfo> initialEquipment;

    private List<PlayerItemData> runtimePouch;
    private List<PlayerItemData> runtimeStash;
    private List<PlayerEquipmentData> runtimeEquipmentStash;

    public event System.Action OnInventoryUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerControl = FindObjectOfType<PlayerControl>();
        itemDatabase = FindObjectOfType<ItemDatabase>();

        LoadInventoryFromDataManager();
    }

    public void LoadInventoryFromDataManager()
    {
        if (DataManager.Instance?.gameData == null) return;

        runtimePouch = new List<PlayerItemData>(DataManager.Instance.gameData.pouchItems);
        runtimeStash = new List<PlayerItemData>(DataManager.Instance.gameData.stashItems);
        runtimeEquipmentStash = new List<PlayerEquipmentData>(DataManager.Instance.gameData.equipmentStash);

        OnInventoryUpdated?.Invoke();
    }

    public void SaveInventoryToDataManager()
    {
        if (DataManager.Instance?.gameData == null) return;

        DataManager.Instance.gameData.pouchItems = this.runtimePouch;
        DataManager.Instance.gameData.stashItems = this.runtimeStash;
    }

    // --- 아이템 관리 함수들 ---
    public void AddItem(string itemID, int count = 1)
    {
        ItemInfo info = itemDatabase.GetItemByID(itemID);
        if (info == null) return;

        PlayerItemData pouchItem = runtimePouch.FirstOrDefault(item => item.itemID == itemID);
        int spaceInPouch = info.pouchCapacity;

        if (pouchItem != null)
        {
            spaceInPouch -= pouchItem.count;
        }

        int amountToPouch = Mathf.Min(count, spaceInPouch);

        if (amountToPouch > 0)
        {
            if (pouchItem != null) pouchItem.count += amountToPouch;
            else runtimePouch.Add(new PlayerItemData { itemID = itemID, count = amountToPouch });
        }

        int overflowAmount = count - amountToPouch;
        if (overflowAmount > 0)
        {
            PlayerItemData stashItem = runtimeStash.FirstOrDefault(item => item.itemID == itemID);
            if (stashItem != null)
            {
                stashItem.count += overflowAmount;
            }
            else
            {
                runtimeStash.Add(new PlayerItemData { itemID = itemID, count = overflowAmount });
            }
        }
        OnInventoryUpdated?.Invoke();
    }

    public bool UseItem(string itemID)
    {
        PlayerItemData itemToUse = runtimePouch.FirstOrDefault(item => item.itemID == itemID);
        if (itemToUse != null && itemToUse.count > 0)
        {
            // 실제 아이템 사용 효과 구현
            itemToUse.count--;
            if (itemToUse.count <= 0) runtimePouch.Remove(itemToUse);
            OnInventoryUpdated?.Invoke();
            return true;
        }
        return false;
    }

    public void MoveFromStashToPouch(string itemID, int amount) { /* ... */ }
    public void MoveFromPouchToStash(string itemID, int amount) { /* ... */ }

    public int GetPouchItemCount(string itemID)
    {
        PlayerItemData item = runtimePouch.FirstOrDefault(i => i.itemID == itemID);
        return item?.count ?? 0;
    }

    public int GetStashItemCount(string itemID)
    {
        PlayerItemData item = runtimeStash.FirstOrDefault(i => i.itemID == itemID);
        return item?.count ?? 0;
    }

    // --- 장비 관련 함수 ---
    public void SetupInitialEquipment()
    {
        if (runtimeEquipmentStash.Count > 0) return;
        if (initialEquipment == null || initialEquipment.Count == 0) return;

        foreach (var weaponInfo in initialEquipment)
        {
            if (weaponInfo != null)
            {
                PlayerEquipmentData newEquipment = new PlayerEquipmentData
                {
                    weaponID = weaponInfo.weaponID
                };
                AddEquipment(newEquipment);
            }
        }
    }

    public void AddEquipment(PlayerEquipmentData newEquipment)
    {
        runtimeEquipmentStash.Add(newEquipment);
        Debug.Log($"{newEquipment.weaponID} 장비 추가됨.");
        OnInventoryUpdated?.Invoke();
    }

    public void EquipWeaponFromStash(PlayerEquipmentData equipmentToEquip)
    {
        if (playerControl == null || itemDatabase == null) return;

        WeaponInfo weaponInfo = itemDatabase.GetWeaponByID(equipmentToEquip.weaponID);

        if (weaponInfo != null)
        {
            playerControl.SetWeapon(weaponInfo);

            if (DataManager.Instance != null)
            {
                DataManager.Instance.UpdateEquippedWeapon(equipmentToEquip);
            }
        }
        else
        {
            Debug.LogError($"{equipmentToEquip.weaponID}에 해당하는 WeaponInfo를 데이터베이스에서 찾을 수 없습니다.");
        }
    }

    public List<PlayerItemData> GetPouchItems() => runtimePouch;
    public List<PlayerItemData> GetStashItems() => runtimeStash;
    public List<PlayerEquipmentData> GetEquipmentStash() => runtimeEquipmentStash;
}