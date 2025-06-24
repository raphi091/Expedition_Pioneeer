using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ActionPlayerInput;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    private ActionInput input;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        input = new ActionInput();
    }

    private void OnEnable()
    {
        input.Lobby.Cancel.performed += OnCancelPressed;
        input.Player.Menu.performed += OnPausePressed;
        PlayerControl.OnPlayerDied += HandlePlayerDeath;

        EnableUIControls();
    }

    private void OnDisable()
    {
        input.Lobby.Cancel.performed -= OnCancelPressed;
        input.Player.Menu.performed -= OnPausePressed;
        PlayerControl.OnPlayerDied -= HandlePlayerDeath;
    }

    public void EnableGameplayControls()
    {
        input.Lobby.Disable();
        input.Player.Enable();
    }

    public void EnableUIControls()
    {
        input.Player.Disable();
        input.Lobby.Enable();
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName.Equals("Lobby"))
        {
            FindObjectOfType<LobbyUIManager>()?.OnEsc();
        }
        else if (currentSceneName.Equals("Select"))
        {
            FindObjectOfType<SelectUIManager>()?.OnEsc();
        }
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        FindObjectOfType<InGameUIManager>()?.OnEsc();
    }

    private void HandlePlayerDeath()
    {
        
    }
}
