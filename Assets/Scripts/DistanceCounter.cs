using UnityEngine;
using TMPro;

public class DistanceCounter : MonoBehaviour
{
    public Transform player;
    public TMP_Text textoDistancia;

    private float alturaInicial;

    void Start()
    {
        if (player != null)
        {
            alturaInicial = player.position.y;
        }
    }

    void Update()
    {
        if (player == null || textoDistancia == null)
            return;

        float distancia = alturaInicial - player.position.y;

        // Não deixa ficar negativo
        distancia = Mathf.Max(0, distancia);

        textoDistancia.text = Mathf.FloorToInt(distancia) + " m";
    }
}