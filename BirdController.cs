using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : MonoBehaviour
{
    [Header("Flight")]
    [Tooltip("Upward speed given on each flap. Setting velocity (not adding force) makes every flap feel identical.")]
    [SerializeField] private float flapVelocity = 6f;
    [Tooltip("Gravity multiplier while playing. Higher = heavier, snappier bird.")]
    [SerializeField] private float gravityScale = 2.2f;
    [Tooltip("Clamp on fall speed so the bird never drops uncontrollably fast.")]
    [SerializeField] private float maxFallSpeed = 10f;

    [Header("Tilt (visual only)")]
    [SerializeField] private Transform visual;
    [SerializeField] private float maxUpAngle = 30f;
    [SerializeField] private float maxDownAngle = -90f;
    [SerializeField] private float tiltSmoothing = 8f;

    [Header("Ready state (idle hover before first flap)")]
    [SerializeField] private float hoverAmplitude = 0.15f;
    [SerializeField] private float hoverFrequency = 2f;

    [Header("Animation")]
    [Tooltip("Animator on the Visual child. Needs a bool parameter named 'IsFlying'.")]
    [SerializeField] private Animator animator;

    [Header("Audio / FX (bonus)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flapClip;
    [SerializeField] private ParticleSystem flapParticles;
    [SerializeField] private ParticleSystem hitParticles;

    private static readonly int IsFlyingHash = Animator.StringToHash("IsFlying");

    private Rigidbody2D rb;
    private float startY;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.freezeRotation = true;
        SetFlyingAnim(true);
    }

    private void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (!isDead && FlapPressed())
        {
            if (gm.State == GameState.Ready)
            {
                gm.StartGame();
                rb.gravityScale = gravityScale;
            }
            if (gm.State == GameState.Playing) Flap();
        }

        if (gm.State == GameState.Ready)
        {
            float y = startY + Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            rb.position = new Vector2(rb.position.x, y);
        }

        UpdateTilt();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    private static bool FlapPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return key || click || touch;
    }

    private void Flap()
    {
        rb.linearVelocity = new Vector2(0f, flapVelocity);

        if (audioSource && flapClip) audioSource.PlayOneShot(flapClip);
        if (flapParticles) flapParticles.Play();
    }

    private void UpdateTilt()
    {
        if (!visual) return;

        float target;
        if (GameManager.Instance.State == GameState.Ready)
            target = 0f;
        else
            target = Mathf.Lerp(maxDownAngle, maxUpAngle, Mathf.InverseLerp(-maxFallSpeed, flapVelocity, rb.linearVelocity.y));

        float current = visual.localEulerAngles.z;
        float z = Mathf.LerpAngle(current, target, tiltSmoothing * Time.deltaTime);
        visual.localRotation = Quaternion.Euler(0f, 0f, z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ceiling")) return;

        if (isDead)
        {
            SetFlyingAnim(false);
            return;
        }
        Die();
    }

    private void Die()
    {
        isDead = true;
        if (hitParticles) hitParticles.Play();
        GameManager.Instance?.GameOver();

        SetFlyingAnim(false);
    }

    private void SetFlyingAnim(bool flying)
    {
        if (animator) animator.SetBool(IsFlyingHash, flying);
    }
}
