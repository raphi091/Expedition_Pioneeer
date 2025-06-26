using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageControl : MonoBehaviour, IInteractable
{
    [Header("StorageUI")]
    [SerializeField] private StorageUIManager storageManager;
    [SerializeField] private float promptHeight = 2.0f;


    private void Awake()
    {
        if (storageManager == null)
        {
            storageManager = StorageUIManager.Instance;
        }
    }

    public void Interact(PlayerControl player)
    {
        if (storageManager != null)
        {
            storageManager.OpenPanel();
        }
    }

    public void Highlight()
    {
    }

    public void Unhighlight()
    {
    }

    public string GetInteractionPrompt()
    {
        return "창고 사용하기";
    }

    public Vector3 GetPromptPosition()
    {
        return transform.position + Vector3.up * promptHeight;
    }
}
