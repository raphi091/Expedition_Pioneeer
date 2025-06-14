using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


[RequireComponent(typeof(CinemachineFreeLook))]
public class CameraSensitivityControl : MonoBehaviour
{
    private CinemachineFreeLook playercamera;

    private float mouseX;
    private float mouseY;

    private void Awake()
    {
        if (!TryGetComponent(out playercamera))
            Debug.LogWarning("CameraSensitivityControl ] CinemachineFreeLook 없음");

        if (playercamera != null)
        {
            mouseX = playercamera.m_XAxis.m_MaxSpeed;
            mouseY = playercamera.m_YAxis.m_MaxSpeed;
        }
    }

    private void Start()
    {
        UpdateSensitivity();
    }

    public void UpdateSensitivity()
    {
        if (playercamera == null || DataManager.Instance == null)
        {
            return;
        }

        float sensitivity = DataManager.Instance.setting.mouseSensitivity;

        playercamera.m_XAxis.m_MaxSpeed = mouseX * sensitivity;
        playercamera.m_YAxis.m_MaxSpeed = mouseY * sensitivity;
    }
}
