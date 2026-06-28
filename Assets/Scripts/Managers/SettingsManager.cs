using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the persistent storage and application of global user preferences.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField]
    private Slider _volumeControlInterface;
    [SerializeField]
    private Slider _sensitivityControlInterface;

    public static float GlobalSensitivity = 1f;

    private float _fallbackVolume = 0.8f;
    private float _fallbackSensitivity = 0.5f;

    private const string PREFS_KEY_VOLUME = "SysVolume_Prefs";
    private const string PREFS_KEY_SENSITIVITY = "SysSens_Prefs";

    private void Start()
    {
        ApplyStoredPreferences();

        if ( _volumeControlInterface != null )
        {
            _volumeControlInterface.minValue = 0f;
            _volumeControlInterface.maxValue = 1f;
            _volumeControlInterface.onValueChanged.AddListener(SetVolume);
        }

        if ( _sensitivityControlInterface != null )
        {
            _sensitivityControlInterface.minValue = 0.1f;
            _sensitivityControlInterface.maxValue = 3f;
            _sensitivityControlInterface.onValueChanged.AddListener(SetSensitivity);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PREFS_KEY_VOLUME, value);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        GlobalSensitivity = value;
        PlayerPrefs.SetFloat(PREFS_KEY_SENSITIVITY, value);
        PlayerPrefs.Save();
    }

    private void ApplyStoredPreferences()
    {
        float retrievedVolume = PlayerPrefs.GetFloat(PREFS_KEY_VOLUME, _fallbackVolume);
        AudioListener.volume = retrievedVolume;

        if ( _volumeControlInterface != null )
        {
            _volumeControlInterface.value = retrievedVolume;
        }

        float retrievedSensitivity = PlayerPrefs.GetFloat(PREFS_KEY_SENSITIVITY, _fallbackSensitivity);
        GlobalSensitivity = retrievedSensitivity;

        if ( _sensitivityControlInterface != null )
        {
            _sensitivityControlInterface.value = retrievedSensitivity;
        }
    }
}