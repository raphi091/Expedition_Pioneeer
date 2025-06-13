using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot")]
    public int slotIndex;

    [Header("UI")]
    public GameObject existingDataPanel;
    public GameObject newGamePanel;

    [Header("Date Text")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI playTimeText;

    private Button selectButton;
    private SelectUIManager manager;
    private GameData gameData;


    public void Setup(SelectUIManager manager, int index, GameData data)
    {
        this.manager = manager;
        this.slotIndex = index;

        selectButton = GetComponent<Button>();
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSlotClicked);

        UpdateUI(data);
    }

    private void UpdateUI(GameData data)
    {
        if (data != null)
        {
            existingDataPanel.SetActive(true);
            newGamePanel.SetActive(false);

            characterNameText.text = data.characterInfo.characterName;
            levelText.text = "Lv " + data.characterInfo.level;
            goldText.text = data.gold.ToString("N0") + " G";

            float time = data.playTime;
            string playTimeStr = $"{(int)(time / 3600):D2}:{(int)((time % 3600) / 60):D2}:{(int)(time % 60):D2}";
            playTimeText.text = playTimeStr;
        }
        else
        {
            existingDataPanel.SetActive(false);
            newGamePanel.SetActive(true);
        }
    }

    private void OnSlotClicked()
    {
        manager.OnSlotSelected(slotIndex);
    }
}
