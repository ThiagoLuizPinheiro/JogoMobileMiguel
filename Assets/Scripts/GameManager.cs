using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Referências")]
    public GameObject painelMenu;
    public TMP_Text textoContagem;
    public PlayerController player;

    [Header("Animação da contagem")]
    public float duracaoPulso = 0.4f;
    public float escalaMaxima = 1.6f;

    void Start()
    {
        if (textoContagem != null)
            textoContagem.gameObject.SetActive(false);

        if (GameButtons.iniciarAutomaticamente)
        {
            GameButtons.iniciarAutomaticamente = false;

            painelMenu.SetActive(false);

            StartCoroutine(Contagem());
        }
    }

    public void IniciarJogo()
    {
        painelMenu.SetActive(false);

        player.ResetarJogo();

        StartCoroutine(Contagem());
    }
    IEnumerator Contagem()
    {
        textoContagem.gameObject.SetActive(true);

        yield return StartCoroutine(AnimarTexto("3"));
        yield return StartCoroutine(AnimarTexto("2"));
        yield return StartCoroutine(AnimarTexto("1"));
        yield return StartCoroutine(AnimarTexto("GO!"));

        textoContagem.gameObject.SetActive(false);

        player.IniciarMovimento();
    }

    IEnumerator AnimarTexto(string valor)
    {
        textoContagem.text = valor;

        RectTransform rt = textoContagem.rectTransform;
        Color corOriginal = textoContagem.color;

        float tempo = 0f;

        // Fase 1: cresce rápido e some a opacidade (efeito de "pulso")
        while (tempo < duracaoPulso)
        {
            tempo += Time.unscaledDeltaTime;
            float t = tempo / duracaoPulso;

            // Curva: começa em 0.5, estoura até escalaMaxima, some no final
            float escala = Mathf.Lerp(0.5f, escalaMaxima, t);
            float alpha = 1f - t;

            rt.localScale = Vector3.one * escala;
            textoContagem.color = new Color(corOriginal.r, corOriginal.g, corOriginal.b, alpha);

            yield return null;
        }

        // Reseta pro próximo número
        rt.localScale = Vector3.one;
        textoContagem.color = new Color(corOriginal.r, corOriginal.g, corOriginal.b, 1f);
    }
}