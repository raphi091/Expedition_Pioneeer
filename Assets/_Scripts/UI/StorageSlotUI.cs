using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class StorageSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI itemCountText;

    public PlayerItemData RepresentedItem { get; private set; }
    public bool IsInPouch { get; private set; }

    private StorageUIManager storageUIManager;

    public void Setup(PlayerItemData itemData, bool isInPouch, StorageUIManager manager)
    {
        this.RepresentedItem = itemData;
        this.IsInPouch = isInPouch;
        this.storageUIManager = manager;

        ItemInfo info = ItemDatabase.Instance.GetItemByID(itemData.itemID);
        if (info != null)
        {
            itemIcon.sprite = info.itemIcon;
            itemCountText.text = itemData.count.ToString();
            itemIcon.enabled = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RepresentedItem == null) return;

        storageUIManager.OnDragStart(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RepresentedItem == null) return;

        storageUIManager.OnDragging();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (RepresentedItem == null) return;

        storageUIManager.OnDragEnd(eventData);
    }
}
