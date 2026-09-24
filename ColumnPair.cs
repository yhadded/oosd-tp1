using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ColumnPair : MonoBehaviour
{
    [SerializeField] private SpriteRenderer topColumn;
    [SerializeField] private SpriteRenderer bottomColumn;
    [Tooltip("Trigger that sits in the gap and gives the point when the bird leaves it.")]
    [SerializeField] private BoxCollider2D scoreZone;
    [Tooltip("World X under which the pair is off-screen and gets destroyed.")]
    [SerializeField] private float destroyX = -12f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Layout(float gap)
    {
        float centerY = transform.position.y;

        MoveEdgeTo(topColumn, centerY + gap * 0.5f, alignBottomEdge: true);
        MoveEdgeTo(bottomColumn, centerY - gap * 0.5f, alignBottomEdge: false);

        if (scoreZone)
        {
            scoreZone.isTrigger = true;
            scoreZone.offset = Vector2.zero;
            scoreZone.size = new Vector2(scoreZone.size.x, gap);
        }
    }

    private static void MoveEdgeTo(SpriteRenderer sr, float targetY, bool alignBottomEdge)
    {
        if (!sr) return;
        float edge = alignBottomEdge ? sr.bounds.min.y : sr.bounds.max.y;
        sr.transform.position += Vector3.up * (targetY - edge);
    }

    private void FixedUpdate()
    {
        float speed = GameManager.Instance ? GameManager.Instance.CurrentSpeed : 0f;
        rb.linearVelocity = Vector2.left * speed;
    }

    private void Update()
    {
        if (transform.position.x < destroyX) Destroy(gameObject);
    }
}
