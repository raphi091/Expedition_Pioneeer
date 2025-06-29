using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSetting
{
    public int resolutionIndex;
    public FullScreenMode fullscreenMode;

    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;

    public float mouseSensitivity;

    public GameSetting()
    {
        resolutionIndex = -1;
        fullscreenMode = FullScreenMode.FullScreenWindow;
        masterVolume = 1f;
        bgmVolume = 1f;
        sfxVolume = 1f;
        mouseSensitivity = 1f;
    }
}
