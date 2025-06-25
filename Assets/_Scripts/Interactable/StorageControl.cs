using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageControl : MonoBehaviour, IInteractable
{
    [Header("StorageUI")]
    [SerializeField] private StorageUIManager storageManager;

    private void Start()
    {
    }

    public void Interact(PlayerControl player)
    {
        if (storageManager != null)
        {
            storageManager.OpenPanel();
        }
        else
        {
            return;
        }
    }

    public string GetInteractText()
    {
        return "창고 열기";
    }
}
