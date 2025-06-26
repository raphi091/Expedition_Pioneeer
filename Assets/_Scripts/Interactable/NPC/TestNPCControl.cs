using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNPCControl : MonoBehaviour, IInteractable
{
    private DialogueUIManager dialogueUIManager;
    private Animator animator;

    [Header("Settings")]
    [SerializeField] private float dialogueDuration = 3.0f;
    [SerializeField] private float popupDuration = 2.5f;
    [SerializeField] private float promptHeight = 2.0f;


    private void Start()
    {
        if (!TryGetComponent(out animator))
            Debug.LogWarning("TestNPC ] Animator 없음");

        dialogueUIManager = FindObjectOfType<DialogueUIManager>();
    }

    public void Interact(PlayerControl player)
    {
        StartCoroutine(InteractionSequence());
    }

    private IEnumerator InteractionSequence()
    {
        if (DataManager.Instance.gameData.hasReceivedItemsFromTestNPC)
        {
            animator.SetTrigger("Talking");
            dialogueUIManager.ShowMassage("자네는 이미 모든 것을 받았네.", dialogueDuration);

            yield return new WaitForSecondsRealtime(dialogueDuration);
        }
        else
        {
            animator.SetTrigger("Talking");
            dialogueUIManager.ShowMassage("필요한 모든 것을 지급해주지...", dialogueDuration);

            yield return new WaitForSecondsRealtime(dialogueDuration);

            InventoryManager.Instance.AddAllTestItemsAndWeapons();

            DataManager.Instance.gameData.hasReceivedItemsFromTestNPC = true;
            InventoryManager.Instance.SaveInventoryToDataManager();
            DataManager.Instance.SaveGame();

            dialogueUIManager.ShowPopup("모든 아이템과 장비가 지급되었습니다.", popupDuration);

            yield return new WaitForSecondsRealtime(popupDuration);
        }

        PlayerInteractionControl.Instance.EndInteraction();
    }

    public void Highlight()
    {
    }

    public void Unhighlight()
    {
    }

    public string GetInteractionPrompt()
    {
        return "대화하기";
    }

    public Vector3 GetPromptPosition()
    {
        return transform.position + Vector3.up * promptHeight;
    }
}
