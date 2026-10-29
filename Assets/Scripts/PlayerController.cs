using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float suavidade = 8f;
    [SerializeField] private float velocidadeMaxima = 12f;

    [Header("Controle por Inclinacao")]
    [Tooltip("Se ligado e o aparelho tiver sensor, usa inclinacao. Senao usa toque/mouse.")]
    [SerializeField] private bool usarInclinacao = true;
    [Tooltip("Quanto maior, menos precisa inclinar o celular pra chegar na velocidade maxima.")]
    [SerializeField] private float sensibilidade = 2.5f;
    [Tooltip("Ignora inclinacoes pequenas pra bola nao ficar tremendo.")]
    [SerializeField] private float zonaMorta = 0.03f;
    [Tooltip("Quanto maior, mais rapida a resposta (menor suavizacao).")]
    [SerializeField] private float suavizacaoInclinacao = 10f;
    [SerializeField] private bool inverterX = false;
    [SerializeField] private bool inverterY = false;

    [Header("Queda")]
    [SerializeField] private float velocidadeQuedaInicial = 5f;
    [SerializeField] private float aceleracaoQueda = 0.25f;
    [SerializeField] private float velocidadeQuedaMaxima = 15f;

    [Header("Inicio")]
    [SerializeField] private Vector3 posicaoInicial;

    [Header("Morte")]
    [SerializeField] private GameObject painelMorte;

    [Header("Efeito de Ar")]
    [SerializeField] private ParticleSystem efeitoAr;
    [SerializeField] private float emissaoMinima = 20f;
    [SerializeField] private float emissaoMaxima = 80f;
    [SerializeField] private float velocidadeParticulaMinima = 10f;
    [SerializeField] private float velocidadeParticulaMaxima = 25f;

    [Header("Rastro (opcional)")]
    [SerializeField] private TrailRenderer rastro;

    [Header("Dica de Movimento")]
    [Tooltip("Objeto de UI com o texto 'Incline o celular para se mover'. Some quando o jogador se mexe.")]
    [SerializeField] private GameObject dicaMovimento;
    [Tooltip("Quanto o jogador precisa inclinar pra dica sumir (0 a 1).")]
    [SerializeField] private float limiteDica = 0.2f;

    private bool dicaVisivel;

    private Rigidbody rb;
    private Camera cam;
    private Vector3 alvo;

    private float velocidadeQueda;
    private bool jogoIniciado;
    private bool morto;

    // Inclinacao
    private bool temSensor;
    private Vector2 inclinacaoNeutra;
    private Vector2 inclinacaoSuave;

    private bool ModoInclinacao => usarInclinacao && temSensor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        AtivarSensores();
    }

    private void OnDestroy()
    {
        DesativarSensores();
    }

    private void Start()
    {
        rb.position = posicaoInicial;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        alvo = posicaoInicial;
        velocidadeQueda = velocidadeQuedaInicial;

        PararEfeitos(true);
        EsconderDica();
    }

    // ---------------- SENSORES ----------------

    private void AtivarSensores()
    {
        if (GravitySensor.current != null)
            InputSystem.EnableDevice(GravitySensor.current);

        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);

        temSensor = GravitySensor.current != null || Accelerometer.current != null;
    }

    private void DesativarSensores()
    {
        if (GravitySensor.current != null && GravitySensor.current.enabled)
            InputSystem.DisableDevice(GravitySensor.current);

        if (Accelerometer.current != null && Accelerometer.current.enabled)
            InputSystem.DisableDevice(Accelerometer.current);
    }

    // Retorna a direcao da gravidade ja normalizada (-1 a 1 em cada eixo),
    // assim nao depende da unidade que o aparelho usa.
    private bool LerGravidade(out Vector3 g)
    {
        if (GravitySensor.current != null && GravitySensor.current.enabled)
        {
            g = GravitySensor.current.gravity.ReadValue();
        }
        else if (Accelerometer.current != null && Accelerometer.current.enabled)
        {
            g = Accelerometer.current.acceleration.ReadValue();
        }
        else
        {
            g = Vector3.zero;
            return false;
        }

        if (g.sqrMagnitude < 0.0001f)
            return false;

        g.Normalize();
        return true;
    }

    // Define a posicao atual do celular como "reto" (neutro).
    public void CalibrarInclinacao()
    {
        if (LerGravidade(out Vector3 g))
            inclinacaoNeutra = new Vector2(g.x, g.y);

        inclinacaoSuave = Vector2.zero;
    }

    private Vector2 LerInclinacao()
    {
        if (!LerGravidade(out Vector3 g))
            return Vector2.zero;

        Vector2 delta = new Vector2(g.x, g.y) - inclinacaoNeutra;

        if (inverterX) delta.x = -delta.x;
        if (inverterY) delta.y = -delta.y;

        // zona morta
        if (delta.magnitude < zonaMorta)
            return Vector2.zero;

        return Vector2.ClampMagnitude(delta * sensibilidade, 1f);
    }

    // ---------------- LOOP ----------------

    private void Update()
    {
        if (!jogoIniciado || morto)
            return;

        if (ModoInclinacao)
        {
            Vector2 alvoInclinacao = LerInclinacao();
            float k = 1f - Mathf.Exp(-suavizacaoInclinacao * Time.unscaledDeltaTime);
            inclinacaoSuave = Vector2.Lerp(inclinacaoSuave, alvoInclinacao, k);

            if (dicaVisivel && inclinacaoSuave.magnitude > limiteDica)
                EsconderDica();

            return;
        }

        AtualizarAlvoPorToque();
    }

    private void AtualizarAlvoPorToque()
    {
        if (cam == null)
            return;

        Vector2 posicaoTela;
        bool temInput = false;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            posicaoTela =
                Touchscreen.current.primaryTouch.position.ReadValue();

            temInput = true;
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.isPressed)
        {
            posicaoTela = Mouse.current.position.ReadValue();
            temInput = true;
        }
        else
        {
            posicaoTela = Vector2.zero;
        }

        if (!temInput)
        {
            alvo = rb.position;
            return;
        }

        if (dicaVisivel)
            EsconderDica();

        Ray ray = cam.ScreenPointToRay(posicaoTela);

        Plane plano = new Plane(
            Vector3.up,
            new Vector3(0f, rb.position.y, 0f)
        );

        if (plano.Raycast(ray, out float distancia))
        {
            Vector3 ponto = ray.GetPoint(distancia);

            alvo = new Vector3(
                ponto.x,
                rb.position.y,
                ponto.z
            );
        }
    }

    private void FixedUpdate()
    {
        if (!jogoIniciado || morto)
            return;

        velocidadeQueda = Mathf.MoveTowards(
            velocidadeQueda,
            velocidadeQuedaMaxima,
            aceleracaoQueda * Time.fixedDeltaTime
        );

        Vector3 horizontal;

        if (ModoInclinacao)
        {
            horizontal = new Vector3(
                inclinacaoSuave.x,
                0f,
                inclinacaoSuave.y
            ) * velocidadeMaxima;
        }
        else
        {
            Vector3 direcao = alvo - rb.position;
            direcao.y = 0f;

            horizontal = Vector3.ClampMagnitude(
                direcao * suavidade,
                velocidadeMaxima
            );
        }

        rb.linearVelocity = new Vector3(
            horizontal.x,
            -velocidadeQueda,
            horizontal.z
        );
    }

    private void LateUpdate()
    {
        AtualizarEfeitoAr();
    }

    // ---------------- EFEITOS ----------------

    private void AtualizarEfeitoAr()
    {
        if (efeitoAr == null)
            return;

        if (!jogoIniciado || morto)
            return;

        if (!efeitoAr.isPlaying)
            efeitoAr.Play();

        // 0 = velocidade inicial, 1 = velocidade maxima de queda
        float t = Mathf.InverseLerp(
            velocidadeQuedaInicial,
            velocidadeQuedaMaxima,
            velocidadeQueda
        );

        var emissao = efeitoAr.emission;
        emissao.rateOverTime = Mathf.Lerp(emissaoMinima, emissaoMaxima, t);

        var main = efeitoAr.main;
        main.startSpeed = Mathf.Lerp(
            velocidadeParticulaMinima,
            velocidadeParticulaMaxima,
            t
        );
    }

    private void PararEfeitos(bool limpar)
    {
        if (efeitoAr != null)
        {
            efeitoAr.Stop(
                true,
                limpar
                    ? ParticleSystemStopBehavior.StopEmittingAndClear
                    : ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (rastro != null)
        {
            rastro.emitting = false;

            if (limpar)
                rastro.Clear();
        }
    }

    // ---------------- JOGO ----------------

    private void OnCollisionEnter(Collision collision)
    {
        if (!jogoIniciado || morto)
            return;

        if (collision.gameObject.CompareTag("Obstacle"))
            Morrer();
    }

    private void Morrer()
    {
        morto = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        PararEfeitos(false);
        EsconderDica();

        if (painelMorte != null)
            painelMorte.SetActive(true);

        Time.timeScale = 0f;
    }

    private void MostrarDica()
    {
        dicaVisivel = true;

        if (dicaMovimento != null)
            dicaMovimento.SetActive(true);
    }

    private void EsconderDica()
    {
        dicaVisivel = false;

        if (dicaMovimento != null)
            dicaMovimento.SetActive(false);
    }

    public void IniciarMovimento()
    {
        Time.timeScale = 1f;

        jogoIniciado = true;
        morto = false;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        alvo = rb.position;
        velocidadeQueda = velocidadeQuedaInicial;

        // Posicao em que o jogador esta segurando o celular vira o "reto"
        CalibrarInclinacao();

        MostrarDica();

        if (efeitoAr != null)
            efeitoAr.Play();

        if (rastro != null)
        {
            rastro.Clear();
            rastro.emitting = true;
        }
    }

    public void ResetarJogo()
    {
        Time.timeScale = 1f;

        jogoIniciado = false;
        morto = false;

        rb.isKinematic = false;
        rb.position = posicaoInicial;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        alvo = posicaoInicial;
        velocidadeQueda = velocidadeQuedaInicial;
        inclinacaoSuave = Vector2.zero;

        PararEfeitos(true);
        EsconderDica();

        if (painelMorte != null)
            painelMorte.SetActive(false);
    }
}