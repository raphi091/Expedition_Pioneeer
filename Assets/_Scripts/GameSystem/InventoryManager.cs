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
        itemDatabase = FindObjectOfType<ItemDatabase>();
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

    public void RegisterPlayer(PlayerControl pc)
    {
        this.playerControl = pc;
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
        ItemInfo info = itemDatabase.GetItemByID(itemID);
        if (info == null) return false;

        PlayerItemData itemToUse = runtimePouch.FirstOrDefault(item => item.itemID == itemID);
        if (itemToUse != null && itemToUse.count > 0)
        {
            itemToUse.count--;

            if (itemToUse.count <= 0) 
                runtimePouch.Remove(itemToUse);

            OnInventoryUpdated?.Invoke();

            if (playerControl != null)
            {
                playerControl.RequestUseItem(info);
            }

            return true;
        }
        return false;
    }

    public void MoveFromStashToPouch(string itemID, int amount)
    {
        ItemInfo info = itemDatabase.GetItemByID(itemID);
        if (info == null) return;

        PlayerItemData stashItem = runtimeStash.FirstOrDefault(i => i.itemID == itemID);
        if (stashItem == null || stashItem.count < amount) return;

        PlayerItemData pouchItem = runtimePouch.FirstOrDefault(i => i.itemID == itemID);
        int spaceInPouch = info.pouchCapacity - (pouchItem?.count ?? 0);

        int amountToMove = Mathf.Min(amount, spaceInPouch);
        if (amountToMove <= 0)
        {
            return;
        }

        stashItem.count -= amountToMove;
        if (stashItem.count <= 0)
        {
            runtimeStash.Remove(stashItem);
        }

        if (pouchItem != null)
        {
            pouchItem.count += amountToMove;
        }
        else
        {
            runtimePouch.Add(new PlayerItemData { itemID = itemID, count = amountToMove });
        }

        OnInventoryUpdated?.Invoke(); // UI 갱신
    }

    public void MoveFromPouchToStash(string itemID, int amount)
    {
        PlayerItemData pouchItem = runtimePouch.FirstOrDefault(i => i.itemID == itemID);
        if (pouchItem == null || pouchItem.count < amount) return;

        int amountToMove = amount;

        pouchItem.count -= amountToMove;
        if (pouchItem.count <= 0)
        {
            runtimePouch.Remove(pouchItem);
        }

        PlayerItemData stashItem = runtimeStash.FirstOrDefault(i => i.itemID == itemID);
        if (stashItem != null)
        {
            stashItem.count += amountToMove;
        }
        else
        {
            runtimeStash.Add(new PlayerItemData { itemID = itemID, count = amountToMove });
        }

        OnInventoryUpdated?.Invoke();
    }

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