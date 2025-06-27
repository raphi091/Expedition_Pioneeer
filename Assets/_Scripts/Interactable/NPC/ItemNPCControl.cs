using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNPCControl : MonoBehaviour, IInteractable
{
    private DialogueUIManager dialogueUIManager;
    private Animator animator;

    [Header("Settings")]
    [SerializeField] private float dialogueDuration = 3.0f;
    [SerializeField] private float promptHeight = 2.0f;


    private void Start()
    {
        if (!TryGetComponent(out animator))
            Debug.LogWarning("ItemNPC ] Animator 없음");

        dialogueUIManager = FindObjectOfType<DialogueUIManager>();
    }

    public void Interact(PlayerControl player)
    {
        StartCoroutine(InteractionSequence());
    }

    private IEnumerator InteractionSequence()
    {
        animator.SetTrigger("Talking");
        dialogueUIManager.ShowMassage("아이템 상점은 준비 중입니다.", dialogueDuration);

        yield return new WaitForSecondsRealtime(dialogueDuration);

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
