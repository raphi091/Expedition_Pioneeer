using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ActionPlayerInput;


public class StartPanel : MonoBehaviour
{
    [SerializeField] private LobbyUIManager manager;
    private ActionInput input;


    private void Awake()
    {
        input = new ActionInput();
    }

    private void OnEnable()
    {
        input.Lobby.AnyButton.performed += OnAnyButton;
    }

    private void OnDisable()
    {
        input.Lobby.AnyButton.performed -= OnAnyButton;
    }

    private void OnAnyButton(InputAction.CallbackContext context)
    {
        if (manager != null)
        {
            manager.OnStartButton();
            this.enabled = false;
        }
        else
        {
            Debug.LogError("StartPanel ] LobbyUIManger 없음");
        }
    }
}
