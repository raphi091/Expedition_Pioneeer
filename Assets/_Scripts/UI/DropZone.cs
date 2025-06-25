using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class DropZone : MonoBehaviour, IDropHandler
{
    public bool isPouchZone;

    public void OnDrop(PointerEventData eventData)
    {
        StorageUIManager.Instance.OnItemDropped(this);
    }
}
