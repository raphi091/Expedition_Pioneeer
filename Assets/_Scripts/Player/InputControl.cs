using UnityEngine;
using ActionPlayerInput;


public class InputControl : MonoBehaviour
{
    [HideInInspector] public ActionInput actionInput;

    void Awake()
    {
        actionInput = new ActionInput();
    }

    void OnDestroy()
    {
        actionInput.Dispose();
    }

    void OnEnable()
    {
        actionInput.Enable();
    }

    void OnDisable()
    {
        actionInput.Disable();
    }
}
