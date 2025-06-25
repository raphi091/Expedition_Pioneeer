using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class WeaponSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponInfo;
    public GameObject equippedMarker;

    private PlayerEquipmentData representedWeapon;
    private StorageUIManager storageUIManager;
    private Button myButton;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnSlotClicked);
    }

    public void Setup(PlayerEquipmentData weaponData, bool isEquipped, StorageUIManager manager)
    {
        this.representedWeapon = weaponData;
        this.storageUIManager = manager;

        WeaponInfo info = ItemDatabase.Instance.GetWeaponByID(weaponData.weaponID);
        if (info != null)
        {
            weaponIcon.sprite = info.weaponIcon;
            weaponNameText.text = info.weaponName;
            weaponInfo.text = info.weaponDescription;
        }

        equippedMarker.SetActive(isEquipped);
    }

    private void OnSlotClicked()
    {
        storageUIManager.OnWeaponSlotClicked(representedWeapon);
    }
}
