using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeMovimento = 8f;

    [Header("Queda")]
    public float velocidadeQueda = 5f;
    public float aceleracaoQueda = 0.5f;
    public float velocidadeQuedaMaxima = 30f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Aumenta a velocidade da queda
        velocidadeQueda += aceleracaoQueda * Time.fixedDeltaTime;
        velocidadeQueda = Mathf.Min(velocidadeQueda, velocidadeQuedaMaxima);

        Vector3 movimento = Vector3.zero;

        // WASD
        if (Keyboard.current.wKey.isPressed)
            movimento += Vector3.forward;

        if (Keyboard.current.sKey.isPressed)
            movimento += Vector3.back;

        if (Keyboard.current.aKey.isPressed)
            movimento += Vector3.left;

        if (Keyboard.current.dKey.isPressed)
            movimento += Vector3.right;

        if (movimento.magnitude > 1f)
            movimento.Normalize();

        // Velocidade horizontal
        Vector3 velocidadeHorizontal = movimento * velocidadeMovimento;

        // Mantém a queda
        velocidadeHorizontal.y = -velocidadeQueda;

        rb.linearVelocity = velocidadeHorizontal;
    }
}