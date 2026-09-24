using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Coloque este script no próprio botão de Start (junto com o Button)
public class BotaoStart : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float escalaAperto = 0.85f;
    public float velocidadeAnimacao = 12f;

    private Vector3 escalaOriginal;
    private Coroutine animacaoAtual;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimarPara(escalaOriginal * escalaAperto);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimarPara(escalaOriginal);
    }

    void AnimarPara(Vector3 alvo)
    {
        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);

        animacaoAtual = StartCoroutine(Animar(alvo));
    }

    IEnumerator Animar(Vector3 alvo)
    {
        while (Vector3.Distance(transform.localScale, alvo) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                alvo,
                velocidadeAnimacao * Time.unscaledDeltaTime
            );
            yield return null;
        }

        transform.localScale = alvo;
    }
}