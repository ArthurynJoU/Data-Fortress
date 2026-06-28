using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates a continuous cyclical colour shift to create a dynamic rainbow effect.
/// </summary>
public class RainbowGlow : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.1f;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        if ( _image != null )
        {
            Color rainbowColor = Color.HSVToRGB(Mathf.PingPong(Time.time * _speed, 1f), 0.6f, 1f);
            rainbowColor.a = 0.3f;

            _image.color = rainbowColor;
        }
    }
}