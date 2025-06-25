using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerInteractionControl : MonoBehaviour
{
    [Header("Setting")]
    private PlayerControl pc;
    private Animator animator;
    private InventoryManager inventory;

    [Header("UI")]
    // public PouchUIManager pouchUI;
    private ItemUIManager quickSlotUI;

    [Header("Interaction")]
    private IInteractable currentInteractable;


    private void Awake()
    {
        inventory = FindObjectOfType<InventoryManager>();
        quickSlotUI = FindObjectOfType<ItemUIManager>();
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

        quickSlotUI.UseCurrentItem();
    }

    public void RequestInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(pc);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;

            string promptText = currentInteractable.GetInteractText();
            // TODO: "F키 눌러 [대화하기]" 같은 상호작용 UI 표시
            // ex: interactionPromptUI.Show(interactable.GetInteractText());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            // TODO: 상호작용 UI 숨기기
            // ex: interactionPromptUI.Hide();
        }
    }
}
