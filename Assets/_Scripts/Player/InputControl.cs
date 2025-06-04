using UnityEngine;
using ActionPlayerInput;


public class InputControl : MonoBehaviour
{
    [HideInInspector] public ActionInput actionInput;


    void Awake()
    {
        actionInput = new ActionInput();
    }

    void OnEnable()
    {
        actionInput.Enable();
    }

    void OnDisable()
    {
        actionInput.Disable();
    }

    void OnDestroy()
    {
        actionInput.Dispose();
    }
}
