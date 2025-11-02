using System.Collections;
using UnityEngine;

public class AmbienteSonoro : MonoBehaviour
{
    [Header("Fontes de Som 3D")]
    public AudioSource somJanela;
    public AudioSource somPorta;
    public AudioSource somRelogio;

    public void AvisarMonstro(string local)
    {
        AudioSource fonte = GetFonte(local);
        if (fonte != null && fonte.isPlaying)
            StartCoroutine(FadeOut(fonte, 1f));
    }

    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public void RestaurarSom(string local)
    {
        AudioSource fonte = GetFonte(local);
        if (fonte != null)
        {
            fonte.volume = 1f;
            if (!fonte.isPlaying)
                fonte.Play();
        }
    }

    private AudioSource GetFonte(string local)
    {
        switch (local)
        {
            case "Janela": return somJanela;
            case "Porta": return somPorta;
            case "Relogio": return somRelogio;
            default: return null;
        }
    }
}
