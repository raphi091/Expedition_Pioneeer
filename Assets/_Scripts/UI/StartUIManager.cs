using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    [SerializeField] private GameObject StartUI;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private Text statusText;


    private void Awake()
    {
        StartUI.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LoadLobbyScene());
    }

    private IEnumerator LoadLobbyScene()
    {
        yield return null;

        StartUI.SetActive(true);

        yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Lobby");

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            loadingBar.value = progress;
            statusText.text = $"데이터 확인 중...";

            if (asyncOperation.progress >= 0.9f)
            {
                statusText.text = "데이터 확인 중...";
                loadingBar.value = 1f;

                yield return new WaitForSeconds(1f);

                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

