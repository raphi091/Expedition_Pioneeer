using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Vector3 lookAtMiniMapCamera = new Vector3 (90f, 0f, 104.3f);

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(lookAtMiniMapCamera);
    }
}
