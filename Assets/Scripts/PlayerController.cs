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

    [Header("Morte")]
    public GameObject painelMorte; // arraste o painel de UI de morte aqui no Inspector

    private Rigidbody rb;
    private bool morto = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (morto) return; // trava tudo se já morreu

        // Aumenta a velocidade da queda
        velocidadeQueda += aceleracaoQueda * Time.fixedDeltaTime;
        velocidadeQueda = Mathf.Min(velocidadeQueda, velocidadeQuedaMaxima);

        Vector3 movimento = Vector3.zero;

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

        Vector3 velocidadeHorizontal = movimento * velocidadeMovimento;
        velocidadeHorizontal.y = -velocidadeQueda;

        rb.linearVelocity = velocidadeHorizontal;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (morto) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Morrer();
        }
    }

    void Morrer()
    {
        morto = true;

        // Zera a velocidade pra parar de vez (inclusive a queda)
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // opcional: trava física totalmente

        if (painelMorte != null)
        {
            painelMorte.SetActive(true);
        }

        Time.timeScale = 0f; // opcional: pausa o jogo inteiro (remova se não quiser)
    }

    // Chame isso no botão de "Reiniciar" da UI
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}