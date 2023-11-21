using System.Collections;
using UnityEngine;

public class Objetivo : MonoBehaviour {
  [Header("Graphics")]
  [SerializeField] private Sprite objetivo;
  [SerializeField] private Sprite hombreOlla;
  [SerializeField] private Sprite hombre1Hit;
  [SerializeField] private Sprite hombreOut;
  [SerializeField] private Sprite hombre2Hit;

  [Header("GameManager")]
  [SerializeField] private JuegoController juegoController;

  // The offset of the sprite to hide it.
  private Vector2 posInicial = new Vector2(0f, -2.56f);
  private Vector2 posFinal = Vector2.zero;
  // How long it takes to show a mole.
  private float mostrarDuracion = 0.5f;
  private float duracion = 1f;

  private SpriteRenderer spriteRenderer;
  private Animator animator;
  private BoxCollider2D boxCollider2D;
  private Vector2 boxOffset;
  private Vector2 boxSize;
  private Vector2 boxOffsetHidden;
  private Vector2 boxSizeHidden;

  // Mole Parameters 
  private bool golpeable = true;
  public enum TipoHombre { Normal, Olla, Bomba };
  private TipoHombre tipoHombre;
  private float ollasRate = 0.25f;
  private float bombasRate;
  private int vidas;
  private int hombreIndex;

  private IEnumerator ShowHide(Vector2 inicio, Vector2 final) {
    // Make sure we start at the start.
    transform.localPosition = inicio;

    // Show the mole.
    float tiempo = 0f;
    while (tiempo < mostrarDuracion) {
      transform.localPosition = Vector2.Lerp(inicio, final, tiempo / mostrarDuracion);
      boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, tiempo / mostrarDuracion);
      boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, tiempo / mostrarDuracion);
      // Update at max framerate.
      tiempo += Time.deltaTime;
      yield return null;
    }

    // Make sure we're exactly at the end.
    transform.localPosition = final;
    boxCollider2D.offset = boxOffset;
    boxCollider2D.size = boxSize;

    // Wait for duration to pass.
    yield return new WaitForSeconds(duracion); //corrutina. Esperamos el tiempo que sea la variable duracion

    // Hide the mole.
    tiempo = 0f;
    while (tiempo < mostrarDuracion) {
      transform.localPosition = Vector2.Lerp(final, inicio, tiempo / mostrarDuracion);
      boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, tiempo / mostrarDuracion);
      boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, tiempo / mostrarDuracion);
      // Update at max framerate.
      tiempo += Time.deltaTime;
      yield return null;
    }
    // Make sure we're exactly back at the start position.
    transform.localPosition = inicio;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;

    // If we got to the end and it's still hittable then we missed it.
    if (golpeable) {
      golpeable = false;
      // We only give time penalty if it isn't a bomb.
      juegoController.Missed(hombreIndex, tipoHombre != TipoHombre.Bomba);
    }
  }

  public void Hide() {
    // Set the appropriate mole parameters to hide it.
    transform.localPosition = posInicial;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;
  }

  private IEnumerator QuickHide() {
    yield return new WaitForSeconds(0.25f);
    // Whilst we were waiting we may have spawned again here, so just
    // check that hasn't happened before hiding it. This will stop it
    // flickering in that case.
    if (!golpeable) {
      Hide();
    }
  }

  private void OnMouseDown() {
    if (golpeable) {
      switch (tipoHombre) {
        case TipoHombre.Normal:
          spriteRenderer.sprite = hombreOut;
          juegoController.AddScore(hombreIndex);
          // Stop the animation
          StopAllCoroutines();
          StartCoroutine(QuickHide());
          // Turn off hittable so that we can't keep tapping for score.
          golpeable = false;
          break;
        case TipoHombre.Olla:
          // If lives == 2 reduce, and change sprite.
          if (vidas == 2) {
            spriteRenderer.sprite = hombre1Hit;
            vidas--;
          } else {
            spriteRenderer.sprite = hombre2Hit;
            juegoController.AddScore(hombreIndex);
            // Stop the animation
            StopAllCoroutines();
            StartCoroutine(QuickHide());
            // Turn off hittable so that we can't keep tapping for score.
            golpeable = false;
          }
          break;
        case TipoHombre.Bomba:
          // Game over, 1 for bomb.
          juegoController.GameOver(1);
          break;
        default:
          break;
      }
    }
  }

  private void CreateNext() {
    float random = Random.Range(0f, 1f);
    if (random < bombasRate) {
      // Make a bomb.
      tipoHombre = TipoHombre.Bomba;
      // The animator handles setting the sprite.
      animator.enabled = true;
    } else {
      animator.enabled = false;
      random = Random.Range(0f, 1f);
      if (random < ollasRate) {
        // Create a hard one.
        tipoHombre = TipoHombre.Olla;
        spriteRenderer.sprite = hombreOlla;
        vidas = 2;
      } else {
        // Create a standard one.
        tipoHombre = TipoHombre.Normal;
        spriteRenderer.sprite = objetivo;
        vidas = 1;
      }
    }
    // Mark as hittable so we can register an onclick event.
    golpeable = true;
  }

  // As the level progresses the game gets harder.
  private void SetLevel(int level) {
    // As level increases increse the bomb rate to 0.25 at level 10.
    bombasRate = Mathf.Min(level * 0.025f, 0.25f); //incremento de la dificultad

    // Increase the amounts of HardHats until 100% at level 40.
    ollasRate = Mathf.Min(level * 0.025f, 1f); //incremento de la dificultad

    // Duration bounds get quicker as we progress. No cap on insanity.
    float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
    float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
    duracion = Random.Range(durationMin, durationMax);
  }

  private void Awake() {
    // Get references to the components we'll need.
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
    boxCollider2D = GetComponent<BoxCollider2D>();
    // Work out collider values.
    boxOffset = boxCollider2D.offset;
    boxSize = boxCollider2D.size;
    boxOffsetHidden = new Vector2(boxOffset.x, -posInicial.y / 2f);
    boxSizeHidden = new Vector2(boxSize.x, 0f);
  }

  public void Activate(int level) {
    SetLevel(level);
    CreateNext();
    StartCoroutine(ShowHide(posInicial, posFinal));
  }

  // Used by the game manager to uniquely identify moles. 
  public void SetIndex(int index) {
    hombreIndex = index;
  }

  // Used to freeze the game on finish.
  public void StopGame() {
    golpeable = false;
    StopAllCoroutines();
  }
}
