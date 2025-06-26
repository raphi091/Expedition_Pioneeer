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

    [Header("Slot Options UI")]
    [SerializeField] private TextMeshProUGUI nicknameText;

    [Header("Character Profile")]
    [SerializeField] private ActorProfile SlayerTypeA_Profile;
    [SerializeField] private ActorProfile SlayerTypeB_Profile;

    [Header("Character Set")]
    [SerializeField] private GameObject characterCreationPanel;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField newNameInputField;

    [Header("Loading")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("Fade In/Out")]
    [SerializeField] private Image fade;
    private float startfade = 1f;

    [Header("Panel / Field")]
    [SerializeField] private GameObject selectSlotPanel;
    [SerializeField] private GameObject slotOptionPanel;
    [SerializeField] private GameObject slotDeletePanel;
    [SerializeField] private GameObject slotRenamePanel;
    [SerializeField] private GameObject selectField;
    [SerializeField] private GameObject creationField;

    [Header("Button")]
    [SerializeField] private Button selectSlotFirstBtn;
    [SerializeField] private Button slotOptionFirstBtn;
    [SerializeField] private Button slotDeleteFirstBtn;
    [SerializeField] private Button CreaftCharacterFirstBtn;

    [Header("SFX")]
    public AudioClip ButtonClip;

    private Stack<GameObject> uiPanelStack = new Stack<GameObject>();
    private LoadingManager loadingManager;
    private CharacterPreview characterPreview;
    private CharacterType chosenClass;
    private int selectedSlotForCreation;
    private int confirmedSlotIndex;


    private void Awake()
    {
        selectField.SetActive(true);
        creationField.SetActive(false);

        selectSlotPanel.SetActive(true);
        slotOptionPanel.SetActive(false);
        slotDeletePanel.SetActive(false);
        slotRenamePanel.SetActive(false);
        characterCreationPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        LoadingPanel.SetActive(false);

        fade.gameObject.SetActive(true);

        if (!TryGetComponent(out loadingManager))
            Debug.Log("SelectUIManager ] LoadingManager 없음");

        if (!TryGetComponent(out characterPreview))
            Debug.Log("SelectUIManager ] CharacterPreview 없음");
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

            if (topPanel == slotOptionPanel)
            {
                StartCoroutine(CloseTopPanelAnimated());
                characterPreview.ClearCharacter();
            }
            else if (topPanel == characterCreationPanel)
            {
                StartCoroutine(CloseTopPanelAnimated());
                selectField.SetActive(true);
                creationField.SetActive(false);
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
        SoundManager.Instance.PlaySFX(ButtonClip);
        FadeIn(startfade);

        yield return new WaitForSeconds(startfade);

        SceneManager.LoadScene("Lobby");
    }

    public void OnSlotSelected(int slotIndex)
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
        GameData data = DataManager.Instance.GetDataForSlot(slotIndex);

        if (data != null)
        {
            StartCoroutine(SelectData(data, slotIndex));
        }
        else
        {
            StartCoroutine(BeginCharacterCreation(slotIndex));
        }
    }

    private IEnumerator SelectData(GameData data, int slotIndex)
    {
        FadeIn(startfade / 2f);

        yield return new WaitForSeconds(startfade / 2f);

        nicknameText.text = data.characterInfo.characterName;
        confirmedSlotIndex = slotIndex;
        characterPreview.ShowCharacter(data.characterInfo.actorProfileID);
        OpenPanel(slotOptionPanel, false);

        yield return new WaitForSeconds(startfade / 2f);

        FadeOut(startfade / 2f);
    }

    private IEnumerator BeginCharacterCreation(int slotIndex)
    {
        FadeIn(startfade / 2f);

        yield return new WaitForSeconds(startfade / 2f);

        selectedSlotForCreation = slotIndex;
        selectField.SetActive(false);
        creationField.SetActive(true);
        OpenPanel(characterCreationPanel, false);

        yield return new WaitForSeconds(startfade / 2f);

        FadeOut(startfade / 2f);
    }

    public void OnCharacterClassSelected(int classIndex)
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
        this.chosenClass = (CharacterType)classIndex;

        OpenPanel(nameInputPanel, false);
    }

    public void OnConfirmCreation()
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
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

        GameData newData = new GameData(characterName);

        newData.characterInfo.actorProfileID = selectedProfile.alias;
        newData.characterInfo.level = 1;
        newData.characterInfo.experience = 0;
        newData.gold = 0;

        DataManager.Instance.startType = GameStartType.NewGame;
        DataManager.Instance.gameData = newData;
        DataManager.Instance.currentSlotIndex = selectedSlotForCreation;

        DataManager.Instance.SaveGame();

        DataManager.Instance.RecordSessionStartTime();

        StartCoroutine(StartGame(selectedSlotForCreation));
    }

    public void OnStartGame()
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
        DataManager.Instance.startType = GameStartType.LoadGame;
        StartCoroutine(StartGame(confirmedSlotIndex));
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

    public void OnDeleteData()
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
        OpenPanel(slotDeletePanel, false);
    }

    public void OnDelete()
    {
        DataManager.Instance.DeleteData(confirmedSlotIndex);
        saveSlots[confirmedSlotIndex].Setup(this, confirmedSlotIndex, null);
        OnEsc();
        OnEsc();
    }

    public void OnRenameCharacter()
    {
        SoundManager.Instance.PlaySFX(ButtonClip);
        OpenPanel(slotRenamePanel, false);
    }

    public void OnRename()
    {
        string newName = newNameInputField.text;

        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        DataManager.Instance.RenameCharacter(confirmedSlotIndex, newName);
        saveSlots[confirmedSlotIndex].Setup(this, confirmedSlotIndex, DataManager.Instance.GetDataForSlot(confirmedSlotIndex));
        OnEsc();
        OnEsc();
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

        if (panelToOpen == slotOptionPanel)
        {
            SetSelectedUIElement(slotOptionFirstBtn.gameObject);
        }

        if (panelToOpen == slotDeletePanel)
        {
            SetSelectedUIElement(slotDeleteFirstBtn.gameObject);
        }

        if (panelToOpen == slotRenamePanel)
        {
            SetSelectedUIElement(newNameInputField.gameObject);
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
            SoundManager.Instance.PlaySFX(ButtonClip);

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
        SoundManager.Instance.PlaySFX(ButtonClip);
        
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
