using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject StartPanel, LobbyPanel, SettingPanel;

    [Header("Start")]
    [SerializeField] private TextMeshProUGUI startTitle, startSubTitle;
    [SerializeField] private Image startLogo;
    [SerializeField] private GameObject StartBtn;
    [SerializeField] private Volume volume;
    [SerializeField] private Text StartBtnTex;

    [Header("BloomSet")]
    public float targetIntensity = 20f;
    public float bloomDuration = 1f;
    public float startDelay = 0.5f;

    private Bloom bloom;

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyTitlePanel;

    [Space(10)]
    [SerializeField] private Image fade;
    private float startfade = 1f;
    [SerializeField] Text Version;


    private void Awake()
    {
        StartPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        StartBtn.SetActive(false);
        // SettingPanel.SetActive(false);

        if (!volume.profile.TryGet(out bloom))
            Debug.LogWarning("LobbyUIManager ] Volume ] Bloom 없음");

        Version.text = $"v{Application.version}";
    }

    private IEnumerator Start()
    {
        StartCoroutine(PulseRoutine());

        yield return new WaitForSeconds(startDelay + bloomDuration);

        StartBtn.SetActive(true);
        StartCoroutine(FadeText(1f, startfade));
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
}
