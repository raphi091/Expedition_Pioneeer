using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[RequireComponent(typeof(Animator))]
public class PlayerInteractionControl : MonoBehaviour
{
    public static PlayerInteractionControl Instance = null;

    [Header("Setting")]
    private PlayerControl pc;
    private Animator animator;
    private InventoryManager inventory;

    [Header("UI")]
    // public PouchUIManager pouchUI;
    private ItemUIManager quickSlotUI;

    [Header("Interaction UI")]
    public GameObject worldSpacePromptPrefab;
    private GameObject promptInstance;
    private TextMeshProUGUI promptText;

    [Header("Interaction")]
    private IInteractable currentInteractable;
    private Transform mainCameraTransform;

    private bool isInteracting = false;
    private bool isUseItem = false;

    public bool IsUserItem => isUseItem;
    public bool IsInteracting => isInteracting;


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

        inventory = FindObjectOfType<InventoryManager>();
        quickSlotUI = FindObjectOfType<ItemUIManager>();
    }

    private void Start()
    {
        mainCameraTransform = Camera.main.transform;

        if (worldSpacePromptPrefab != null)
        {
            promptInstance = Instantiate(worldSpacePromptPrefab);
            promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
            promptInstance.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (promptInstance != null && promptInstance.activeSelf)
        {
            promptInstance.transform.rotation = mainCameraTransform.rotation;
        }
    }

    public void Initialize(PlayerControl playerControl)
    {
        pc = playerControl;
        animator = playerControl.animator;
    }

    public void TogglePouchUI()
    {
        // if (pouchUI == null) return;

        // pouchUI.TogglePanel();
        // TODO: UI가 열리면 게임 시간을 멈추거나 플레이어 입력을 막는 로직 추가
    }

    public void RequestUseQuickSlotItem()
    {
        if (quickSlotUI == null) return;

        isUseItem = true;
        quickSlotUI.UseCurrentItem();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isInteracting) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            currentInteractable.Highlight();
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            currentInteractable.Unhighlight();
            currentInteractable = null;
            HideInteractionPrompt();
        }
    }

    public void RequestInteraction()
    {
        if (currentInteractable != null && !isInteracting)
        {
            currentInteractable.Interact(pc);

            HideInteractionPrompt();
            isInteracting = true;
        }
    }

    public void EndInteraction()
    {
        isInteracting = false;

        if (currentInteractable != null)
        {
            ShowInteractionPrompt();
        }
    }

    private void ShowInteractionPrompt()
    {
        if (promptInstance != null)
        {
            promptText.text = currentInteractable.GetInteractionPrompt();
            promptInstance.transform.position = currentInteractable.GetPromptPosition();
            promptInstance.SetActive(true);
        }
    }

    private void HideInteractionPrompt()
    {
        if (promptInstance != null)
        {
            promptInstance.SetActive(false);
        }
    }

    public void Animation_UesItem()
    {
        isUseItem = false;
    }
}
