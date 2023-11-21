using System.Collections.Generic;
using UnityEngine;

public class JuegoController : MonoBehaviour {
  [SerializeField] private List<Objetivo> objetivos;

  [Header("UI objects")]
  [SerializeField] private GameObject botonPlay;
  [SerializeField] private GameObject toposUI;
  [SerializeField] private GameObject outOfTimeText;
  [SerializeField] private GameObject bombaText;
  [SerializeField] private TMPro.TextMeshProUGUI tiempoText;
  [SerializeField] private TMPro.TextMeshProUGUI puntosText;

  // Hardcoded variables you may want to tune.
  private float tiempoInicial = 30f;

  // Global variables
  private float tiempoRestante;
  private HashSet<Objetivo> hombresActuales = new HashSet<Objetivo>();
  private int puntuacion;
  private bool playing;

  // This is public so the play button can see it.
  public void StartGame() {
    // Hide/show the UI elements we don't/do want to see.
    botonPlay.SetActive(false);
    outOfTimeText.SetActive(false);
    bombaText.SetActive(false);
    toposUI.SetActive(true);
    // Hide all the visible moles.
    for (int i = 0; i < objetivos.Count; i++) {
      objetivos[i].Hide();
      objetivos[i].SetIndex(i);
    }
    // Remove any old game state.
    hombresActuales.Clear();
    // Start with 30 seconds.
    tiempoRestante = tiempoInicial;
    puntuacion = 0;
    puntosText.text = "0";
    playing = true;
  }

  public void GameOver(int type) {
    // Show the message.
    if (type == 0) {
      outOfTimeText.SetActive(true);
    } else {
      bombaText.SetActive(true);
    }
    // Hide all moles.
    foreach (Objetivo topo in objetivos) {
      topo.StopGame();
    }
    // Stop the game and show the start UI.
    playing = false;
    botonPlay.SetActive(true);
  }

  // Update is called once per frame
  void Update() {
    if (playing) {
      // Update time.
      tiempoRestante -= Time.deltaTime;
      if (tiempoRestante <= 0) {
        tiempoRestante = 0;
        GameOver(0);
      }
      tiempoText.text = $"{(int)tiempoRestante / 60}:{(int)tiempoRestante % 60:D2}";
      // Check if we need to start any more moles.
      if (hombresActuales.Count <= (puntuacion / 10)) {
        // Choose a random mole.
        int index = Random.Range(0, objetivos.Count);
        // Doesn't matter if it's already doing something, we'll just try again next frame.
        if (!hombresActuales.Contains(objetivos[index])) {
          hombresActuales.Add(objetivos[index]);
          objetivos[index].Activate(puntuacion / 10);
        }
      }
    }
  }

  public void AddScore(int indexHombre) {
    // Add and update score.
    puntuacion += 1;
    puntosText.text = $"{puntuacion}";
    // Increase time by a little bit.
    tiempoRestante += 1;
    // Remove from active moles.
    hombresActuales.Remove(objetivos[indexHombre]);
  }

  public void Missed(int indexHombre, bool isHombre) {
    if (isHombre) {
      // Decrease time by a little bit.
      tiempoRestante -= 2;
    }
    // Remove from active moles.
    hombresActuales.Remove(objetivos[indexHombre]);
  }
}
