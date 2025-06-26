using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerControl player);
    void Highlight();
    void Unhighlight();

    Vector3 GetPromptPosition();
    string GetInteractionPrompt();
}
