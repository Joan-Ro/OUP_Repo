using UnityEngine;
using UnityEngine.UI;

public class MusicaFondo : MonoBehaviour
{
    [Header("Audio")]
    private AudioSource audioSource;

    [Header("UI")]
    public Image iconoVolumen;

    [Header("Sprites de volumen")]
    public Sprite mute;
    public Sprite bajo;
    public Sprite medio;
    public Sprite alto;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ActualizarIcono(audioSource.volume);
    }

    // Este método lo sigue llamando el SLIDER
    public void SetVolumen(float volumen)
    {
        audioSource.volume = volumen;
        ActualizarIcono(volumen);
    }

    void ActualizarIcono(float valor)
    {
        if (iconoVolumen == null) return;

        if (valor <= 0.01f)
            iconoVolumen.sprite = mute;
        else if (valor < 0.33f)
            iconoVolumen.sprite = bajo;
        else if (valor < 0.66f)
            iconoVolumen.sprite = medio;
        else
            iconoVolumen.sprite = alto;
    }
}
