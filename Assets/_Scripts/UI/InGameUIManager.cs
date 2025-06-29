using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance = null;

    [Header("Menu")]
    [SerializeField] private GameObject playerinfoPanel;
    [SerializeField] private GameObject menuPanel, savePanel, titlePanel, quitPanel, backPanel;

    [Header("Save")]
    [SerializeField] private GameObject savingPanel;
    [SerializeField] private Text saveText;

    [Header("Titel")]
    [SerializeField] private GameObject savetotitlePanel;
    [SerializeField] private GameObject saveandtitlePanel;
    [SerializeField] private GameObject dontsaveandtitlePanel;
    [SerializeField] private Text saveTitleText;

    [Header("Quit")]
    [SerializeField] private GameObject savetoquitPanel;
    [SerializeField] private GameObject saveandquitPanel;
    [SerializeField] private GameObject dontsaveandquitPanel;
    [SerializeField] private Text saveQuitText;

    [Header("Interaction")]
    [SerializeField] private GameObject storagePanel;

    [Header("Death")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject respawnPanel;

    [Header("Fade In/Out")]
    [SerializeField] private Image fade;
    private float startfade = 1f;

    private bool isPause = false;
    public bool IsPause => isPause;

    private Stack<GameObject> uiPanelStack = new Stack<GameObject>();


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

        playerinfoPanel.SetActive(true);
        menuPanel.SetActive(false);
        savePanel.SetActive(false);
        quitPanel.SetActive(false);
        backPanel.SetActive(false);
        storagePanel.SetActive(false);
        deathPanel.SetActive(false);
        respawnPanel.SetActive(false);

        fade.gameObject.SetActive(true);
    }

    private IEnumerator Start()
    {
        FadeOut(startfade);
        SoundManager.Instance.PlayBGM(BGMTrackName.Village);

        yield return new WaitForSeconds(startfade);
    }

    //-----버튼
    public void OnEsc()
    {
        if (uiPanelStack.Count > 0)
        {
            CloseTopPanelInstant();

            if (uiPanelStack.Count == 0)
            {
                FindObjectOfType<PlayerMoveControl>().CursorLockState();
                backPanel.SetActive(false);
                PauseOFF();

                if (!storagePanel.activeSelf)
                {
                    PlayerInteractionControl.Instance.EndInteraction();
                }
            }
        }
        else
        {
            FindObjectOfType<PlayerMoveControl>().CursorLockState();
            backPanel.SetActive(true);
            PauseON();
            OpenPanel(menuPanel);
        }
    }

    public void PauseON()
    {
        isPause = true;
        Time.timeScale = 0f;
    }

    public void PauseOFF()
    {
        isPause = false;
        Time.timeScale = 1f;
    }

    public void OnSaveBtn()
    {
        OpenPanel(savePanel);
    }

    public void OnSave()
    {
        StartCoroutine(SaveGame());
    }

    private IEnumerator SaveGame()
    {
        OpenPanel(savingPanel);
        saveText.text = "게임 저장 중...";
        InventoryManager.Instance.SaveInventoryToDataManager();
        DataManager.Instance.SaveGame();

        yield return new WaitForSecondsRealtime(1f);

        saveText.text = "저장이 완료되었습니다.";

        yield return new WaitForSecondsRealtime(0.2f);

        OnEsc();
        OnEsc();
    }

    public void OnTitleBtn()
    {
        OpenPanel(titlePanel);
    }

    public void OnsaveTitleBtn()
    {
        StartCoroutine(OnsaveTitle());
    }

    private IEnumerator OnsaveTitle()
    {
        OpenPanel(saveandtitlePanel);
        saveTitleText.text = "게임 저장 중...";
        InventoryManager.Instance.SaveInventoryToDataManager();
        DataManager.Instance.SaveGame();

        yield return new WaitForSecondsRealtime(1f);

        saveTitleText.text = "저장이 완료되었습니다.\n타이틀 화면으로 돌아갑니다.";

        yield return new WaitForSecondsRealtime(0.2f);

        FadeIn(startfade);

        yield return new WaitForSecondsRealtime(startfade);

        Time.timeScale = 1f;
        SceneManager.LoadScene("Lobby");
    }

    public void OndontSaveTitleBtn()
    {
        OpenPanel(dontsaveandtitlePanel);
    }

    public void OndontSaveTitleYesBtn()
    {
        StartCoroutine(DontSaveTitle());
    }

    private IEnumerator DontSaveTitle()
    {
        FadeIn(startfade);

        yield return new WaitForSecondsRealtime(startfade);

        Time.timeScale = 1f;
        SceneManager.LoadScene("Lobby");
    }

    public void OnQuitBtn()
    {
        OpenPanel(quitPanel);
    }

    public void OnsaveQuitBtn()
    {
        StartCoroutine(OnsaveQuit());
    }

    private IEnumerator OnsaveQuit()
    {
        OpenPanel(saveandquitPanel);
        saveQuitText.text = "게임 저장 중...";
        InventoryManager.Instance.SaveInventoryToDataManager();
        DataManager.Instance.SaveGame();

        yield return new WaitForSecondsRealtime(1f);

        saveQuitText.text = "저장이 완료되었습니다.";

        yield return new WaitForSecondsRealtime(0.2f);

        Time.timeScale = 1f;
        Application.Quit();
    }

    public void OndontSaveQuitBtn()
    {
        OpenPanel(dontsaveandquitPanel);
    }

    public void OndontSaveQuitYesBtn()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void OnOpenStorage()
    {
        OpenPanel(storagePanel);

        FindObjectOfType<PlayerMoveControl>().CursorLockState();
        backPanel.SetActive(true);
        PauseON();
    }

    public void PlayerDeath()
    {
        StartCoroutine(Death_co());
    }

    private IEnumerator Death_co()
    {
        deathPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        FadeIn(startfade);

        yield return new WaitForSeconds(1f);

        deathPanel.SetActive(false);
        respawnPanel.SetActive(true);
    }

    public void PlayerRespawn()
    {
        StartCoroutine(PlayerRespawn_co());
    }

    private IEnumerator PlayerRespawn_co()
    {
        respawnPanel.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        FadeOut(startfade);
    }

    //-----연출
    private void OpenPanel(GameObject panelToOpen)
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToHide = uiPanelStack.Peek();
            panelToHide.SetActive(false);
        }

        uiPanelStack.Push(panelToOpen);
        panelToOpen.SetActive(true);
    }

    private void CloseTopPanelInstant()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            panelToClose.SetActive(false);

            if (uiPanelStack.Count > 0)
            {
                GameObject panelToShow = uiPanelStack.Peek();
                panelToShow.SetActive(true);
            }
        }
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
