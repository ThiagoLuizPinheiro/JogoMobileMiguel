using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Área do cano")]
    [SerializeField] private float tamanhoArea = 4f;

    [Header("Distância de spawn")]
    [SerializeField] private float distanciaSpawn = 30f;
    [SerializeField] private float distanciaEntreLinhas = 8f;

    [Header("Quantidade")]
    [SerializeField] private int obstaculosPorLinha = 12;
    [SerializeField] private int obstaculosMaximosPorLinha = 22;
    [SerializeField] private float aumentoDificuldadePorDistancia = 0.01f;

    [Header("Tamanho dos obstáculos")]
    [SerializeField] private float tamanhoMinimo = 0.7f;
    [SerializeField] private float tamanhoMaximo = 2f;

    [Header("Formas (variedade)")]
    [Tooltip("Chance de o obstaculo virar uma barra comprida e fina.")]
    [SerializeField, Range(0f, 1f)] private float chanceBarra = 0.3f;
    [SerializeField] private float comprimentoMaximoBarra = 4f;
    [Tooltip("Chance de girar o obstaculo (deixa o quadrado mais interessante).")]
    [SerializeField, Range(0f, 1f)] private float chanceGirar = 0.4f;

    [Header("Passagem")]
    [Tooltip("Raio livre da passagem. Precisa ser MAIOR que o raio da bola.")]
    [SerializeField] private float espacoPassagem = 1.5f;
    [Tooltip("A passagem vai diminuindo com a profundidade ate este valor.")]
    [SerializeField] private float espacoPassagemMinimo = 1.1f;
    [SerializeField] private float reducaoPassagemPorDistancia = 0.002f;
    [SerializeField] private float distanciaMinimaEntreObstaculos = 0.8f;

    [Header("Anti ficar parado")]
    [Tooltip("A passagem de cada linha muda de lugar pelo menos isso em relacao a anterior.")]
    [SerializeField] private float distanciaMinimaMudancaPassagem = 3.5f;
    [Tooltip("E no maximo isso, pra sempre dar tempo de chegar nela.")]
    [SerializeField] private float distanciaMaximaMudancaPassagem = 5.5f;
    [Tooltip("Chance de nascer um obstaculo exatamente na posicao atual do jogador.")]
    [SerializeField, Range(0f, 1f)] private float chanceMirarJogador = 0.7f;

    [Header("Limpeza")]
    [SerializeField] private float distanciaParaDestruir = 30f;

    private readonly List<GameObject> obstaculosAtivos = new();
    private float proximaLinhaY;
    private float alturaInicial;
    private Vector2 passagemAnterior;

    private void Start()
    {
        if (player == null || obstaclePrefab == null)
        {
            enabled = false;
            return;
        }

        IniciarLinhas(player.position);
    }

    private void Update()
    {
        if (player == null)
            return;

        while (player.position.y - proximaLinhaY < distanciaSpawn)
            CriarLinha();

        LimparObstaculos();
    }

    private void IniciarLinhas(Vector3 origem)
    {
        alturaInicial = origem.y;
        proximaLinhaY = origem.y - distanciaSpawn;

        // A primeira passagem parte de onde o jogador comeca.
        passagemAnterior = LimitarNaArea(new Vector2(origem.x, origem.z));

        for (int i = 0; i < 5; i++)
            CriarLinha();
    }

    // Chame ao reiniciar o jogo (depois do ResetarJogo do player),
    // passando a posicao inicial do player, pra recriar os obstaculos do zero.
    public void Reiniciar(Vector3 posicaoInicialPlayer)
    {
        foreach (GameObject obstaculo in obstaculosAtivos)
        {
            if (obstaculo != null)
                Destroy(obstaculo);
        }

        obstaculosAtivos.Clear();
        IniciarLinhas(posicaoInicialPlayer);
    }

    private void CriarLinha()
    {
        float profundidade = Mathf.Max(0f, alturaInicial - proximaLinhaY);

        float raioPassagem = Mathf.Max(
            espacoPassagemMinimo,
            espacoPassagem - profundidade * reducaoPassagemPorDistancia
        );

        // A nova passagem sempre fica longe da anterior:
        // quem ficar parado no mesmo lugar vai bater.
        Vector2 passagem = EscolherNovaPassagem(passagemAnterior);

        int quantidade = Mathf.Clamp(
            obstaculosPorLinha + Mathf.FloorToInt(
                profundidade * aumentoDificuldadePorDistancia
            ),
            obstaculosPorLinha,
            obstaculosMaximosPorLinha
        );

        List<Vector2> posicoes = new();
        int criados = 0;

        // 1) Bloqueia o lugar da passagem anterior (onde o jogador provavelmente esta).
        {
            float lado = Random.Range(
                Mathf.Lerp(tamanhoMinimo, tamanhoMaximo, 0.6f),
                tamanhoMaximo
            );

            if (TentarCriar(
                    passagemAnterior,
                    new Vector3(lado, lado, lado),
                    Random.Range(0f, 90f),
                    passagem, raioPassagem, posicoes, false))
            {
                criados++;
            }
        }

        // 2) Coloca um obstaculo na posicao atual do jogador.
        if (Random.value < chanceMirarJogador)
        {
            Vector2 posJogador = LimitarNaArea(
                new Vector2(player.position.x, player.position.z)
            );

            float lado = Random.Range(
                Mathf.Lerp(tamanhoMinimo, tamanhoMaximo, 0.5f),
                tamanhoMaximo
            );

            if (TentarCriar(
                    posJogador,
                    new Vector3(lado, lado, lado),
                    Random.Range(0f, 90f),
                    passagem, raioPassagem, posicoes, false))
            {
                criados++;
            }
        }

        // 3) Preenche o resto aleatoriamente, sempre respeitando a passagem.
        int tentativas = 0;
        int maxTentativas = quantidade * 12;

        while (criados < quantidade && tentativas < maxTentativas)
        {
            tentativas++;

            Vector2 posicao = new Vector2(
                Random.Range(-tamanhoArea, tamanhoArea),
                Random.Range(-tamanhoArea, tamanhoArea)
            );

            Vector3 escala = GerarEscala(out float rotacaoY);

            if (TentarCriar(
                    posicao, escala, rotacaoY,
                    passagem, raioPassagem, posicoes, true))
            {
                criados++;
            }
        }

        passagemAnterior = passagem;
        proximaLinhaY -= distanciaEntreLinhas;
    }

    private Vector2 EscolherNovaPassagem(Vector2 anterior)
    {
        float min = distanciaMinimaMudancaPassagem;
        float max = Mathf.Max(min + 0.5f, distanciaMaximaMudancaPassagem);

        for (int i = 0; i < 30; i++)
        {
            Vector2 candidata = new Vector2(
                Random.Range(-tamanhoArea, tamanhoArea),
                Random.Range(-tamanhoArea, tamanhoArea)
            );

            float d = Vector2.Distance(candidata, anterior);

            if (d >= min && d <= max)
                return candidata;
        }

        // Plano B: empurra a passagem pro lado oposto do centro.
        Vector2 direcao = -anterior;

        if (direcao.sqrMagnitude < 0.01f)
            direcao = Random.insideUnitCircle;

        direcao.Normalize();

        return LimitarNaArea(anterior + direcao * min);
    }

    private Vector3 GerarEscala(out float rotacaoY)
    {
        if (Random.value < chanceBarra)
        {
            // Barra comprida e fina
            float comprimento = Random.Range(tamanhoMaximo, comprimentoMaximoBarra);
            float espessura = tamanhoMinimo;
            float altura = Random.Range(tamanhoMinimo, tamanhoMaximo);

            rotacaoY = Random.Range(0f, 180f);
            return new Vector3(comprimento, altura, espessura);
        }

        float tamanho = Random.Range(tamanhoMinimo, tamanhoMaximo);

        rotacaoY = Random.value < chanceGirar
            ? Random.Range(0f, 90f)
            : 0f;

        return new Vector3(tamanho, tamanho, tamanho);
    }

    private bool TentarCriar(
        Vector2 posicao,
        Vector3 escala,
        float rotacaoY,
        Vector2 passagem,
        float raioPassagem,
        List<Vector2> posicoes,
        bool checarProximidade)
    {
        // Raio que cobre o obstaculo em qualquer rotacao.
        float raio = 0.5f * Mathf.Sqrt(escala.x * escala.x + escala.z * escala.z);

        // Garante que a passagem fica realmente livre.
        if (Vector2.Distance(posicao, passagem) < raioPassagem + raio)
            return false;

        if (checarProximidade)
        {
            foreach (Vector2 existente in posicoes)
            {
                if (Vector2.Distance(posicao, existente) <
                    distanciaMinimaEntreObstaculos)
                {
                    return false;
                }
            }
        }

        GameObject obstaculo = Instantiate(
            obstaclePrefab,
            new Vector3(posicao.x, proximaLinhaY, posicao.y),
            Quaternion.Euler(0f, rotacaoY, 0f)
        );

        obstaculo.transform.localScale = escala;

        obstaculosAtivos.Add(obstaculo);
        posicoes.Add(posicao);

        return true;
    }

    private Vector2 LimitarNaArea(Vector2 p)
    {
        return new Vector2(
            Mathf.Clamp(p.x, -tamanhoArea, tamanhoArea),
            Mathf.Clamp(p.y, -tamanhoArea, tamanhoArea)
        );
    }

    private void LimparObstaculos()
    {
        for (int i = obstaculosAtivos.Count - 1; i >= 0; i--)
        {
            GameObject obstaculo = obstaculosAtivos[i];

            if (obstaculo == null)
            {
                obstaculosAtivos.RemoveAt(i);
                continue;
            }

            if (obstaculo.transform.position.y >
                player.position.y + distanciaParaDestruir)
            {
                Destroy(obstaculo);
                obstaculosAtivos.RemoveAt(i);
            }
        }
    }
}