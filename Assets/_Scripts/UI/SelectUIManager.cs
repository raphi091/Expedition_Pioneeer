using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;


public enum CharacterType
{
    NONE = 0,
    SlayerTypeA,
    SlayerTypeB
}

public class SelectUIManager : MonoBehaviour
{
    [Header("Save Slot")]
    [SerializeField] private SaveSlotUI[] saveSlots;

    [Header("Character Profile")]
    [SerializeField] private ActorProfile SlayerTypeA_Profile;
    [SerializeField] private ActorProfile SlayerTypeB_Profile;

    [Header("Creat Character")]
    [SerializeField] private GameObject characterCreationPanel;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Loading")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("Fade In/Out")]
    [SerializeField] private Image fade;
    private float startfade = 1f;

    [Header("Panel")]
    [SerializeField] private GameObject selectSlotPanel;
    [SerializeField] private Button selectSlotFirstBtn;
    [SerializeField] private Button CreaftCharacterFirstBtn;

    [Header("Field")]
    [SerializeField] private GameObject selectField;
    [SerializeField] private GameObject creationField;

    private Stack<GameObject> uiPanelStack = new Stack<GameObject>();
    private LoadingManager loadingManager;
    private CharacterType chosenClass;
    private int selectedSlotForCreation;


    private void Awake()
    {
        selectField.SetActive(true);
        creationField.SetActive(false);

        selectSlotPanel.SetActive(true);
        characterCreationPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        LoadingPanel.SetActive(false);

        if (!TryGetComponent(out loadingManager))
            Debug.Log("SelectUIManager ] LoadingManager 없음");
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            GameData data = DataManager.Instance.GetDataForSlot(i);
            saveSlots[i].Setup(this, i, data);
        }

        SetSelectedUIElement(selectSlotFirstBtn.gameObject);

        yield return null;

        FadeOut(startfade);
    }

    public void OnEsc()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject topPanel = uiPanelStack.Peek();

            if (topPanel == characterCreationPanel)
            {
                selectField.SetActive(true);
                creationField.SetActive(false);
                StartCoroutine(CloseTopPanelAnimated());
            }
            else
            {
                CloseTopPanelInstant();
            }
        }
        else
        {
            StartCoroutine(ReturnLobby());
        }
    }

    public IEnumerator ReturnLobby()
    {
        FadeIn(startfade);

        yield return new WaitForSeconds(startfade);

        SceneManager.LoadScene("Lobby");
    }

    public void OnSlotSelected(int slotIndex)
    {
        GameData data = DataManager.Instance.GetDataForSlot(slotIndex);
        if (data != null)
        {
            StartCoroutine(StartGame(slotIndex));
        }
        else
        {
            StartCoroutine(BeginCharacterCreation(slotIndex));
        }
    }

    private IEnumerator StartGame(int slotIndex)
    {
        FadeIn(startfade);

        yield return new WaitForSeconds(startfade);

        DataManager.Instance.LoadGame(slotIndex);
        GameManager.Instance.EnableGameplayControls();
        LoadingPanel.SetActive(true);

        yield return null;

        GameManager.Instance.EnableGameplayControls();
        loadingManager.StartLoading("InGame");
    }

    private IEnumerator BeginCharacterCreation(int slotIndex)
    {
        FadeIn(startfade / 2f);

        yield return new WaitForSeconds(startfade / 2f);

        selectedSlotForCreation = slotIndex;
        selectField.SetActive(false);
        creationField.SetActive(true);
        OpenPanel(characterCreationPanel, true);

        yield return new WaitForSeconds(startfade / 2f);

        FadeOut(startfade / 2f);
    }

    public void OnCharacterClassSelected(int classIndex)
    {
        this.chosenClass = (CharacterType)classIndex;

        OpenPanel(nameInputPanel, false);
    }

    public void OnConfirmCreation()
    {
        string characterName = nameInputField.text;
        if (string.IsNullOrWhiteSpace(characterName))
        {
            return;
        }

        ActorProfile selectedProfile = null;
        switch (chosenClass)
        {
            case CharacterType.SlayerTypeA:
                selectedProfile = SlayerTypeA_Profile;
                break;
            case CharacterType.SlayerTypeB:
                selectedProfile = SlayerTypeB_Profile;
                break;
        }

        if (selectedProfile == null)
        {
            return;
        }

        GameData newData = new GameData();

        newData.characterInfo.actorProfileID = selectedProfile.name;
        newData.characterInfo.characterName = characterName;
        newData.characterInfo.level = 1;
        newData.characterInfo.experience = 0;
        newData.gold = 0;

        DataManager.Instance.gameData = newData;
        DataManager.Instance.currentSlotIndex = selectedSlotForCreation;
        DataManager.Instance.SaveGame();

        StartGame(selectedSlotForCreation);
    }

    //-----게임 패드
    private void SetSelectedUIElement(GameObject element)
    {
        EventSystem.current.SetSelectedGameObject(element);
    }

    //-----연출
    private void OpenPanel(GameObject panelToOpen, bool useAnimation)
    {
        GameObject panelToHide = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : selectSlotPanel;
        uiPanelStack.Push(panelToOpen);

        if (useAnimation)
        {
            StartCoroutine(AnimateAndSwitchPanels(panelToHide, panelToOpen));
        }
        else
        {
            panelToHide.SetActive(false);
            panelToOpen.SetActive(true);
        }

        if (panelToOpen == characterCreationPanel)
        {
            SetSelectedUIElement(CreaftCharacterFirstBtn.gameObject);
        }

        if (panelToOpen == nameInputPanel)
        {
            SetSelectedUIElement(nameInputField.gameObject);
        }
    }

    private IEnumerator CloseTopPanelAnimated()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            GameObject panelToShow = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : selectSlotPanel;

            yield return StartCoroutine(AnimateAndSwitchPanels(panelToClose, panelToShow));

            SetSelectedUIElement(selectSlotFirstBtn.gameObject);
        }
    }

    private void CloseTopPanelInstant()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            panelToClose.SetActive(false);
            GameObject panelToShow = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : selectSlotPanel;
            panelToShow.SetActive(true);
        }

        if (uiPanelStack.Count > 0)
        {
            GameObject newTopPanel = uiPanelStack.Peek();

            if (newTopPanel == characterCreationPanel)
                SetSelectedUIElement(CreaftCharacterFirstBtn.gameObject);
        }
        else
        {
            SetSelectedUIElement(selectSlotFirstBtn.gameObject);
        }
    }

    private IEnumerator AnimateAndSwitchPanels(GameObject panelToHide, GameObject panelToShow)
    {
        yield return StartCoroutine(Fade(1f, startfade / 2f));

        panelToHide.SetActive(false);
        panelToShow.SetActive(true);

        yield return StartCoroutine(Fade(0f, startfade / 2f));
    }

    private void FadeIn(float duration)
    {
        StartCoroutine(Fade(1f, duration));
    }

    private void FadeOut(float duration)
    {
        StartCoroutine(Fade(0f, duration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fade.color.a;
        float elapsedTime = 0f;
        Color currentColor = fade.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            fade.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            yield return null;
        }

        fade.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}
