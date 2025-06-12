using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject StartPanel;
    [SerializeField] private GameObject LobbyPanel, OptionPanel, ExitPanel;

    [Header("Start")]
    [SerializeField] private Image startLogo;
    [SerializeField] private Text Version;
    [SerializeField] private GameObject StartBtn;
    [SerializeField] private Text StartBtnTex;
    [SerializeField] private Button exitFirstBtn;

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyTitlePanel;
    [SerializeField] private Button lobbyFirstBtn;

    [Header("Option")]
    [SerializeField] private GameObject screenPanel;
    [SerializeField] private GameObject soundPanel, keyPanel, resetPanel;
    [SerializeField] private Button optionFirstBtn;

    [Header("BloomSet")]
    public float targetIntensity = 20f;
    public float bloomDuration = 1f;
    public float startDelay = 0.5f;

    [SerializeField] private Volume volume;
    private Bloom bloom;

    [Header("Fade In/Out")]
    [SerializeField] private Image fade;
    private float startfade = 1f;

    private Stack<GameObject> uiPanelStack = new Stack<GameObject>();


    private void Awake()
    {
        StartPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        StartBtn.SetActive(false);
        OptionPanel.SetActive(false);
        ExitPanel.SetActive(false);

        if (!volume.profile.TryGet(out bloom))
            Debug.LogWarning("LobbyUIManager ] Volume ] Bloom 없음");

        Version.text = $"v{Application.version}";
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay / 2f);

        FadeOut(startfade);

        yield return new WaitForSeconds(startfade / 2f);

        StartCoroutine(PulseRoutine());

        yield return new WaitForSeconds(startDelay + bloomDuration);

        StartBtn.SetActive(true);
        StartCoroutine(FadeText(1f, startfade));

        yield return new WaitForSeconds(startfade / 2f);

        SetSelectedUIElement(StartBtn);

        while (true)
        {
            yield return new WaitForSeconds(bloomDuration * 20f);

            if (StartPanel.activeSelf)
                StartCoroutine(PulseRoutine());
        }
    }

    //-----버튼 영역
    public void OnEsc()
    {
        if (StartPanel.activeSelf) return;

        if (uiPanelStack.Count > 0)
        {
            GameObject topPanel = uiPanelStack.Peek();

            if (topPanel == OptionPanel)
            {
                StartCoroutine(CloseTopPanelAnimated());
            }
            else
            {
                CloseTopPanelInstant();
            }
        }
        else
        {
            if (!ExitPanel.activeSelf)
                OnExitButton();
            else
                OnExitNoButton();
        }
    }

    public void OnStartButton()
    {
        StartCoroutine(Startbtn(startfade));
    }

    private IEnumerator Startbtn(float duration)
    {
        FadeIn(duration);

        yield return new WaitForSeconds(duration);

        StartPanel.SetActive(false);
        LobbyPanel.SetActive(true);

        yield return new WaitForSeconds(duration / 5f);

        FadeOut(duration);

        SetSelectedUIElement(lobbyFirstBtn.gameObject);
    }

    public void OnOptionButton()
    {
        OpenPanel(OptionPanel, true);

        OnOptionScreenSet();
    }

    public void OnOptionScreenSet()
    {
        if (screenPanel.activeSelf) return;

        screenPanel.SetActive(true);
        soundPanel.SetActive(false);
        keyPanel.SetActive(false);
        resetPanel.SetActive(false);
    }

    public void OnOptionSoundSet()
    {
        if (soundPanel.activeSelf) return;

        screenPanel.SetActive(false);
        soundPanel.SetActive(true);
        keyPanel.SetActive(false);
        resetPanel.SetActive(false);
    }

    public void OnOptionKeySet()
    {
        if (keyPanel.activeSelf) return;

        screenPanel.SetActive(false);
        soundPanel.SetActive(false);
        keyPanel.SetActive(true);
        resetPanel.SetActive(false);
    }

    public void OnOptionReset()
    {
        if (resetPanel.activeSelf) return;

        screenPanel.SetActive(false);
        soundPanel.SetActive(false);
        keyPanel.SetActive(false);
        resetPanel.SetActive(true);
    }

    public void OnExitButton()
    {
        ExitPanel.SetActive(true);

        SetSelectedUIElement(exitFirstBtn.gameObject);
    }

    public void OnExitYesButton()
    {
        Application.Quit();
    }

    public void OnExitNoButton()
    {
        ExitPanel.SetActive(false);

        SetSelectedUIElement(lobbyFirstBtn.gameObject);
    }

    public void OnGameStart()
    {
        StartCoroutine(GameStart());
    }

    private IEnumerator GameStart()
    {
        FadeIn(startfade / 2f);

        yield return new WaitForSeconds(startfade / 2f);

        SceneManager.LoadScene("Select");
    }

    //-----게임 패드
    private void SetSelectedUIElement(GameObject element)
    {
        EventSystem.current.SetSelectedGameObject(element);
    }

    //-----연출 영역
    private void OpenPanel(GameObject panelToOpen, bool useAnimation)
    {
        GameObject panelToHide = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : LobbyPanel;
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

        if (panelToOpen == OptionPanel)
        {
            SetSelectedUIElement(optionFirstBtn.gameObject);
        }
    }

    private IEnumerator CloseTopPanelAnimated()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            GameObject panelToShow = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : LobbyPanel;

            yield return StartCoroutine(AnimateAndSwitchPanels(panelToClose, panelToShow));

            SetSelectedUIElement(lobbyFirstBtn.gameObject);
        }
    }

    private void CloseTopPanelInstant()
    {
        if (uiPanelStack.Count > 0)
        {
            GameObject panelToClose = uiPanelStack.Pop();
            panelToClose.SetActive(false);
            GameObject panelToShow = (uiPanelStack.Count > 0) ? uiPanelStack.Peek() : LobbyPanel;
            panelToShow.SetActive(true);
        }

        if (uiPanelStack.Count > 0)
        {
            GameObject newTopPanel = uiPanelStack.Peek();
            if (newTopPanel == OptionPanel) 
                SetSelectedUIElement(optionFirstBtn.gameObject);
        }
        else
        {
            SetSelectedUIElement(lobbyFirstBtn.gameObject);
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

    private IEnumerator FadeText(float targetAlpha, float duration)
    {
        float startAlpha = StartBtnTex.color.a;
        float elapsedTime = 0f;
        Color currentColor = StartBtnTex.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            StartBtnTex.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            yield return null;
        }

        StartBtnTex.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    private IEnumerator PulseRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        float startIntensity = bloom.intensity.value;
        float timer = 0f;

        while (timer < bloomDuration / 2)
        {
            bloom.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, timer / (bloomDuration / 2));

            timer += Time.deltaTime;
            yield return null;
        }

        bloom.intensity.value = targetIntensity;

        timer = 0f;
        while (timer < bloomDuration / 2)
        {
            bloom.intensity.value = Mathf.Lerp(targetIntensity, startIntensity, timer / (bloomDuration / 2));

            timer += Time.deltaTime;
            yield return null;
        }

        bloom.intensity.value = startIntensity;
    }
}
