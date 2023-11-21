using System.Collections.Generic;
using UnityEngine;

public class JuegoController : MonoBehaviour {
  [SerializeField] private List<Objetivo> objetivos;

  [Header("Graphics")]
  [SerializeField] private GameObject boom;
  [SerializeField] private GameObject tiempo;

    [Header("UI objects")]
  [SerializeField] private GameObject botonPlay;
  [SerializeField] private GameObject juegoUI;

  [SerializeField] private TMPro.TextMeshProUGUI tiempoText;
  [SerializeField] private TMPro.TextMeshProUGUI puntosText;
  
  private float tiempoInicial = 30f;
  private float tiempoRestante;
  private HashSet<Objetivo> objetivosActuales = new HashSet<Objetivo>();
  private int puntuacion;
  private bool playing;

  public void StartGame() {
    botonPlay.SetActive(false);
    tiempo.SetActive(false);
    boom.SetActive(false);
    juegoUI.SetActive(true);
    for (int i = 0; i < objetivos.Count; i++) {
      objetivos[i].Hide();
      objetivos[i].SetIndex(i);
    }
    objetivosActuales.Clear();
    tiempoRestante = tiempoInicial;
    puntuacion = 0;
    puntosText.text = "0";
    playing = true;
  }

  public void GameOver(int type) {
    if (type == 0) {
      tiempo.SetActive(true);
    } else {
        boom.SetActive(true);
    }
    foreach (Objetivo objetivo in objetivos) {
      objetivo.StopGame();
    }
    playing = false;
    botonPlay.SetActive(true);
  }
  void Update() {
    if (playing) {
      tiempoRestante -= Time.deltaTime;
      if (tiempoRestante <= 0) {
        tiempoRestante = 0;
        GameOver(0);
      }
      tiempoText.text = $"{(int)tiempoRestante / 60}:{(int)tiempoRestante % 60:D2}";
      if (objetivosActuales.Count <= (puntuacion / 10)) {
        int index = Random.Range(0, objetivos.Count);
        if (!objetivosActuales.Contains(objetivos[index])) {
          objetivosActuales.Add(objetivos[index]);
          objetivos[index].Activate(puntuacion / 10);
        }
      }
    }
  }

  public void AddScore(int objetivoIndex) {
    puntuacion += 1;
    puntosText.text = $"{puntuacion}";
    tiempoRestante += 1;
    objetivosActuales.Remove(objetivos[objetivoIndex]);
  }

  public void Missed(int objetivoIndex, bool isTrue) {
    if (isTrue) {
      tiempoRestante -= 2;
    }
    objetivosActuales.Remove(objetivos[objetivoIndex]);
  }
}
