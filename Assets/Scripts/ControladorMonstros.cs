using System.Collections;
using UnityEngine;

public class ControladorMonstros : MonoBehaviour
{
    [Header("Referências")]
    public AmbienteSonoro ambiente;
    public Transform jogador;
    public Light lanterna;

    [Header("Pontos de Aparição")]
    public Transform pontoJanela;
    public Transform pontoPorta;
    public Transform pontoRelogio;
    public Transform pontoCama; // Ponto especial

    [Header("Monstro")]
    public GameObject monstroPrefab;
    public float tempoParaSumirComLuz = 2f;
    public float distanciaMaxLuz = 10f;
    public float tempoMinEntreAparicoes = 5f;
    public float tempoMaxEntreAparicoes = 10f;
    public float velocidadeAtaque = 3f;
    public float tempoParadoAntesAtaque = 2f;

    private bool monstroAtivo = false;

    void Start()
    {
        StartCoroutine(CicloMonstro());
    }

    private IEnumerator CicloMonstro()
    {
        while (true)
        {
            float espera = Random.Range(tempoMinEntreAparicoes, tempoMaxEntreAparicoes);
            yield return new WaitForSeconds(espera);

            if (!monstroAtivo)
            {
                int ponto = Random.Range(0, 4);
                Transform pontoEscolhido = pontoJanela;

                switch (ponto)
                {
                    case 0: pontoEscolhido = pontoJanela; break;
                    case 1: pontoEscolhido = pontoPorta; break;
                    case 2: pontoEscolhido = pontoRelogio; break;
                    case 3: pontoEscolhido = pontoCama; break;
                }

                StartCoroutine(AparecerMonstro(pontoEscolhido));
            }
        }
    }

    private IEnumerator AparecerMonstro(Transform ponto)
    {
        monstroAtivo = true;

        yield return StartCoroutine(FadeOutSom(ponto));

        GameObject monstro = Instantiate(monstroPrefab, ponto.position, Quaternion.identity);
        MonstroBehavior behavior = monstro.AddComponent<MonstroBehavior>();
        behavior.Inicializar(jogador, lanterna, tempoParaSumirComLuz, distanciaMaxLuz, velocidadeAtaque, tempoParadoAntesAtaque, ambiente, ponto);

        yield return new WaitUntil(() => monstro == null);

        RestaurarSomPorPonto(ponto);
        monstroAtivo = false;
    }

    private IEnumerator FadeOutSom(Transform ponto)
    {
        // Se for o ponto embaixo da cama, fade em todos os sons
        if(ponto.name == "Cama")
        {
            AudioSource[] fontes = { ambiente.somJanela, ambiente.somPorta, ambiente.somRelogio };
            float[] volumesIniciais = new float[3];
            for(int i=0;i<3;i++) volumesIniciais[i] = fontes[i].volume;

            float t=0f;
            float tempoFade = 2f;
            while(t < tempoFade)
            {
                t += Time.deltaTime;
                for(int i=0;i<3;i++)
                    fontes[i].volume = Mathf.Lerp(volumesIniciais[i], 0f, t/tempoFade);
                yield return null;
            }

            AvisarMonstroPorPonto(ponto);
            yield break;
        }

        // Caso normal (Porta, Janela, Relógio)
        AudioSource fonte = ObterSomPorPonto(ponto);
        if (fonte == null) yield break;

        float tempoFadeNormal = 2f;
        float t2 = 0f;
        float volumeInicial = fonte.volume;

        while (t2 < tempoFadeNormal)
        {
            t2 += Time.deltaTime;
            fonte.volume = Mathf.Lerp(volumeInicial, 0f, t2/tempoFadeNormal);
            yield return null;
        }

        AvisarMonstroPorPonto(ponto);
    }

    private void RestaurarSomPorPonto(Transform ponto)
    {
        if(ponto.name == "Cama")
        {
            ambiente.RestaurarSom("Janela");
            ambiente.RestaurarSom("Porta");
            ambiente.RestaurarSom("Relogio");
        }
        else
        {
            switch (ponto.name)
            {
                case "Janela": ambiente.RestaurarSom("Janela"); break;
                case "Porta": ambiente.RestaurarSom("Porta"); break;
                case "Relogio": ambiente.RestaurarSom("Relogio"); break;
            }
        }
    }

    private AudioSource ObterSomPorPonto(Transform ponto)
    {
        switch (ponto.name)
        {
            case "Janela": return ambiente.somJanela;
            case "Porta": return ambiente.somPorta;
            case "Relogio": return ambiente.somRelogio;
        }
        return null; // Cama não tem som próprio
    }

    private void AvisarMonstroPorPonto(Transform ponto)
    {
        if(ponto.name == "Cama")
        {
            ambiente.AvisarMonstro("Janela");
            ambiente.AvisarMonstro("Porta");
            ambiente.AvisarMonstro("Relogio");
        }
        else
        {
            switch (ponto.name)
            {
                case "Janela": ambiente.AvisarMonstro("Janela"); break;
                case "Porta": ambiente.AvisarMonstro("Porta"); break;
                case "Relogio": ambiente.AvisarMonstro("Relogio"); break;
            }
        }
    }
}

// === Comportamento do monstro com animações ===
public class MonstroBehavior : MonoBehaviour
{
    private Transform jogador;
    private Light lanterna;
    private float tempoParaSumir;
    private float distanciaMaxLuz;
    private float velocidadeAtaque;
    private float tempoParado;
    private AmbienteSonoro ambiente;
    private Transform pontoSom;

    private float tempoIluminado = 0f;
    private bool atacando = false;
    private bool esperando = true;

    public Animator animator;

    public void Inicializar(Transform jogador, Light lanterna, float tempoParaSumir, float distanciaMaxLuz, float velocidadeAtaque, float tempoParadoAntesAtaque, AmbienteSonoro ambiente, Transform pontoSom)
    {
        this.jogador = jogador;
        this.lanterna = lanterna;
        this.tempoParaSumir = tempoParaSumir;
        this.distanciaMaxLuz = distanciaMaxLuz;
        this.velocidadeAtaque = velocidadeAtaque;
        this.tempoParado = tempoParadoAntesAtaque;
        this.ambiente = ambiente;
        this.pontoSom = pontoSom;

        animator = GetComponent<Animator>();

        // Animação inicial
        if(animator != null)
        {
            if(pontoSom.name == "Cama") animator.Play("UnderBed");
            else animator.Play("Idle");
        }

        StartCoroutine(EsperarAntesAtaque());
    }

    private IEnumerator EsperarAntesAtaque()
    {
        float timer = 0f;
        while (timer < tempoParado)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        esperando = false;
    }

    void Update()
    {
        // Verifica iluminação da lanterna
        if (lanterna != null && lanterna.enabled)
        {
            Vector3 direcao = transform.position - lanterna.transform.position;
            float distancia = direcao.magnitude;
            float angulo = Vector3.Angle(lanterna.transform.forward, direcao);

            if (angulo < lanterna.spotAngle / 2f && distancia <= distanciaMaxLuz)
            {
                tempoIluminado += Time.deltaTime;
                if (tempoIluminado >= tempoParaSumir) Desaparecer();
            }
            else tempoIluminado = 0f;
        }
        else tempoIluminado = 0f;

        // Começa ataque
        if (!esperando && !atacando)
        {
            atacando = true;
            if(animator != null) animator.Play("Walk");
            StartCoroutine(MoverParaJogador());
        }
    }

    private IEnumerator MoverParaJogador()
    {
        while (this != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, jogador.position, velocidadeAtaque * Time.deltaTime);

            if (Vector3.Distance(transform.position, jogador.position) < 1f)
            {
                if(animator != null) animator.Play("JumpScare");
                Debug.Log("💀 O jogador foi atacado!");
                Destroy(gameObject);
            }

            yield return null;
        }
    }

    private void Desaparecer()
    {
        Debug.Log("💡 Monstro iluminado e desapareceu!");
        Destroy(gameObject);
    }
}
