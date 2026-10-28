
using UnityEngine;

public class TutorialAnimation : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float altura = 15f;
    [SerializeField] private float velocidade = 2f;

    [Header("Rotação")]
    [SerializeField] private float rotacaoMaxima = 5f;
    [SerializeField] private float velocidadeRotacao = 1.5f;

    private RectTransform rectTransform;
    private Vector2 posicaoInicial;
    private Quaternion rotacaoInicial;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        posicaoInicial = rectTransform.anchoredPosition;
        rotacaoInicial = rectTransform.localRotation;
    }

    private void Update()
    {
        float tempo = Time.unscaledTime;

        float movimentoY = Mathf.Sin(tempo * velocidade) * altura;
        float rotacaoZ = Mathf.Sin(tempo * velocidadeRotacao) * rotacaoMaxima;

        rectTransform.anchoredPosition =
            posicaoInicial + new Vector2(0f, movimentoY);

        rectTransform.localRotation =
            rotacaoInicial * Quaternion.Euler(0f, 0f, rotacaoZ);
    }
}
