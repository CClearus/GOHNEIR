using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VideoSettingsManager : MonoBehaviour
{
    [Header("UI Controls")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    private Resolution[] resolutions;

    void Start()
    {
        SetupResolutionDropdown();
        SetupQualityDropdown();
        LoadSettings();
    }

    // Populates available monitor resolutions dynamically
    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height} @{resolutions[i].refreshRateRatio.value:0}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // Populates Unity's built-in quality presets (Very Low, Medium, Ultra, etc.)
    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        List<string> options = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    // --- UI Listener Methods ---

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityIndex", qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isVSync ? 1 : 0);
    }

    // Restores saved settings on start
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("QualityIndex"))
        {
            int qIndex = PlayerPrefs.GetInt("QualityIndex");
            QualitySettings.SetQualityLevel(qIndex);
            qualityDropdown.value = qIndex;
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            bool isFS = PlayerPrefs.GetInt("Fullscreen") == 1;
            Screen.fullScreen = isFS;
            fullscreenToggle.isOn = isFS;
        }

        if (PlayerPrefs.HasKey("VSync"))
        {
            bool isVS = PlayerPrefs.GetInt("VSync") == 1;
            QualitySettings.vSyncCount = isVS ? 1 : 0;
            vsyncToggle.isOn = isVS;
        }

        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex");
            if (resIndex < resolutions.Length)
            {
                SetResolution(resIndex);
                resolutionDropdown.value = resIndex;
            }
        }
    }
}