using System.Collections;
using UnityEngine;

public class Objetivo : MonoBehaviour
{
    [Header("Gráficos")]
    [SerializeField] private Sprite spriteObjetivo;
    [SerializeField] private Sprite spriteObjetivoSecundario;
    [SerializeField] private Sprite spriteObjetivoSecundarioRoto;
    [SerializeField] private Sprite spriteObjetivoGolpeado;
    [SerializeField] private Sprite spriteObjetivoSecundarioGolpeado;

    [Header("GameManager")]
    [SerializeField] private ControladorJuego controladorJuego;

    private Vector2 posicionInicial = new Vector2(0f, -2.56f);
    private Vector2 posicionFinal = Vector2.zero;
    private float duracionMostrar = 0.5f;
    private float duracion = 1f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider2D;
    private Vector2 desplazamientoCaja;
    private Vector2 tamañoCaja;
    private Vector2 desplazamientoCajaOculto;
    private Vector2 tamañoCajaOculto;

    private bool golpeable = true;
    public enum TipoObjetivo { Normal, Olla, Bomba };
    private TipoObjetivo tipoObjetivo;
    private float tasaObjetivoSecundario = 0.25f;
    private float tasaBombas;
    private int vidas;
    private int indiceObjetivo;

    private IEnumerator MostrarOcultar(Vector2 inicio, Vector2 fin)
    {
        transform.localPosition = inicio;

        float tiempo = 0f;
        while (tiempo < duracionMostrar)
        {
            transform.localPosition = Vector2.Lerp(inicio, fin, tiempo / duracionMostrar);
            boxCollider2D.offset = Vector2.Lerp(desplazamientoCajaOculto, desplazamientoCaja, tiempo / duracionMostrar);
            boxCollider2D.size = Vector2.Lerp(tamañoCajaOculto, tamañoCaja, tiempo / duracionMostrar);
            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = fin;
        boxCollider2D.offset = desplazamientoCaja;
        boxCollider2D.size = tamañoCaja;

        yield return new WaitForSeconds(duracion);

        tiempo = 0f;
        while (tiempo < duracionMostrar)
        {
            transform.localPosition = Vector2.Lerp(fin, inicio, tiempo / duracionMostrar);
            boxCollider2D.offset = Vector2.Lerp(desplazamientoCaja, desplazamientoCajaOculto, tiempo / duracionMostrar);
            boxCollider2D.size = Vector2.Lerp(tamañoCaja, tamañoCajaOculto, tiempo / duracionMostrar);
            tiempo += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = inicio;
        boxCollider2D.offset = desplazamientoCajaOculto;
        boxCollider2D.size = tamañoCajaOculto;

        if (golpeable)
        {
            golpeable = false;
            controladorJuego.Fallado(indiceObjetivo, tipoObjetivo != TipoObjetivo.Bomba);
        }
    }

    public void Ocultar()
    {
        transform.localPosition = posicionInicial;
        boxCollider2D.offset = desplazamientoCajaOculto;
        boxCollider2D.size = tamañoCajaOculto;
    }

    private IEnumerator OcultarRapido()
    {
        yield return new WaitForSeconds(0.25f);
        if (!golpeable)
        {
            Ocultar();
        }
    }

    private void OnMouseDown()
    {
        if (golpeable)
        {
            switch (tipoObjetivo)
            {
                case TipoObjetivo.Normal:
                    spriteRenderer.sprite = spriteObjetivoGolpeado;
                    controladorJuego.SumarPuntuacion(indiceObjetivo);
                    StopAllCoroutines();
                    StartCoroutine(OcultarRapido());
                    golpeable = false;
                    break;
                case TipoObjetivo.Olla:
                    if (vidas == 2)
                    {
                        spriteRenderer.sprite = spriteObjetivoSecundarioRoto;
                        vidas--;
                    }
                    else
                    {
                        spriteRenderer.sprite = spriteObjetivoSecundarioGolpeado;
                        controladorJuego.SumarPuntuacion(indiceObjetivo);
                        StopAllCoroutines();
                        StartCoroutine(OcultarRapido());
                        golpeable = false;
                    }
                    break;
                case TipoObjetivo.Bomba:
                    controladorJuego.JuegoTerminado(1);
                    break;
            }
        }
    }

    private void CrearSiguiente()
    {
        float aleatorio = Random.Range(0f, 1f);
        if (aleatorio < tasaBombas)
        {
            tipoObjetivo = TipoObjetivo.Bomba;
            animator.enabled = true;
        }
        else
        {
            animator.enabled = false;
            aleatorio = Random.Range(0f, 1f);
            if (aleatorio < tasaObjetivoSecundario)
            {
                tipoObjetivo = TipoObjetivo.Olla;
                spriteRenderer.sprite = spriteObjetivoSecundario;
                vidas = 2;
            }
            else
            {
                tipoObjetivo = TipoObjetivo.Normal;
                spriteRenderer.sprite = spriteObjetivo;
                vidas = 1;
            }
        }
        golpeable = true;
    }

    private void EstablecerNivel(int nivel)
    {
        tasaBombas = Mathf.Min(nivel * 0.025f, 0.25f); // Ajuste de la tasa de bombas
        tasaObjetivoSecundario = Mathf.Min(nivel * 0.025f, 1f); // Ajuste de la tasa del segundo objetivo
        float duracionMin = Mathf.Clamp(1 - nivel * 0.1f, 0.01f, 1f);
        float duracionMax = Mathf.Clamp(2 - nivel * 0.1f, 0.01f, 2f);
        duracion = Random.Range(duracionMin, duracionMax);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        desplazamientoCaja = boxCollider2D.offset;
        tamañoCaja = boxCollider2D.size;
        desplazamientoCajaOculto = new Vector2(desplazamientoCaja.x, -posicionInicial.y / 2f);
        tamañoCajaOculto = new Vector2(tamañoCaja.x, 0f);
    }

    public void Activar(int nivel)
    {
        EstablecerNivel(nivel);
        CrearSiguiente();
        StartCoroutine(MostrarOcultar(posicionInicial, posicionFinal));
    }

    public void SetIndice(int indice)
    {
        indiceObjetivo = indice;
    }

    public void DetenerJuego()
    {
        golpeable = false;
        StopAllCoroutines();
    }
}
