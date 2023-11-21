using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JuegoController : MonoBehaviour
{
    [SerializeField] private List<Objetivo> objetivos;
    [SerializeField] private GameObject efectoExplosion;
    [SerializeField] private GameObject objetoTiempo;
    [SerializeField] private GameObject botonJugar;
    [SerializeField] private GameObject interfazJuego;
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoPuntos;
    private float tiempoInicial = 30f;
    private float tiempoRestante;
    private HashSet<Objetivo> objetivosActuales = new HashSet<Objetivo>();
    private int puntuacion;
    private bool jugando;

    private void Start()
    {
        tiempoRestante = tiempoInicial;
        puntuacion = 0;
        ActualizarInterfazPuntuacion();
        jugando = false;
        interfazJuego.SetActive(false);
    }
    public void IniciarJuego()
    {
        botonJugar.SetActive(false);
        objetoTiempo.SetActive(true);
        efectoExplosion.SetActive(false);
        interfazJuego.SetActive(true);

        foreach (var objetivo in objetivos)
        {
            objetivo.Ocultar();
            objetivo.SetIndice(objetivos.IndexOf(objetivo));
        }

        objetivosActuales.Clear();
        tiempoRestante = tiempoInicial;
        puntuacion = 0;
        ActualizarInterfazPuntuacion();
        jugando = true;
    }
    public void JuegoTerminado(int tipo)
    {
        if (tipo == 0)
        {
            objetoTiempo.SetActive(false);
        }
        else
        {
            efectoExplosion.SetActive(true);
        }

        foreach (var objetivo in objetivos)
        {
            objetivo.DetenerJuego();
        }

        jugando = false;
        botonJugar.SetActive(true);
    }
    private void Update()
    {
        if (jugando)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                JuegoTerminado(0);
            }

            ActualizarInterfazTiempo();

            if (objetivosActuales.Count <= (puntuacion / 10))
            {
                int indice = Random.Range(0, objetivos.Count);

                if (!objetivosActuales.Contains(objetivos[indice]))
                {
                    objetivosActuales.Add(objetivos[indice]);
                    objetivos[indice].Activar(puntuacion / 10);
                }
            }
        }
    }
    public void SumarPuntuacion(int indiceObjetivo)
    {
        puntuacion++;
        ActualizarInterfazPuntuacion();
        tiempoRestante++;
        objetivosActuales.Remove(objetivos[indiceObjetivo]);
    }
    public void Fallado(int indiceObjetivo, bool esVerdadero)
    {
        if (esVerdadero)
        {
            tiempoRestante -= 2;
        }

        objetivosActuales.Remove(objetivos[indiceObjetivo]);
    }
    private void ActualizarInterfazTiempo()
    {
        textoTiempo.text = $"{(int)tiempoRestante / 60}:{(int)tiempoRestante % 60:D2}";
    }
    private void ActualizarInterfazPuntuacion()
    {
        textoPuntos.text = $"{puntuacion}";
    }
}
