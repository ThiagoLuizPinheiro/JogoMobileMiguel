using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidadeMovimento = 15f;
    [SerializeField] private float suavidade = 15f;

    [Header("Queda")]
    [SerializeField] private float velocidadeQueda = 5f;
    [SerializeField] private float aceleracaoQueda = 0.5f;
    [SerializeField] private float velocidadeQuedaMaxima = 30f;

    [Header("Início")]
    [SerializeField] private Vector3 posicaoInicial;

    [Header("Morte")]
    [SerializeField] private GameObject painelMorte;

    private Rigidbody rb;
    private Camera cam;

    private Vector3 alvo;
    private bool jogoIniciado;
    private bool morto;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    private void Start()
    {
        transform.position = posicaoInicial;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;

        alvo = transform.position;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!jogoIniciado || morto)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            MoverComMouse();
        }
    }

    private void FixedUpdate()
    {
        if (!jogoIniciado || morto)
            return;

        velocidadeQueda += aceleracaoQueda * Time.fixedDeltaTime;

        velocidadeQueda = Mathf.Min(
            velocidadeQueda,
            velocidadeQuedaMaxima
        );

        Vector3 posicao = rb.position;

        posicao.x = Mathf.Lerp(
            posicao.x,
            alvo.x,
            suavidade * Time.fixedDeltaTime
        );

        posicao.z = Mathf.Lerp(
            posicao.z,
            alvo.z,
            suavidade * Time.fixedDeltaTime
        );

        rb.MovePosition(posicao);

        rb.linearVelocity = new Vector3(
            0f,
            -velocidadeQueda,
            0f
        );
    }

    private void MoverComMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(mousePosition);

        Plane planoMovimento = new Plane(
            Vector3.up,
            transform.position
        );

        if (planoMovimento.Raycast(ray, out float distancia))
        {
            Vector3 ponto = ray.GetPoint(distancia);

            alvo = new Vector3(
                ponto.x,
                transform.position.y,
                ponto.z
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (morto || !jogoIniciado)
            return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        morto = true;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (painelMorte != null)
            painelMorte.SetActive(true);

        Time.timeScale = 0f;
    }

    public void IniciarMovimento()
    {
        jogoIniciado = true;
        morto = false;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        alvo = transform.position;
    }

    public void ResetarJogo()
    {
        morto = false;
        jogoIniciado = false;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        transform.position = posicaoInicial;

        alvo = posicaoInicial;

        velocidadeQueda = 5f;

        if (painelMorte != null)
            painelMorte.SetActive(false);
    }
}