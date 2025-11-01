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
        switch (local)
        {
            case "Janela":
                StartCoroutine(FadeOut(somJanela, 1f));
                break;
            case "Porta":
                StartCoroutine(FadeOut(somPorta, 1f));
                break;
            case "Armario":
                StartCoroutine(FadeOut(somRelogio, 1f));
                break;
        }
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
        switch (local)
        {
            case "Janela":
                somJanela.Play();
                break;
            case "Porta":
                somPorta.Play();
                break;
            case "Armario":
                somRelogio.Play();
                break;
        }
    }
}
