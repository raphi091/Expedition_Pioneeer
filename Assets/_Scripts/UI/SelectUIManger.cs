using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectUIManger : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("Fade In/Out")]
    [SerializeField] private Image fade;
    private float startfade = 1f;

    private LoadingManager loadingManager;


    private void Awake()
    {
        LoadingPanel.SetActive(false);

        if (!TryGetComponent(out loadingManager))
            Debug.Log("SelectUIManager ] LoadingManager 없음");
    }

    private IEnumerator Start()
    {
        FadeOut(startfade);

        yield return new WaitForSeconds(startfade / 2f);
    }

    public void OnStartPlayButton()
    {
        StartCoroutine(StartPlay());
    }

    private IEnumerator StartPlay()
    {
        FadeIn(startfade);

        yield return new WaitForSeconds(startfade);

        LoadingPanel.SetActive(true);

        yield return null;

        loadingManager.StartLoading("InGame");
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
