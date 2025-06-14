using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;


public class SettingManager : MonoBehaviour
{
    [Header("Screen Setting")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Sound Setting")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider bgmVolume;
    [SerializeField] private Slider sfxVolume;

    [Header("Mouse Setting")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TextMeshProUGUI mouseSensitivityText;

    private Resolution[] resolutions;

    private void Start()
    {
        if (resolutionDropdown == null || fullscreenToggle == null)
        {
            return;
        }

        SetupResolutions();
        LoadSettingsToUI();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgmVolume.onValueChanged.AddListener(SetBGMVolume);
        sfxVolume.onValueChanged.AddListener(SetSFXVolume);

        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

   private void SetupResolutions()
    {
        float targetAspectRatio = 1920f / 1080f;

        resolutions = Screen.resolutions
            .Where(res => Mathf.Abs((float)res.width / res.height - targetAspectRatio) < 0.01f)
            .Distinct()
            .ToArray();

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        resolutionDropdown.AddOptions(options);
    }

    private void LoadSettingsToUI()
    {
        GameSetting settings = DataManager.Instance.setting;

        fullscreenToggle.isOn = settings.fullscreenMode == FullScreenMode.FullScreenWindow;
        Screen.fullScreenMode = settings.fullscreenMode;

        int savedResolutionIndex = settings.resolutionIndex;
        if (savedResolutionIndex != -1 && savedResolutionIndex < resolutions.Length)
        {
            resolutionDropdown.value = savedResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            SetResolution(savedResolutionIndex);
        }
        else
        {
            int currentResolutionIndex = resolutions.ToList().FindIndex(res => res.width == Screen.width && res.height == Screen.height);
            if(currentResolutionIndex != -1)
            {
                resolutionDropdown.value = currentResolutionIndex;
            }
        }

        masterVolume.value = settings.masterVolume;
        bgmVolume.value = settings.bgmVolume;
        sfxVolume.value = settings.sfxVolume;

        SetMasterVolume(settings.masterVolume);
        SetBGMVolume(settings.bgmVolume);
        SetSFXVolume(settings.sfxVolume);

        mouseSensitivitySlider.value = settings.mouseSensitivity;
        UpdateMouseSensitivityText(settings.mouseSensitivity);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        
        DataManager.Instance.setting.resolutionIndex = resolutionIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;

        DataManager.Instance.setting.fullscreenMode = mode;
    }

    public void SetMasterVolume(float value)
    {
        float volume = value > 0.0001f ? Mathf.Log10(value) * 20 : -80f;
        mixer.SetFloat("MasterVolume", volume);
        DataManager.Instance.setting.masterVolume = value;
    }

    public void SetBGMVolume(float value)
    {
        float volume = value > 0.0001f ? Mathf.Log10(value) * 20 : -80f;
        mixer.SetFloat("BGMVolume", volume);
        DataManager.Instance.setting.bgmVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        float volume = value > 0.0001f ? Mathf.Log10(value) * 20 : -80f;
        mixer.SetFloat("SFXVolume", volume);
        DataManager.Instance.setting.sfxVolume = value;
    }

    public void SetMouseSensitivity(float value)
    {
        DataManager.Instance.setting.mouseSensitivity = value;
        UpdateMouseSensitivityText(value);
    }

    private void UpdateMouseSensitivityText(float value)
    {
        if (mouseSensitivityText != null)
        {
            mouseSensitivityText.text = value.ToString("F2");
        }
    }

    public void OnClickConfirmReset()
    {
        DataManager.Instance.setting = new GameSetting();

        LoadSettingsToUI();
        ApplyAndSaveChanges();
    }

    public void ApplyAndSaveChanges()
    {
        DataManager.Instance.SaveSettings();

        var sensitivityController = FindObjectOfType<CameraSensitivityControl>();

        if (sensitivityController != null)
        {
            sensitivityController.UpdateSensitivity();
        }
    }
}
