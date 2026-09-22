using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject obstaclePrefab;

    [Header("Área do cano")]
    public float tamanhoArea = 4f;

    [Header("Distância de spawn")]
    public float distanciaSpawn = 30f;

    [Header("Quantidade")]
    public int obstaculosPorLinha = 8;

    [Header("Tamanho dos obstáculos")]
    public float tamanhoMinimo = 0.7f;
    public float tamanhoMaximo = 3f;

    [Header("Espaço mínimo para o jogador")]
    public float espacoPassagem = 1.5f;

    [Header("Distância entre linhas")]
    public float distanciaEntreLinhas = 10f;

    [Header("Limpeza")]
    public float distanciaParaDestruir = 30f;

    private float proximaLinhaY;

    void Start()
    {
        if (player == null || obstaclePrefab == null)
            return;

        proximaLinhaY = player.position.y - distanciaSpawn;

        for (int i = 0; i < 5; i++)
        {
            CriarLinha();
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Cria novas linhas abaixo do jogador
        while (player.position.y - proximaLinhaY < distanciaSpawn)
        {
            CriarLinha();
        }

        // Remove obstáculos que ficaram muito acima do jogador
        LimparObstaculos();
    }

    void CriarLinha()
    {
        // Escolhe uma posição que ficará livre
        float passagemX = Random.Range(-tamanhoArea, tamanhoArea);
        float passagemZ = Random.Range(-tamanhoArea, tamanhoArea);

        for (int i = 0; i < obstaculosPorLinha; i++)
        {
            float x = Random.Range(-tamanhoArea, tamanhoArea);
            float z = Random.Range(-tamanhoArea, tamanhoArea);

            // Mantém uma passagem livre
            if (Vector2.Distance(
                new Vector2(x, z),
                new Vector2(passagemX, passagemZ)
            ) < espacoPassagem)
            {
                continue;
            }

            GameObject obstaculo = Instantiate(
                obstaclePrefab,
                new Vector3(x, proximaLinhaY, z),
                Quaternion.identity
            );

            float tamanho = Random.Range(
                tamanhoMinimo,
                tamanhoMaximo
            );

            obstaculo.transform.localScale = new Vector3(
                tamanho,
                tamanho,
                tamanho
            );
        }

        proximaLinhaY -= distanciaEntreLinhas;
    }

    void LimparObstaculos()
    {
        GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstaculo in obstaculos)
        {
            // Se o obstáculo ficou acima do jogador,
            // destrói para liberar memória.
            if (obstaculo.transform.position.y >
                player.position.y + distanciaParaDestruir)
            {
                Destroy(obstaculo);
            }
        }
    }
}