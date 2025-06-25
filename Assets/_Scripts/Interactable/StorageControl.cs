using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageControl : MonoBehaviour
{
    private StorageUIManager storageManager;

    private void Start()
    {
        if (storageManager == null)
        {
            storageManager = FindObjectOfType<StorageUIManager>();
        }
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
