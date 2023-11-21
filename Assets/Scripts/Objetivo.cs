using System.Collections;
using UnityEngine;

public class Objetivo : MonoBehaviour {
  [Header("Graphics")]
  [SerializeField] private Sprite objetivo;
  [SerializeField] private Sprite objetivo2;
  [SerializeField] private Sprite objetivo2Cracks;
  [SerializeField] private Sprite objetivoHit;
  [SerializeField] private Sprite objetivo2Hit;

  [Header("GameManager")]
  [SerializeField] private JuegoController gameManager;
  
  private Vector2 posInicial = new Vector2(0f, -2.56f);
  private Vector2 posFinal = Vector2.zero;
  private float mostrarDuracion = 0.5f;
  private float duracion = 1f;

  private SpriteRenderer spriteRenderer;
  private Animator animator;
  private BoxCollider2D boxCollider2D;
  private Vector2 boxOffset;
  private Vector2 boxSize;
  private Vector2 boxOffsetHidden;
  private Vector2 boxSizeHidden;
  
  private bool golpeable = true;
  public enum TipoObjetivo { Normal, Olla, Bomba };
  private TipoObjetivo tipoObjetivo;
  private float objetivo2Rate = 0.25f;
  private float bombasRate;
  private int vidas;
  private int objetivoIndex;

  private IEnumerator ShowHide(Vector2 start, Vector2 end) {
    transform.localPosition = start;
    
    float tiempo = 0f;
    while (tiempo < mostrarDuracion) {
      transform.localPosition = Vector2.Lerp(start, end, tiempo / mostrarDuracion);
      boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, tiempo / mostrarDuracion);
      boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, tiempo / mostrarDuracion);
      tiempo += Time.deltaTime;
      yield return null;
    }
    
    transform.localPosition = end;
    boxCollider2D.offset = boxOffset;
    boxCollider2D.size = boxSize;
    
    yield return new WaitForSeconds(duracion);
    
    tiempo = 0f;
    while (tiempo < mostrarDuracion) {
      transform.localPosition = Vector2.Lerp(end, start, tiempo / mostrarDuracion);
      boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, tiempo / mostrarDuracion);
      boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, tiempo / mostrarDuracion);
      tiempo += Time.deltaTime;
      yield return null;
    }
    transform.localPosition = start;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;
    
    if (golpeable) {
      golpeable = false;
      gameManager.Missed(objetivoIndex, tipoObjetivo != TipoObjetivo.Bomba);
    }
  }
  public void Hide() {
    transform.localPosition = posInicial;
    boxCollider2D.offset = boxOffsetHidden;
    boxCollider2D.size = boxSizeHidden;
  }
  private IEnumerator QuickHide() {
    yield return new WaitForSeconds(0.25f);
    if (!golpeable) {
      Hide();
    }
  }
  private void OnMouseDown() {
    if (golpeable) {
      switch (tipoObjetivo) {
        case TipoObjetivo.Normal:
          spriteRenderer.sprite = objetivoHit;
          gameManager.AddScore(objetivoIndex);
          StopAllCoroutines();
          StartCoroutine(QuickHide());
          golpeable = false;
          break;
        case TipoObjetivo.Olla:
          if (vidas == 2) {
            spriteRenderer.sprite = objetivo2Cracks;
            vidas--;
          } else {
            spriteRenderer.sprite = objetivo2Hit;
            gameManager.AddScore(objetivoIndex);
            StopAllCoroutines();
            StartCoroutine(QuickHide());
            golpeable = false;
          }
          break;
        case TipoObjetivo.Bomba:
          gameManager.GameOver(1);
          break;
      }
    }
  }
  private void CreateNext() {
    float random = Random.Range(0f, 1f);
    if (random < bombasRate) {
      tipoObjetivo = TipoObjetivo.Bomba;
      animator.enabled = true;
    } else {
      animator.enabled = false;
      random = Random.Range(0f, 1f);
      if (random < objetivo2Rate) {
        tipoObjetivo = TipoObjetivo.Olla;
        spriteRenderer.sprite = objetivo2;
        vidas = 2;
      } else {
        tipoObjetivo = TipoObjetivo.Normal;
        spriteRenderer.sprite = objetivo;
        vidas = 1;
      }
    }
    golpeable = true;
  }
  private void SetLevel(int level) {
    
    bombasRate = Mathf.Min(level * 0.025f, 0.25f);  //incremento de la dificultad
    
    objetivo2Rate = Mathf.Min(level * 0.025f, 1f);  //incremento de la dificultad
    float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
    float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
    duracion = Random.Range(durationMin, durationMax);
  }

  private void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
    boxCollider2D = GetComponent<BoxCollider2D>();
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
  public void SetIndex(int index) {
    objetivoIndex = index;
  }
  public void StopGame() {
    golpeable = false;
    StopAllCoroutines();
  }
}
