using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class StorageUIManager : MonoBehaviour
{
    public static StorageUIManager Instance = null;

    [Header("New Panel References")]
    public GameObject weaponsPanel;
    public GameObject itemsPanel;

    [Header("Weapon UI")]
    public Transform weaponSlotContainer;
    public GameObject weaponSlotPrefab;

    [Header("Item UI")]
    public Transform pouchSlotContainer;
    public Transform storageSlotContainer;
    public Image draggableIcon;
    public QuantityPopupUI quantityPopup;

    public GameObject slotPrefab;

    private StorageSlotUI draggedSlot;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated += RedrawAllSlots;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated -= RedrawAllSlots;
        }
    }

    public void OpenPanel()
    {
        InGameUIManager.Instance.OnOpenStorage();
        weaponsPanel.SetActive(false);
        itemsPanel.SetActive(false);
    }

    public void OnClick_OpenWeaponsPanel()
    {
        weaponsPanel.SetActive(true);
        itemsPanel.SetActive(false);
        RedrawWeaponsPanel();
    }

    public void OnClick_OpenItemsPanel()
    {
        weaponsPanel.SetActive(false);
        itemsPanel.SetActive(true);
        RedrawAllSlots();
    }

    private void RedrawWeaponsPanel()
    {
        foreach (Transform child in weaponSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // 데이터 가져오기
        List<PlayerEquipmentData> ownedWeapons = InventoryManager.Instance.GetEquipmentStash();
        PlayerEquipmentData equippedWeapon = DataManager.Instance.gameData.currentEquippedWeapon;

        foreach (var weaponData in ownedWeapons)
        {
            GameObject slotObj = Instantiate(weaponSlotPrefab, weaponSlotContainer);
            WeaponSlotUI slotUI = slotObj.GetComponent<WeaponSlotUI>();

            bool isEquipped = (equippedWeapon != null && equippedWeapon.weaponID == weaponData.weaponID);
            slotUI.Setup(weaponData, isEquipped, this);
        }
    }

    public void OnWeaponSlotClicked(PlayerEquipmentData weaponToEquip)
    {
        InventoryManager.Instance.EquipWeaponFromStash(weaponToEquip);

        RedrawWeaponsPanel();
    }

    private void RedrawAllSlots()
    {
        if (InventoryManager.Instance == null) return;

        foreach (Transform child in pouchSlotContainer)
        {
            Destroy(child.gameObject);
        }

        List<PlayerItemData> pouchItems = InventoryManager.Instance.GetPouchItems();

        foreach (var itemData in pouchItems)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, pouchSlotContainer);
            StorageSlotUI slotUI = newSlotObj.GetComponent<StorageSlotUI>();
            slotUI.Setup(itemData, true, this);
        }

        foreach (Transform child in storageSlotContainer)
        {
            Destroy(child.gameObject);
        }
        List<PlayerItemData> stashItems = InventoryManager.Instance.GetStashItems();
        foreach (var itemData in stashItems)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, storageSlotContainer);
            StorageSlotUI slotUI = newSlotObj.GetComponent<StorageSlotUI>();
            slotUI.Setup(itemData, false, this);
        }
    }

    public void OnDragStart(StorageSlotUI sourceSlot)
    {
        draggedSlot = sourceSlot;

        draggableIcon.gameObject.SetActive(true);
        draggableIcon.sprite = sourceSlot.itemIcon.sprite;

        sourceSlot.itemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDragging()
    {
        draggableIcon.transform.position = Input.mousePosition;
    }

    public void OnDragEnd(PointerEventData eventData)
    {
        ResetDrag();
    }

    public void OnItemDropped(DropZone destinationZone)
    {
        if (draggedSlot == null) return;

        if (draggedSlot.IsInPouch == destinationZone.isPouchZone) return;

        bool moveToPouch = destinationZone.isPouchZone;
        quantityPopup.Show(draggedSlot, moveToPouch);
    }

    private void ResetDrag()
    {
        if (draggedSlot != null)
        {
            draggedSlot.itemIcon.color = Color.white;
        }

        draggedSlot = null;
        draggableIcon.gameObject.SetActive(false);
    }
}
