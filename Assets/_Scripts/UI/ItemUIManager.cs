using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SlotUI
{
    public GameObject slot;
    public Image itemIcon;
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
}

[System.Serializable]
public class PlayerItem
{
    public ItemInfo itemInfo;
    public int quantity;
}

public class ItemUIManager : MonoBehaviour
{
    [Header("Item")]
    public List<PlayerItem> playerItems = new List<PlayerItem>();

    [Header("UI")]
    [SerializeField] private SlotUI previousSlot;
    [SerializeField] private SlotUI currentSlot;
    [SerializeField] private SlotUI nextSlot;
    [SerializeField] private TextMeshProUGUI currentItemName;
    [SerializeField] private TextMeshProUGUI currentItemQuantity;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.35f;
    [SerializeField] private Vector3 currentSlotScale = Vector3.one;
    [SerializeField] private float currentSlotAlpha = 1f;
    [SerializeField] private Vector3 sideSlotScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] private float sideSlotAlpha = 0.6f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Slot Positions")]
    public Vector2 previousSlotPosition;
    public Vector2 currentSlotPosition;
    public Vector2 nextSlotPosition;

    private InputControl input;
    private ItemDatabase itemDatabase;
    private int currentItemIndex = 0;
    private bool isAnimating = false;


    private void Awake()
    {
        input = FindObjectOfType<InputControl>();
        itemDatabase = FindObjectOfType<ItemDatabase>();
    }

    private void OnEnable()
    {
        input.actionInput.Player.ItemScroll.performed += OnScroll;
        input.actionInput.Player.ItemNext.performed += OnItemNext;
        input.actionInput.Player.ItemPrevious.performed += OnItemPrevious;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated += UpdateItemListFromManager;
        }
    }

    private void OnDisable()
    {
        input.actionInput.Player.ItemScroll.performed -= OnScroll;
        input.actionInput.Player.ItemNext.performed -= OnItemNext;
        input.actionInput.Player.ItemPrevious.performed -= OnItemPrevious;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated -= UpdateItemListFromManager;
        }
    }

    void Start()
    {
        previousSlot.rectTransform = previousSlot.slot.GetComponent<RectTransform>();
        currentSlot.rectTransform = currentSlot.slot.GetComponent<RectTransform>();
        nextSlot.rectTransform = nextSlot.slot.GetComponent<RectTransform>();

        UpdateAllSlots();
        UpdateItemListFromManager();
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<Vector2>().y;

        if (scrollValue > 0f)
            SelectPreviousItem();
        else if (scrollValue < 0f)
            SelectNextItem();
    }

    private void OnItemNext(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;

        SelectNextItem();
    }

    private void OnItemPrevious(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<InGameUIManager>().IsPause) return;

        SelectPreviousItem();
    }

    public void UseCurrentItem()
    {
        if (isAnimating || playerItems.Count == 0) return;

        PlayerItem currentItem = playerItems[currentItemIndex];
        if (currentItem.quantity > 0)
        {
            InventoryManager.Instance.UseItem(currentItem.itemInfo.itemID);
        }
        else
        {
            // StartCoroutine(ShakeSlotCoroutine(currentSlot));
        }
    }

    private void SelectNextItem()
    {
        if (isAnimating || playerItems.Count < 2) return;
        StartCoroutine(AnimateCarousel(-1));
    }

    private void SelectPreviousItem()
    {
        if (isAnimating || playerItems.Count < 2) return;
        StartCoroutine(AnimateCarousel(1));
    }

    private void UpdateItemListFromManager()
    {
        if (InventoryManager.Instance == null || itemDatabase == null) return;

        playerItems.Clear();

        List<ItemInfo> allConsumables = itemDatabase.GetAllConsumableInfos();

        foreach (ItemInfo info in allConsumables)
        {
            int quantityInPouch = InventoryManager.Instance.GetPouchItemCount(info.itemID);

            playerItems.Add(new PlayerItem { itemInfo = info, quantity = quantityInPouch });
        }

        // 정렬 (선택 사항: 원한다면 특정 순서대로 정렬)
        // playerItems = playerItems.OrderBy(item => item.itemInfo.itemID).ToList();
        if (currentItemIndex >= playerItems.Count)
        {
            currentItemIndex = playerItems.Count > 0 ? playerItems.Count - 1 : 0;
        }

        UpdateAllSlots();
    }

    private void UpdateAllSlots()
    {
        bool hasItems = playerItems.Count > 0;

        currentSlot.slot.SetActive(hasItems);
        previousSlot.slot.SetActive(hasItems && playerItems.Count > 2);
        nextSlot.slot.SetActive(hasItems && playerItems.Count > 1);
        currentItemName.gameObject.SetActive(hasItems);
        currentItemQuantity.gameObject.SetActive(hasItems);

        if (playerItems.Count == 0) return;

        int prevIndex = (currentItemIndex - 1 + playerItems.Count) % playerItems.Count;
        int nextIndex = (currentItemIndex + 1) % playerItems.Count;

        SetSlotData(previousSlot, playerItems[prevIndex], previousSlotPosition, sideSlotScale, sideSlotAlpha);
        SetSlotData(currentSlot, playerItems[currentItemIndex], currentSlotPosition, currentSlotScale, currentSlotAlpha);
        SetSlotData(nextSlot, playerItems[nextIndex], nextSlotPosition, sideSlotScale, sideSlotAlpha);

        currentItemName.text = playerItems[currentItemIndex].itemInfo.itemName;
        currentItemQuantity.text = $"{playerItems[currentItemIndex].quantity}";
    }

    private void SetSlotData(SlotUI slot, PlayerItem data, Vector2 pos, Vector3 scale, float alpha)
    {
        if (slot.slot.activeSelf)
        {
            slot.rectTransform.anchoredPosition = pos;
            slot.rectTransform.localScale = scale;
            slot.canvasGroup.alpha = alpha;
            slot.itemIcon.sprite = data.itemInfo.itemIcon;

            bool isUsable = data.quantity > 0;
        }
    }

    private IEnumerator AnimateCarousel(int direction)
    {
        isAnimating = true;

        Vector2 startPos_Prev = previousSlot.rectTransform.anchoredPosition;
        Vector3 startScale_Prev = previousSlot.rectTransform.localScale;
        float startAlpha_Prev = previousSlot.canvasGroup.alpha;

        Vector2 startPos_Curr = currentSlot.rectTransform.anchoredPosition;
        Vector3 startScale_Curr = currentSlot.rectTransform.localScale;
        float startAlpha_Curr = currentSlot.canvasGroup.alpha;

        Vector2 startPos_Next = nextSlot.rectTransform.anchoredPosition;
        Vector3 startScale_Next = nextSlot.rectTransform.localScale;
        float startAlpha_Next = nextSlot.canvasGroup.alpha;

        Vector2 targetPos_Prev, targetPos_Curr, targetPos_Next;
        Vector3 targetScale_Prev, targetScale_Curr, targetScale_Next;
        float targetAlpha_Prev, targetAlpha_Curr, targetAlpha_Next;

        if (direction == -1)
        {
            targetPos_Curr = previousSlotPosition;
            targetScale_Curr = sideSlotScale;
            targetAlpha_Curr = sideSlotAlpha;

            targetPos_Next = currentSlotPosition;
            targetScale_Next = currentSlotScale;
            targetAlpha_Next = currentSlotAlpha;
            nextSlot.rectTransform.SetAsLastSibling();

            targetPos_Prev = nextSlotPosition;
            targetScale_Prev = sideSlotScale;
            targetAlpha_Prev = sideSlotAlpha;
        }
        else
        {
            targetPos_Curr = nextSlotPosition;
            targetScale_Curr = sideSlotScale;
            targetAlpha_Curr = sideSlotAlpha;

            targetPos_Prev = currentSlotPosition;
            targetScale_Prev = currentSlotScale;
            targetAlpha_Prev = currentSlotAlpha;
            previousSlot.rectTransform.SetAsLastSibling();

            targetPos_Next = previousSlotPosition;
            targetScale_Next = sideSlotScale;
            targetAlpha_Next = sideSlotAlpha;
        }

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / animationDuration));

            previousSlot.rectTransform.anchoredPosition = Vector2.Lerp(startPos_Prev, targetPos_Prev, progress);
            previousSlot.rectTransform.localScale = Vector3.Lerp(startScale_Prev, targetScale_Prev, progress);
            previousSlot.canvasGroup.alpha = Mathf.Lerp(startAlpha_Prev, targetAlpha_Prev, progress);

            currentSlot.rectTransform.anchoredPosition = Vector2.Lerp(startPos_Curr, targetPos_Curr, progress);
            currentSlot.rectTransform.localScale = Vector3.Lerp(startScale_Curr, targetScale_Curr, progress);
            currentSlot.canvasGroup.alpha = Mathf.Lerp(startAlpha_Curr, targetAlpha_Curr, progress);

            nextSlot.rectTransform.anchoredPosition = Vector2.Lerp(startPos_Next, targetPos_Next, progress);
            nextSlot.rectTransform.localScale = Vector3.Lerp(startScale_Next, targetScale_Next, progress);
            nextSlot.canvasGroup.alpha = Mathf.Lerp(startAlpha_Next, targetAlpha_Next, progress);

            yield return null;
        }

        if (direction == -1)
            currentItemIndex = (currentItemIndex + 1) % playerItems.Count;
        else
            currentItemIndex = (currentItemIndex - 1 + playerItems.Count) % playerItems.Count;

        UpdateAllSlots();

        isAnimating = false;
    }
}