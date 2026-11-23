using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class ControlBart : MonoBehaviour
{
    [SerializeField] private float velocidad = 6.6f;
    [SerializeField] private float salto = 11f;
    [SerializeField] private float limiteCaida = -4.9f;
    [SerializeField] private Collider2D obstaculo1;
    [SerializeField] private Collider2D obstaculo2;
    [SerializeField] private AudioClip sonidoGolpe;
    [SerializeField] private AudioClip sonidoCaida;
    [SerializeField] private AudioClip musicaFondo;
    [SerializeField, Range(0f, 1f)] private float volumenMusica = 0.3f;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite poseQuieto;
    [SerializeField] private Sprite[] pasos;
    [SerializeField] private Sprite poseSalto;
    [SerializeField] private Sprite poseGolpe;

    private readonly ContactPoint2D[] contactos = new ContactPoint2D[8];
    private Rigidbody2D cuerpo;
    private AudioSource audioBart;
    private Vector2 puntoInicio;
    private float direccion;
    private float mostrarGolpeHasta;
    private bool solicitaSalto;
    private bool cayo;
    private bool apoyado;
    private float tiempoCaminata;
    private GUIStyle texto;
    private GUIStyle titulo;
    private GUIStyle firma;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        puntoInicio = cuerpo.position;
        audioBart = gameObject.AddComponent<AudioSource>();
        audioBart.playOnAwake = false;
        audioBart.spatialBlend = 0f;
    }

    private void Start()
    {
        if (musicaFondo == null)
            return;

        AudioSource musica = gameObject.AddComponent<AudioSource>();
        musica.playOnAwake = false;
        musica.clip = musicaFondo;
        musica.loop = true;
        musica.spatialBlend = 0f;
        musica.volume = volumenMusica;
        musica.Play();
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (cayo)
        {
            if (teclado != null && teclado.rKey.wasPressedThisFrame)
                Reiniciar();
            return;
        }

        if (cuerpo.position.y < limiteCaida)
        {
            cayo = true;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.simulated = false;
            audioBart.Stop();
            audioBart.PlayOneShot(sonidoCaida);
            return;
        }

        if (teclado == null)
        {
            direccion = 0f;
            return;
        }

        direccion = (teclado.spaceKey.isPressed ? 1f : 0f)
            - (teclado.upArrowKey.isPressed ? 1f : 0f);
        solicitaSalto |= teclado.pKey.wasPressedThisFrame;
    }

    private void FixedUpdate()
    {
        if (cayo)
            return;

        Vector2 velocidadActual = cuerpo.linearVelocity;
        velocidadActual.x = direccion * velocidad;
        apoyado = EstaApoyado();
        if (solicitaSalto && apoyado)
        {
            velocidadActual.y = salto;
            apoyado = false;
        }

        solicitaSalto = false;
        cuerpo.linearVelocity = velocidadActual;
    }

    private void LateUpdate()
    {
        if (direccion != 0f && !cayo)
            visual.flipX = direccion < 0f;

        if (cayo || Time.time < mostrarGolpeHasta)
        {
            visual.sprite = poseGolpe;
        }
        else if (!apoyado)
        {
            visual.sprite = poseSalto;
        }
        else if (Mathf.Abs(cuerpo.linearVelocity.x) > 0.1f)
        {
            tiempoCaminata += Time.deltaTime;
            int fotograma = (int)(tiempoCaminata * 14f) % pasos.Length;
            visual.sprite = pasos[fotograma];
        }
        else
        {
            tiempoCaminata = 0f;
            visual.sprite = poseQuieto;
        }
    }

    private bool EstaApoyado()
    {
        int cantidad = cuerpo.GetContacts(contactos);
        for (int i = 0; i < cantidad; i++)
        {
            if (contactos[i].normal.y > 0.6f)
                return true;
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D choque)
    {
        if (choque.collider != obstaculo1 && choque.collider != obstaculo2)
            return;

        audioBart.PlayOneShot(sonidoGolpe);
        mostrarGolpeHasta = Time.time + 0.65f;
    }

    private void Reiniciar()
    {
        audioBart.Stop();
        direccion = 0f;
        solicitaSalto = false;
        mostrarGolpeHasta = 0f;
        apoyado = false;
        tiempoCaminata = 0f;
        visual.flipX = false;
        transform.position = new Vector3(puntoInicio.x, puntoInicio.y, transform.position.z);
        cuerpo.simulated = true;
        cuerpo.position = puntoInicio;
        cuerpo.linearVelocity = Vector2.zero;
        cayo = false;
    }

    private void OnGUI()
    {
        if (texto == null)
        {
            texto = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 19 };
            texto.normal.textColor = new Color32(213, 226, 250, 255);
            titulo = new GUIStyle(texto) { fontSize = 36, fontStyle = FontStyle.Bold };
            titulo.normal.textColor = Color.white;
            firma = new GUIStyle(texto) { fontSize = 18, fontStyle = FontStyle.Bold };
            firma.normal.textColor = new Color32(229, 243, 236, 255);
        }

        float escala = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
        Matrix4x4 matrizOriginal = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(
            new Vector3((Screen.width - 960f * escala) * 0.5f, (Screen.height - 540f * escala) * 0.5f),
            Quaternion.identity, new Vector3(escala, escala, 1f));

        if (cayo)
        {
            DibujarFondo(new Rect(0, 0, 960, 540), new Color32(8, 13, 28, 172));
            DibujarFondo(new Rect(285, 194, 390, 152), new Color32(25, 36, 66, 255));
            DibujarFondo(new Rect(285, 194, 390, 4), new Color32(248, 172, 86, 255));
            GUI.Label(new Rect(285, 218, 390, 58), "¡Caíste al abismo!", titulo);
            GUI.Label(new Rect(285, 281, 390, 35), "Presiona R para volver al inicio", texto);
        }
        else if (Time.time < mostrarGolpeHasta)
        {
            GUI.Label(new Rect(406, 26, 148, 32), "¡Golpe!", texto);
        }

        float bordeDerecho = Screen.width / (2f * escala) + 480f;
        float bordeInferior = Screen.height / (2f * escala) + 270f;
        Rect credito = new Rect(bordeDerecho - 150f, bordeInferior - 48f, 132f, 30f);
        DibujarFondo(credito, new Color32(9, 24, 32, 226));
        GUI.Label(credito, "@Brendavarm", firma);

        GUI.matrix = matrizOriginal;
    }

    private static void DibujarFondo(Rect rect, Color color)
    {
        Color colorOriginal = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = colorOriginal;
    }
}
