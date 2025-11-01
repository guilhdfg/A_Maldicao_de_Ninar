using System.Collections;
using UnityEngine;

public class ControladorMonstros : MonoBehaviour
{
    [Header("Referências")]
    public AmbienteSonoro ambiente;

    [Header("Pontos do Monstro")]
    public Transform pontoJanela;
    public Transform pontoPorta;
    public Transform pontoArmario;

    [Header("Configurações do Monstro")]
    public GameObject monstroPrefab;          // Prefab do monstro
    public float tempoMinEntreAparicoes = 5f; // tempo mínimo entre aparições
    public float tempoMaxEntreAparicoes = 10f; // tempo máximo entre aparições
    public float duracaoAparicao = 4f;        // quanto tempo o monstro fica visível

    private bool monstroAtivo = false;

    void Start()
    {
        StartCoroutine(CicloMonstro());
    }

    private IEnumerator CicloMonstro()
    {
        while (true)
        {
            // Espera um tempo aleatório entre aparições
            float espera = Random.Range(tempoMinEntreAparicoes, tempoMaxEntreAparicoes);
            yield return new WaitForSeconds(espera);

            // Escolhe um ponto aleatório (0 = janela, 1 = porta, 2 = armário)
            int ponto = Random.Range(0, 3);

            switch (ponto)
            {
                case 0:
                    StartCoroutine(AparecerMonstro("Janela", pontoJanela));
                    break;
                case 1:
                    StartCoroutine(AparecerMonstro("Porta", pontoPorta));
                    break;
                case 2:
                    StartCoroutine(AparecerMonstro("Armario", pontoArmario));
                    break;
            }
        }
    }

    private IEnumerator AparecerMonstro(string local, Transform ponto)
    {
        if (monstroAtivo) yield break; // Evita dois monstros ao mesmo tempo

        monstroAtivo = true;

        // Cria o monstro
        GameObject monstro = Instantiate(monstroPrefab, ponto.position, ponto.rotation);

        // Para o som do local
        ambiente.AvisarMonstro(local);

        Debug.Log($"👁️ Monstro apareceu na {local}! Som parou.");

        // Espera o tempo configurado
        yield return new WaitForSeconds(duracaoAparicao);

        // Some com o monstro
        Destroy(monstro);

        // Volta o som
        ambiente.RestaurarSom(local);

        Debug.Log($"💨 Monstro sumiu da {local}. Som voltou.");

        monstroAtivo = false;
    }
}
