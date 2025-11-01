using UnityEngine;

public class CameraOlharPontos : MonoBehaviour
{
    [Header("Pontos para olhar")]
    public Transform pontoEsquerda;
    public Transform pontoCentro;
    public Transform pontoDireita;

    [Header("Configurações")]
    public float velocidadeRotacao = 3f; // quanto mais alto, mais rápido gira

    private Transform pontoAtual;

    void Start()
    {
        // Começa olhando para o ponto central
        pontoAtual = pontoCentro;
    }

    void Update()
    {
        // Detecta entrada do jogador
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            pontoAtual = pontoEsquerda;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            pontoAtual = pontoDireita;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            pontoAtual = pontoCentro;

        // Faz a rotação suave em direção ao ponto atual
        Quaternion rotAlvo = Quaternion.LookRotation(pontoAtual.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotAlvo, Time.deltaTime * velocidadeRotacao);
    }
}
