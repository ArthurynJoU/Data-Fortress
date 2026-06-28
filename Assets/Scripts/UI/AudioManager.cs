using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Interface Sounds")]
    [SerializeField]
    private AudioClip _buttonClickSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayButtonSound()
    {
        if ( _buttonClickSound != null && _audioSource != null )
        {
            _audioSource.PlayOneShot(_buttonClickSound);
        }
    }
}