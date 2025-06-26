using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class QuantityPopupUI : MonoBehaviour
{
    public TMP_InputField quantityInput;
    public Button okButton;
    public Button cancelButton;
    public TextMeshProUGUI itemNameText;

    private StorageSlotUI sourceSlot;
    private bool dropToPouch;

    public void Show(StorageSlotUI fromSlot, bool toPouch)
    {
        this.sourceSlot = fromSlot;
        this.dropToPouch = toPouch;

        itemNameText.text = ItemDatabase.Instance.GetItemByID(fromSlot.RepresentedItem.itemID).itemName;
        quantityInput.text = "1";

        gameObject.SetActive(true);
    }

    public void OnConfirm()
    {
        if (int.TryParse(quantityInput.text, out int amount) && amount > 0)
        {
            if (dropToPouch)
            {
                InventoryManager.Instance.MoveFromStashToPouch(sourceSlot.RepresentedItem.itemID, amount);
            }
            else
            {
                InventoryManager.Instance.MoveFromPouchToStash(sourceSlot.RepresentedItem.itemID, amount);
            }
        }
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}