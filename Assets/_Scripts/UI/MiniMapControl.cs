using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapControl : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float heightOffset = 20f;
    [SerializeField] private RectTransform playerIcon;

    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 newPosition = playerTransform.position;
        newPosition.y += heightOffset;
        transform.position = newPosition;

        // 맵 고정
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        if (playerIcon != null)
        {
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -playerTransform.eulerAngles.y);
        }

        // 맵 회전
        //transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);

        //if (playerIcon != null)
        //{
        //    playerIcon.localRotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.y);
        //}

    }
}
