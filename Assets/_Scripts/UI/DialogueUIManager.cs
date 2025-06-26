using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DialogueUIManager : MonoBehaviour
{
    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Popup")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        popupPanel.SetActive(false);
    }

    public void ShowMassage(string message, float duration)
    {
        StartCoroutine(ShowMessage_co(message, duration));
    }

    public void ShowPopup(string message, float duration)
    {
        StartCoroutine(ShowPopup_co(message, duration));
    }

    private IEnumerator ShowMessage_co(string message, float duration)
    {
        dialogueText.text = message;
        dialoguePanel.SetActive(true);

        yield return new WaitForSeconds(duration);

        dialoguePanel.SetActive(false);
    }

    private IEnumerator ShowPopup_co(string message, float duration)
    {
        popupText.text = message;
        popupPanel.SetActive(true);

        yield return new WaitForSeconds(duration);

        popupPanel.SetActive(false);
    }
}
