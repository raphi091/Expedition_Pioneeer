using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartUIManager : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI statusText;

    IEnumerator Start()
    {
        progressBar.value = 0;

        yield return StartCoroutine(LoadPhase("코어 시스템 초기화...", 0.3f, 0.5f));

        yield return StartCoroutine(LoadPhase("데이터 확인...", 0.6f, 0.7f));

        yield return StartCoroutine(LoadPhase("설정 불러오는 중...", 1.0f, 0.3f));

        statusText.text = "로딩 완료!";
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("LobbyScene");
    }

    // 로딩의 각 단계를 처리하는 코루틴
    IEnumerator LoadPhase(string status, float targetProgress, float duration)
    {
        statusText.text = status;
        float startProgress = progressBar.value;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            progressBar.value = Mathf.Lerp(startProgress, targetProgress, timer / duration);
            yield return null;
        }

        progressBar.value = targetProgress;
    }
}
